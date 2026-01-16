-- Fix User Passwords with BCrypt Hash
-- Run this on your tenant database to reset user passwords
-- Password: Password123!

-- BCrypt hash for 'Password123!' generated with cost factor 11
DECLARE @BcryptHash NVARCHAR(100) = '$2a$11$rBbBQMa1kLU/nB4YmY1Jt.1gA7rVwNwsIHYCGKqJ6SFKlzz3oFnlC';

-- Update all users to use the correct BCrypt password hash
UPDATE Users
SET PasswordHash = @BcryptHash,
    Status = 0,  -- Active status
    FailedLoginAttempts = 0,
    LockoutEndAt = NULL
WHERE PasswordHash != @BcryptHash OR PasswordHash IS NULL;

PRINT 'Updated ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' user password(s)';
PRINT 'All users can now login with: Password123!';
GO
