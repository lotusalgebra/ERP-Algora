using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.HR;
using Algora.Erp.Domain.Entities.Payroll;
using Algora.Erp.Domain.Enums;
using Algora.Erp.Web.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Payroll.Runs;

[Authorize(Policy = "CanViewPayroll")]
[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalRuns { get; set; }
    public int PendingRuns { get; set; }
    public decimal TotalPaidOut { get; set; }
    public int EmployeesProcessed { get; set; }

    public async Task OnGetAsync()
    {
        TotalRuns = await _context.PayrollRuns.CountAsync();
        PendingRuns = await _context.PayrollRuns.CountAsync(r =>
            r.Status != PayrollRunStatus.Paid && r.Status != PayrollRunStatus.Cancelled);
        TotalPaidOut = await _context.PayrollRuns
            .Where(r => r.Status == PayrollRunStatus.Paid)
            .SumAsync(r => r.TotalNetPay);
        EmployeesProcessed = await _context.Payslips
            .Where(p => p.Status == PayslipStatus.Paid)
            .Select(p => p.EmployeeId)
            .Distinct()
            .CountAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.PayrollRuns
            .Include(r => r.Payslips)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(r =>
                r.RunNumber.ToLower().Contains(search) ||
                r.Name.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<PayrollRunStatus>(statusFilter, out var status))
        {
            query = query.Where(r => r.Status == status);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var runs = await query
            .OrderByDescending(r => r.PeriodStart)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_PayrollRunsTableRows", new PayrollRunsTableViewModel
        {
            Runs = runs,
            Pagination = new PaginationViewModel
            {
                Page = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                PageUrl = "/Payroll/Runs",
                HxTarget = "#runsTableBody",
                HxInclude = "#searchInput,#statusFilter,#pageSizeSelect"
            }
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        return Partial("_PayrollRunForm", new PayrollRunFormViewModel { IsEdit = false });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var run = await _context.PayrollRuns.FindAsync(id);
        if (run == null)
            return NotFound();

        return Partial("_PayrollRunForm", new PayrollRunFormViewModel
        {
            IsEdit = true,
            PayrollRun = run
        });
    }

    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
    {
        var run = await _context.PayrollRuns
            .Include(r => r.Payslips)
                .ThenInclude(p => p.Employee)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (run == null)
            return NotFound();

        return Partial("_PayrollRunDetails", run);
    }

    public async Task<IActionResult> OnPostAsync(PayrollRunFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        PayrollRun? run;

        if (input.Id.HasValue)
        {
            run = await _context.PayrollRuns.FindAsync(input.Id.Value);
            if (run == null)
                return NotFound();
        }
        else
        {
            run = new PayrollRun
            {
                Id = Guid.NewGuid(),
                RunNumber = await GenerateRunNumberAsync(),
                Status = PayrollRunStatus.Draft
            };
            _context.PayrollRuns.Add(run);
        }

        run.Name = input.Name;
        run.PeriodStart = input.PeriodStart;
        run.PeriodEnd = input.PeriodEnd;
        run.PayDate = input.PayDate;
        run.PayFrequency = input.PayFrequency;
        run.Currency = input.Currency;
        run.Notes = input.Notes;

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    public async Task<IActionResult> OnPostProcessAsync(Guid id)
    {
        var run = await _context.PayrollRuns
            .Include(r => r.Payslips)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (run == null)
            return NotFound();

        if (run.Status != PayrollRunStatus.Draft)
            return BadRequest("Only draft payroll runs can be processed.");

        // Get all active employees
        var employees = await _context.Employees
            .Where(e => e.EmploymentStatus == EmploymentStatus.Active && e.BaseSalary.HasValue)
            .ToListAsync();

        // Get salary components
        var components = await _context.SalaryComponents
            .Where(c => c.IsActive && c.IsRecurring)
            .OrderBy(c => c.ComponentType).ThenBy(c => c.SortOrder)
            .ToListAsync();

        run.Status = PayrollRunStatus.Processing;
        int slipNumber = 1;

        foreach (var employee in employees)
        {
            var payslip = new Payslip
            {
                Id = Guid.NewGuid(),
                PayslipNumber = $"PS{run.RunNumber.Substring(2)}-{slipNumber:D4}",
                PayrollRunId = run.Id,
                EmployeeId = employee.Id,
                PeriodStart = run.PeriodStart,
                PeriodEnd = run.PeriodEnd,
                PayDate = run.PayDate,
                Status = PayslipStatus.Processed,
                BasicSalary = employee.BaseSalary ?? 0,
                WorkingDays = 22, // Default working days
                DaysWorked = 22,
                BankAccountNumber = employee.BankAccountNumber,
                BankName = employee.BankName,
                PaymentMethod = "Bank Transfer"
            };

            decimal totalEarnings = 0;
            decimal totalDeductions = 0;
            decimal taxableAmount = 0;
            int lineOrder = 1;

            foreach (var component in components)
            {
                decimal amount = 0;

                switch (component.CalculationType)
                {
                    case CalculationType.Fixed:
                        amount = component.DefaultValue;
                        break;
                    case CalculationType.PercentageOfBasic:
                        amount = payslip.BasicSalary * component.DefaultValue / 100;
                        break;
                    case CalculationType.PercentageOfGross:
                        amount = payslip.BasicSalary * component.DefaultValue / 100; // Simplified
                        break;
                }

                if (component.ComponentType == SalaryComponentType.Earning ||
                    component.ComponentType == SalaryComponentType.Reimbursement)
                {
                    totalEarnings += amount;
                    if (component.IsTaxable)
                        taxableAmount += amount;
                }
                else
                {
                    totalDeductions += amount;
                }

                var line = new PayslipLine
                {
                    Id = Guid.NewGuid(),
                    PayslipId = payslip.Id,
                    SalaryComponentId = component.Id,
                    ComponentCode = component.Code,
                    ComponentName = component.Name,
                    ComponentType = component.ComponentType,
                    CalculationType = component.CalculationType,
                    Value = component.DefaultValue,
                    Amount = amount,
                    IsTaxable = component.IsTaxable,
                    SortOrder = lineOrder++
                };

                _context.PayslipLines.Add(line);
            }

            payslip.TotalEarnings = totalEarnings;
            payslip.TotalDeductions = totalDeductions;
            payslip.TaxableAmount = taxableAmount;
            payslip.GrossPay = totalEarnings;
            payslip.NetPay = totalEarnings - totalDeductions;

            _context.Payslips.Add(payslip);
            slipNumber++;
        }

        run.Status = PayrollRunStatus.Processed;
        run.ProcessedAt = DateTime.UtcNow;
        run.EmployeeCount = employees.Count;
        run.TotalGrossPay = await _context.Payslips.Where(p => p.PayrollRunId == run.Id).SumAsync(p => p.GrossPay);
        run.TotalDeductions = await _context.Payslips.Where(p => p.PayrollRunId == run.Id).SumAsync(p => p.TotalDeductions);
        run.TotalNetPay = await _context.Payslips.Where(p => p.PayrollRunId == run.Id).SumAsync(p => p.NetPay);

        await _context.SaveChangesAsync();

        // Recalculate totals after all payslips are added
        var payslips = await _context.Payslips.Where(p => p.PayrollRunId == run.Id).ToListAsync();
        run.TotalGrossPay = payslips.Sum(p => p.GrossPay);
        run.TotalDeductions = payslips.Sum(p => p.TotalDeductions);
        run.TotalNetPay = payslips.Sum(p => p.NetPay);

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, PayrollRunStatus status)
    {
        var run = await _context.PayrollRuns
            .Include(r => r.Payslips)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (run == null)
            return NotFound();

        run.Status = status;

        if (status == PayrollRunStatus.Approved)
        {
            run.ApprovedAt = DateTime.UtcNow;
            foreach (var payslip in run.Payslips)
            {
                payslip.Status = PayslipStatus.Approved;
            }
        }
        else if (status == PayrollRunStatus.Paid)
        {
            foreach (var payslip in run.Payslips)
            {
                payslip.Status = PayslipStatus.Paid;
                payslip.PaidAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var run = await _context.PayrollRuns
            .Include(r => r.Payslips)
                .ThenInclude(p => p.Lines)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (run == null)
            return NotFound();

        if (run.Status != PayrollRunStatus.Draft)
        {
            return BadRequest("Only draft payroll runs can be deleted.");
        }

        _context.PayrollRuns.Remove(run);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    private async Task<string> GenerateRunNumberAsync()
    {
        var currentMonth = DateTime.UtcNow.ToString("yyyyMM");
        var lastRun = await _context.PayrollRuns
            .IgnoreQueryFilters()
            .Where(r => r.RunNumber.StartsWith($"PR{currentMonth}"))
            .OrderByDescending(r => r.RunNumber)
            .FirstOrDefaultAsync();

        if (lastRun == null)
            return $"PR{currentMonth}001";

        var lastNumber = int.Parse(lastRun.RunNumber.Substring(8));
        return $"PR{currentMonth}{(lastNumber + 1):D3}";
    }
}

public class PayrollRunsTableViewModel
{
    public List<PayrollRun> Runs { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
}

public class PayrollRunFormViewModel
{
    public bool IsEdit { get; set; }
    public PayrollRun? PayrollRun { get; set; }
}

public class PayrollRunFormInput
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    public DateTime PeriodEnd { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
    public DateTime PayDate { get; set; } = DateTime.Today.AddDays(5);
    public PayFrequency PayFrequency { get; set; } = PayFrequency.Monthly;
    public string Currency { get; set; } = "USD";
    public string? Notes { get; set; }
}
