-- Algora ERP - Ecommerce Module Tables
-- Run this script to create all eCommerce-related tables

SET QUOTED_IDENTIFIER ON;

-- =============================================
-- STORES
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Stores')
BEGIN
    CREATE TABLE Stores (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(200) NOT NULL,
        Tagline NVARCHAR(500),
        LogoUrl NVARCHAR(500),
        FaviconUrl NVARCHAR(500),
        Email NVARCHAR(255),
        Phone NVARCHAR(50),
        [Address] NVARCHAR(500),
        FacebookUrl NVARCHAR(500),
        TwitterUrl NVARCHAR(500),
        InstagramUrl NVARCHAR(500),
        Currency NVARCHAR(3) DEFAULT 'USD',
        CurrencySymbol NVARCHAR(10) DEFAULT '$',
        EnableTax BIT DEFAULT 0,
        TaxRate DECIMAL(5,2) DEFAULT 0,
        EnableReviews BIT DEFAULT 1,
        EnableWishlist BIT DEFAULT 1,
        EnableGuestCheckout BIT DEFAULT 1,
        MinOrderAmount INT DEFAULT 0,
        FreeShippingThreshold INT DEFAULT 0,
        MetaTitle NVARCHAR(200),
        MetaDescription NVARCHAR(500),
        MetaKeywords NVARCHAR(500),
        IsActive BIT DEFAULT 1,
        -- Audit fields
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER
    );
    PRINT 'Created table: Stores';
END

-- =============================================
-- WEB CATEGORIES
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WebCategories')
BEGIN
    CREATE TABLE WebCategories (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(200) NOT NULL,
        Slug NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(1000),
        ImageUrl NVARCHAR(500),
        ParentId UNIQUEIDENTIFIER,
        SortOrder INT DEFAULT 0,
        IsActive BIT DEFAULT 1,
        ShowInMenu BIT DEFAULT 1,
        MetaTitle NVARCHAR(200),
        MetaDescription NVARCHAR(500),
        -- Audit fields
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,
        CONSTRAINT FK_WebCategories_Parent FOREIGN KEY (ParentId) REFERENCES WebCategories(Id)
    );
    CREATE UNIQUE INDEX IX_WebCategories_Slug ON WebCategories(Slug) WHERE IsDeleted = 0;
    PRINT 'Created table: WebCategories';
END

-- =============================================
-- ECOMMERCE PRODUCTS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EcommerceProducts')
BEGIN
    CREATE TABLE EcommerceProducts (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(300) NOT NULL,
        Slug NVARCHAR(300) NOT NULL,
        Sku NVARCHAR(100),
        ShortDescription NVARCHAR(500),
        [Description] NVARCHAR(MAX),
        -- Pricing
        Price DECIMAL(18,2) NOT NULL DEFAULT 0,
        CompareAtPrice DECIMAL(18,2),
        CostPrice DECIMAL(18,2),
        -- Links
        InventoryProductId UNIQUEIDENTIFIER,
        CategoryId UNIQUEIDENTIFIER,
        -- Brand
        Brand NVARCHAR(100),
        Vendor NVARCHAR(100),
        -- Physical
        Weight DECIMAL(10,3),
        WeightUnit NVARCHAR(10) DEFAULT 'kg',
        -- Status
        [Status] INT DEFAULT 0, -- 0=Draft, 1=Active, 2=Archived
        IsFeatured BIT DEFAULT 0,
        IsNewArrival BIT DEFAULT 0,
        IsBestSeller BIT DEFAULT 0,
        -- Stock
        TrackInventory BIT DEFAULT 1,
        StockQuantity INT DEFAULT 0,
        LowStockThreshold INT DEFAULT 5,
        AllowBackorder BIT DEFAULT 0,
        -- SEO
        MetaTitle NVARCHAR(200),
        MetaDescription NVARCHAR(500),
        Tags NVARCHAR(500),
        -- Stats
        ViewCount INT DEFAULT 0,
        SalesCount INT DEFAULT 0,
        AverageRating DECIMAL(3,2) DEFAULT 0,
        ReviewCount INT DEFAULT 0,
        -- Audit fields
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,
        CONSTRAINT FK_EcommerceProducts_Category FOREIGN KEY (CategoryId) REFERENCES WebCategories(Id)
        -- Note: FK to Inventory Products table removed - optional integration
    );
    CREATE UNIQUE INDEX IX_EcommerceProducts_Slug ON EcommerceProducts(Slug) WHERE IsDeleted = 0;
    CREATE INDEX IX_EcommerceProducts_Sku ON EcommerceProducts(Sku);
    CREATE INDEX IX_EcommerceProducts_Status ON EcommerceProducts([Status]);
    PRINT 'Created table: EcommerceProducts';
END

-- =============================================
-- PRODUCT IMAGES (BaseEntity)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductImages')
BEGIN
    CREATE TABLE ProductImages (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ProductId UNIQUEIDENTIFIER NOT NULL,
        Url NVARCHAR(500) NOT NULL,
        AltText NVARCHAR(200),
        SortOrder INT DEFAULT 0,
        IsPrimary BIT DEFAULT 0,
        CONSTRAINT FK_ProductImages_Product FOREIGN KEY (ProductId) REFERENCES EcommerceProducts(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_ProductImages_ProductId ON ProductImages(ProductId);
    PRINT 'Created table: ProductImages';
END

-- =============================================
-- PRODUCT VARIANTS (BaseEntity)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductVariants')
BEGIN
    CREATE TABLE ProductVariants (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ProductId UNIQUEIDENTIFIER NOT NULL,
        Sku NVARCHAR(100),
        Name NVARCHAR(200),
        -- Options
        Option1Name NVARCHAR(50),
        Option1Value NVARCHAR(100),
        Option2Name NVARCHAR(50),
        Option2Value NVARCHAR(100),
        Option3Name NVARCHAR(50),
        Option3Value NVARCHAR(100),
        -- Pricing
        Price DECIMAL(18,2) NOT NULL DEFAULT 0,
        CompareAtPrice DECIMAL(18,2),
        -- Stock
        StockQuantity INT DEFAULT 0,
        TrackInventory BIT DEFAULT 1,
        -- Physical
        Weight DECIMAL(10,3),
        -- Image
        ImageUrl NVARCHAR(500),
        IsActive BIT DEFAULT 1,
        CONSTRAINT FK_ProductVariants_Product FOREIGN KEY (ProductId) REFERENCES EcommerceProducts(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_ProductVariants_ProductId ON ProductVariants(ProductId);
    CREATE INDEX IX_ProductVariants_Sku ON ProductVariants(Sku);
    PRINT 'Created table: ProductVariants';
END

-- =============================================
-- WEB CUSTOMERS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WebCustomers')
BEGIN
    CREATE TABLE WebCustomers (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Email NVARCHAR(255) NOT NULL,
        PasswordHash NVARCHAR(500),
        FirstName NVARCHAR(100),
        LastName NVARCHAR(100),
        Phone NVARCHAR(50),
        [Address] NVARCHAR(500),
        City NVARCHAR(100),
        [State] NVARCHAR(100),
        PostalCode NVARCHAR(20),
        Country NVARCHAR(100),
        -- Stats
        OrderCount INT DEFAULT 0,
        TotalSpent DECIMAL(18,2) DEFAULT 0,
        LastOrderAt DATETIME2,
        -- Status
        IsActive BIT DEFAULT 1,
        EmailVerified BIT DEFAULT 0,
        LastLoginAt DATETIME2,
        -- Marketing
        AcceptsMarketing BIT DEFAULT 0,
        Tags NVARCHAR(500),
        Notes NVARCHAR(1000),
        -- Audit fields
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER
    );
    CREATE UNIQUE INDEX IX_WebCustomers_Email ON WebCustomers(Email) WHERE IsDeleted = 0;
    PRINT 'Created table: WebCustomers';
END

-- =============================================
-- CUSTOMER ADDRESSES (BaseEntity)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CustomerAddresses')
BEGIN
    CREATE TABLE CustomerAddresses (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CustomerId UNIQUEIDENTIFIER NOT NULL,
        Label NVARCHAR(50) DEFAULT 'Home',
        FirstName NVARCHAR(100),
        LastName NVARCHAR(100),
        Company NVARCHAR(200),
        Address1 NVARCHAR(500) NOT NULL,
        Address2 NVARCHAR(500),
        City NVARCHAR(100) NOT NULL,
        [State] NVARCHAR(100),
        Country NVARCHAR(100) NOT NULL,
        PostalCode NVARCHAR(20),
        Phone NVARCHAR(50),
        IsDefault BIT DEFAULT 0,
        IsDefaultBilling BIT DEFAULT 0,
        IsDefaultShipping BIT DEFAULT 0,
        CONSTRAINT FK_CustomerAddresses_Customer FOREIGN KEY (CustomerId) REFERENCES WebCustomers(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_CustomerAddresses_CustomerId ON CustomerAddresses(CustomerId);
    PRINT 'Created table: CustomerAddresses';
END

-- =============================================
-- COUPONS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Coupons')
BEGIN
    CREATE TABLE Coupons (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Code NVARCHAR(50) NOT NULL,
        [Description] NVARCHAR(500),
        DiscountType INT DEFAULT 0, -- 0=Percentage, 1=FixedAmount, 2=BuyXGetY
        DiscountValue DECIMAL(18,2) NOT NULL,
        MaxDiscountAmount DECIMAL(18,2),
        MinOrderAmount DECIMAL(18,2),
        MinQuantity INT,
        UsageLimit INT,
        UsageLimitPerCustomer INT DEFAULT 1,
        TimesUsed INT DEFAULT 0,
        StartsAt DATETIME2,
        ExpiresAt DATETIME2,
        FirstOrderOnly BIT DEFAULT 0,
        ApplicableProductIds NVARCHAR(2000),
        ApplicableCategoryIds NVARCHAR(2000),
        ExcludedProductIds NVARCHAR(2000),
        CustomerIds NVARCHAR(2000),
        FreeShipping BIT DEFAULT 0,
        IsActive BIT DEFAULT 1,
        -- Audit fields
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER
    );
    CREATE UNIQUE INDEX IX_Coupons_Code ON Coupons(Code) WHERE IsDeleted = 0;
    PRINT 'Created table: Coupons';
END

-- =============================================
-- SHOPPING CARTS (BaseEntity)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ShoppingCarts')
BEGIN
    CREATE TABLE ShoppingCarts (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CustomerId UNIQUEIDENTIFIER,
        SessionId NVARCHAR(100),
        CouponCode NVARCHAR(50),
        DiscountAmount DECIMAL(18,2) DEFAULT 0,
        Subtotal DECIMAL(18,2) DEFAULT 0,
        TaxAmount DECIMAL(18,2) DEFAULT 0,
        ShippingAmount DECIMAL(18,2) DEFAULT 0,
        Total DECIMAL(18,2) DEFAULT 0,
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2,
        AbandonedAt DATETIME2,
        IsAbandoned BIT DEFAULT 0,
        RemindersSent INT DEFAULT 0,
        LastReminderAt DATETIME2,
        CONSTRAINT FK_ShoppingCarts_Customer FOREIGN KEY (CustomerId) REFERENCES WebCustomers(Id)
    );
    CREATE INDEX IX_ShoppingCarts_CustomerId ON ShoppingCarts(CustomerId);
    CREATE INDEX IX_ShoppingCarts_SessionId ON ShoppingCarts(SessionId);
    PRINT 'Created table: ShoppingCarts';
END

-- =============================================
-- CART ITEMS (BaseEntity)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CartItems')
BEGIN
    CREATE TABLE CartItems (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CartId UNIQUEIDENTIFIER NOT NULL,
        ProductId UNIQUEIDENTIFIER,
        VariantId UNIQUEIDENTIFIER,
        ProductName NVARCHAR(300),
        VariantName NVARCHAR(200),
        Sku NVARCHAR(100),
        ImageUrl NVARCHAR(500),
        Quantity INT NOT NULL DEFAULT 1,
        UnitPrice DECIMAL(18,2) NOT NULL,
        LineTotal DECIMAL(18,2) NOT NULL,
        AddedAt DATETIME2 DEFAULT GETUTCDATE(),
        CONSTRAINT FK_CartItems_Cart FOREIGN KEY (CartId) REFERENCES ShoppingCarts(Id) ON DELETE CASCADE,
        CONSTRAINT FK_CartItems_Product FOREIGN KEY (ProductId) REFERENCES EcommerceProducts(Id),
        CONSTRAINT FK_CartItems_Variant FOREIGN KEY (VariantId) REFERENCES ProductVariants(Id)
    );
    CREATE INDEX IX_CartItems_CartId ON CartItems(CartId);
    PRINT 'Created table: CartItems';
END

-- =============================================
-- WEB ORDERS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WebOrders')
BEGIN
    CREATE TABLE WebOrders (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        OrderNumber NVARCHAR(50) NOT NULL,
        CustomerId UNIQUEIDENTIFIER,
        CustomerEmail NVARCHAR(255),
        CustomerPhone NVARCHAR(50),
        -- Billing Address
        BillingFirstName NVARCHAR(100),
        BillingLastName NVARCHAR(100),
        BillingCompany NVARCHAR(200),
        BillingAddress1 NVARCHAR(500),
        BillingAddress2 NVARCHAR(500),
        BillingCity NVARCHAR(100),
        BillingState NVARCHAR(100),
        BillingPostalCode NVARCHAR(20),
        BillingCountry NVARCHAR(100),
        -- Shipping Address
        ShippingFirstName NVARCHAR(100),
        ShippingLastName NVARCHAR(100),
        ShippingCompany NVARCHAR(200),
        ShippingAddress1 NVARCHAR(500),
        ShippingAddress2 NVARCHAR(500),
        ShippingCity NVARCHAR(100),
        ShippingState NVARCHAR(100),
        ShippingPostalCode NVARCHAR(20),
        ShippingCountry NVARCHAR(100),
        -- Totals
        Subtotal DECIMAL(18,2) DEFAULT 0,
        DiscountAmount DECIMAL(18,2) DEFAULT 0,
        CouponCode NVARCHAR(50),
        ShippingAmount DECIMAL(18,2) DEFAULT 0,
        ShippingMethod NVARCHAR(100),
        ShippingCarrier NVARCHAR(100),
        TaxAmount DECIMAL(18,2) DEFAULT 0,
        Total DECIMAL(18,2) DEFAULT 0,
        -- Payment
        PaymentMethod NVARCHAR(100),
        PaymentTransactionId NVARCHAR(100),
        PaymentStatus INT DEFAULT 0,
        PaidAt DATETIME2,
        -- Status
        [Status] INT DEFAULT 0, -- WebOrderStatus
        FulfillmentStatus INT DEFAULT 0,
        -- Shipping
        TrackingNumber NVARCHAR(100),
        TrackingUrl NVARCHAR(500),
        ShippedAt DATETIME2,
        DeliveredAt DATETIME2,
        -- Notes
        CustomerNotes NVARCHAR(1000),
        InternalNotes NVARCHAR(1000),
        -- Refund
        RefundedAmount DECIMAL(18,2) DEFAULT 0,
        RefundReason NVARCHAR(1000),
        -- Meta
        IpAddress NVARCHAR(50),
        UserAgent NVARCHAR(500),
        Source NVARCHAR(50),
        -- Audit fields
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,
        CONSTRAINT FK_WebOrders_Customer FOREIGN KEY (CustomerId) REFERENCES WebCustomers(Id)
    );
    CREATE UNIQUE INDEX IX_WebOrders_OrderNumber ON WebOrders(OrderNumber) WHERE IsDeleted = 0;
    CREATE INDEX IX_WebOrders_CustomerId ON WebOrders(CustomerId);
    CREATE INDEX IX_WebOrders_Status ON WebOrders([Status]);
    PRINT 'Created table: WebOrders';
END

-- =============================================
-- WEB ORDER ITEMS (BaseEntity)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WebOrderItems')
BEGIN
    CREATE TABLE WebOrderItems (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        OrderId UNIQUEIDENTIFIER NOT NULL,
        ProductId UNIQUEIDENTIFIER,
        VariantId UNIQUEIDENTIFIER,
        ProductName NVARCHAR(300) NOT NULL,
        VariantName NVARCHAR(200),
        Sku NVARCHAR(100),
        ImageUrl NVARCHAR(500),
        Quantity INT NOT NULL DEFAULT 1,
        UnitPrice DECIMAL(18,2) NOT NULL,
        DiscountAmount DECIMAL(18,2) DEFAULT 0,
        TaxAmount DECIMAL(18,2) DEFAULT 0,
        LineTotal DECIMAL(18,2) NOT NULL,
        -- Fulfillment
        FulfilledQuantity INT DEFAULT 0,
        RefundedQuantity INT DEFAULT 0,
        CONSTRAINT FK_WebOrderItems_Order FOREIGN KEY (OrderId) REFERENCES WebOrders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_WebOrderItems_Product FOREIGN KEY (ProductId) REFERENCES EcommerceProducts(Id),
        CONSTRAINT FK_WebOrderItems_Variant FOREIGN KEY (VariantId) REFERENCES ProductVariants(Id)
    );
    CREATE INDEX IX_WebOrderItems_OrderId ON WebOrderItems(OrderId);
    PRINT 'Created table: WebOrderItems';
END

-- =============================================
-- PRODUCT REVIEWS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductReviews')
BEGIN
    CREATE TABLE ProductReviews (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ProductId UNIQUEIDENTIFIER NOT NULL,
        CustomerId UNIQUEIDENTIFIER,
        OrderId UNIQUEIDENTIFIER,
        CustomerName NVARCHAR(100),
        CustomerEmail NVARCHAR(255),
        Rating INT NOT NULL,
        Title NVARCHAR(200),
        Content NVARCHAR(MAX),
        Pros NVARCHAR(1000),
        Cons NVARCHAR(1000),
        ImageUrls NVARCHAR(2000),
        [Status] INT DEFAULT 0, -- 0=Pending, 1=Approved, 2=Rejected, 3=Flagged
        ApprovedAt DATETIME2,
        AdminResponse NVARCHAR(2000),
        AdminRespondedAt DATETIME2,
        HelpfulVotes INT DEFAULT 0,
        UnhelpfulVotes INT DEFAULT 0,
        IsVerifiedPurchase BIT DEFAULT 0,
        -- Audit fields
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,
        CONSTRAINT FK_ProductReviews_Product FOREIGN KEY (ProductId) REFERENCES EcommerceProducts(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ProductReviews_Customer FOREIGN KEY (CustomerId) REFERENCES WebCustomers(Id)
    );
    CREATE INDEX IX_ProductReviews_ProductId ON ProductReviews(ProductId);
    CREATE INDEX IX_ProductReviews_CustomerId ON ProductReviews(CustomerId);
    CREATE INDEX IX_ProductReviews_Status ON ProductReviews([Status]);
    PRINT 'Created table: ProductReviews';
END

-- =============================================
-- WISHLIST ITEMS (BaseEntity)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WishlistItems')
BEGIN
    CREATE TABLE WishlistItems (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CustomerId UNIQUEIDENTIFIER NOT NULL,
        ProductId UNIQUEIDENTIFIER NOT NULL,
        VariantId UNIQUEIDENTIFIER,
        AddedAt DATETIME2 DEFAULT GETUTCDATE(),
        Notes NVARCHAR(500),
        CONSTRAINT FK_WishlistItems_Customer FOREIGN KEY (CustomerId) REFERENCES WebCustomers(Id) ON DELETE CASCADE,
        CONSTRAINT FK_WishlistItems_Product FOREIGN KEY (ProductId) REFERENCES EcommerceProducts(Id) ON DELETE CASCADE,
        CONSTRAINT FK_WishlistItems_Variant FOREIGN KEY (VariantId) REFERENCES ProductVariants(Id)
    );
    CREATE UNIQUE INDEX IX_WishlistItems_CustomerProduct ON WishlistItems(CustomerId, ProductId);
    PRINT 'Created table: WishlistItems';
END

-- =============================================
-- SHIPPING METHODS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ShippingMethods')
BEGIN
    CREATE TABLE ShippingMethods (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500),
        Carrier NVARCHAR(50),
        RateType INT DEFAULT 0, -- 0=FlatRate, 1=WeightBased, 2=PriceBased, 3=Free, 4=Calculated
        Rate DECIMAL(18,2) DEFAULT 0,
        FreeShippingThreshold DECIMAL(18,2),
        RatePerKg DECIMAL(18,2),
        MinWeight DECIMAL(10,3),
        MaxWeight DECIMAL(10,3),
        MinDeliveryDays INT,
        MaxDeliveryDays INT,
        AllowedCountries NVARCHAR(1000),
        ExcludedCountries NVARCHAR(1000),
        SortOrder INT DEFAULT 0,
        IsActive BIT DEFAULT 1,
        -- Audit fields
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER
    );
    PRINT 'Created table: ShippingMethods';
END

-- =============================================
-- WEB PAYMENT METHODS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WebPaymentMethods')
BEGIN
    CREATE TABLE WebPaymentMethods (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500),
        Instructions NVARCHAR(2000),
        Gateway INT DEFAULT 0, -- 0=Manual, 1=Stripe, 2=PayPal, 3=Square, 4=Razorpay, 5=COD
        ApiKey NVARCHAR(500),
        SecretKey NVARCHAR(500),
        WebhookSecret NVARCHAR(500),
        IsSandbox BIT DEFAULT 1,
        TransactionFeePercent DECIMAL(5,2) DEFAULT 0,
        TransactionFeeFixed DECIMAL(18,2) DEFAULT 0,
        MinAmount DECIMAL(18,2),
        MaxAmount DECIMAL(18,2),
        AllowedCountries NVARCHAR(1000),
        IconUrl NVARCHAR(500),
        SortOrder INT DEFAULT 0,
        IsActive BIT DEFAULT 1,
        -- Audit fields
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER
    );
    CREATE UNIQUE INDEX IX_WebPaymentMethods_Code ON WebPaymentMethods(Code) WHERE IsDeleted = 0;
    PRINT 'Created table: WebPaymentMethods';
END

-- =============================================
-- BANNERS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Banners')
BEGIN
    CREATE TABLE Banners (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Title NVARCHAR(200) NOT NULL,
        Subtitle NVARCHAR(500),
        ImageUrl NVARCHAR(500) NOT NULL,
        MobileImageUrl NVARCHAR(500),
        LinkUrl NVARCHAR(500),
        ButtonText NVARCHAR(50),
        Position INT DEFAULT 0, -- BannerPosition enum
        StartsAt DATETIME2,
        EndsAt DATETIME2,
        SortOrder INT DEFAULT 0,
        IsActive BIT DEFAULT 1,
        -- Stats
        Impressions INT DEFAULT 0,
        Clicks INT DEFAULT 0,
        -- Audit fields
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER
    );
    CREATE INDEX IX_Banners_Position ON Banners(Position);
    PRINT 'Created table: Banners';
END

-- =============================================
-- SEED DEFAULT DATA
-- =============================================

-- Seed default shipping methods
IF NOT EXISTS (SELECT * FROM ShippingMethods WHERE Name = 'Standard Shipping')
BEGIN
    INSERT INTO ShippingMethods (Id, Name, [Description], Carrier, RateType, Rate, MinDeliveryDays, MaxDeliveryDays, IsActive)
    VALUES
        (NEWID(), 'Standard Shipping', 'Standard delivery within 5-7 business days', 'Multiple', 0, 5.99, 5, 7, 1),
        (NEWID(), 'Express Shipping', 'Fast delivery within 2-3 business days', 'FedEx', 0, 14.99, 2, 3, 1),
        (NEWID(), 'Overnight Shipping', 'Next day delivery', 'FedEx', 0, 29.99, 1, 1, 1),
        (NEWID(), 'Free Shipping', 'Free standard shipping on qualifying orders', 'Multiple', 3, 0, 5, 7, 1);
    PRINT 'Seeded shipping methods';
END

-- Seed default payment methods
IF NOT EXISTS (SELECT * FROM WebPaymentMethods WHERE Code = 'COD')
BEGIN
    INSERT INTO WebPaymentMethods (Id, Code, Name, [Description], Gateway, IsActive, SortOrder)
    VALUES
        (NEWID(), 'COD', 'Cash on Delivery', 'Pay with cash when your order is delivered', 5, 1, 1),
        (NEWID(), 'STRIPE', 'Credit/Debit Card', 'Pay securely with your credit or debit card via Stripe', 1, 0, 2),
        (NEWID(), 'PAYPAL', 'PayPal', 'Pay with your PayPal account', 2, 0, 3);
    PRINT 'Seeded payment methods';
END

-- Seed default store
IF NOT EXISTS (SELECT * FROM Stores)
BEGIN
    INSERT INTO Stores (Id, Name, Tagline, Currency, CurrencySymbol, IsActive)
    VALUES (NEWID(), 'My Store', 'Welcome to our online store', 'USD', '$', 1);
    PRINT 'Seeded default store';
END

PRINT 'Ecommerce tables creation complete!';
