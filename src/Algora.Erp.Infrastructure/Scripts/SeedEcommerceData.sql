-- Seed eCommerce Sample Data for Algora.Erp
-- Run this script after CreateEcommerceTables.sql

-- =============================================
-- Insert Product Categories
-- =============================================
DECLARE @ElectronicsId UNIQUEIDENTIFIER = NEWID();
DECLARE @ClothingId UNIQUEIDENTIFIER = NEWID();
DECLARE @HomeId UNIQUEIDENTIFIER = NEWID();
DECLARE @BooksId UNIQUEIDENTIFIER = NEWID();
DECLARE @SportsId UNIQUEIDENTIFIER = NEWID();

INSERT INTO EcommerceCategories (Id, Name, Slug, Description, ImageUrl, ParentCategoryId, SortOrder, IsActive, CreatedAt)
VALUES
    (@ElectronicsId, 'Electronics', 'electronics', 'Laptops, phones, tablets and more', 'https://images.unsplash.com/photo-1498049794561-7780e7231661?w=400', NULL, 1, 1, GETUTCDATE()),
    (@ClothingId, 'Clothing', 'clothing', 'Fashion and apparel for everyone', 'https://images.unsplash.com/photo-1489987707025-afc232f7ea0f?w=400', NULL, 2, 1, GETUTCDATE()),
    (@HomeId, 'Home & Garden', 'home-garden', 'Furniture, decor and outdoor essentials', 'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400', NULL, 3, 1, GETUTCDATE()),
    (@BooksId, 'Books', 'books', 'Fiction, non-fiction and educational books', 'https://images.unsplash.com/photo-1495446815901-a7297e633e8d?w=400', NULL, 4, 1, GETUTCDATE()),
    (@SportsId, 'Sports & Outdoors', 'sports-outdoors', 'Fitness equipment and outdoor gear', 'https://images.unsplash.com/photo-1461896836934- voices-of-the-next-generation-11?w=400', NULL, 5, 1, GETUTCDATE());

-- Sub-categories for Electronics
INSERT INTO EcommerceCategories (Id, Name, Slug, Description, ImageUrl, ParentCategoryId, SortOrder, IsActive, CreatedAt)
VALUES
    (NEWID(), 'Laptops', 'laptops', 'Notebooks and ultrabooks', NULL, @ElectronicsId, 1, 1, GETUTCDATE()),
    (NEWID(), 'Smartphones', 'smartphones', 'Mobile phones and accessories', NULL, @ElectronicsId, 2, 1, GETUTCDATE()),
    (NEWID(), 'Audio', 'audio', 'Headphones, speakers and more', NULL, @ElectronicsId, 3, 1, GETUTCDATE());

-- Sub-categories for Clothing
INSERT INTO EcommerceCategories (Id, Name, Slug, Description, ImageUrl, ParentCategoryId, SortOrder, IsActive, CreatedAt)
VALUES
    (NEWID(), 'Men''s Clothing', 'mens-clothing', 'Shirts, pants, jackets for men', NULL, @ClothingId, 1, 1, GETUTCDATE()),
    (NEWID(), 'Women''s Clothing', 'womens-clothing', 'Dresses, tops, bottoms for women', NULL, @ClothingId, 2, 1, GETUTCDATE()),
    (NEWID(), 'Accessories', 'accessories', 'Bags, belts, watches and more', NULL, @ClothingId, 3, 1, GETUTCDATE());

-- =============================================
-- Insert Products
-- =============================================
DECLARE @Product1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product5Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product6Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product7Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product8Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product9Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product10Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product11Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product12Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO EcommerceProducts (Id, Name, Slug, Sku, Description, ShortDescription, Price, CompareAtPrice, CostPrice, StockQuantity, LowStockThreshold, Weight, CategoryId, Status, IsFeatured, SeoTitle, SeoDescription, CreatedAt)
VALUES
    -- Electronics
    (@Product1Id, 'MacBook Pro 14"', 'macbook-pro-14', 'ELEC-001', 'The most powerful MacBook Pro ever is here. With the M3 Pro or M3 Max chip, incredible battery life, and a stunning Liquid Retina XDR display.', 'Powerful laptop with M3 chip', 1999.99, 2199.99, 1500.00, 25, 5, 1.6, @ElectronicsId, 1, 1, 'MacBook Pro 14 inch - M3 Chip', 'Buy the new MacBook Pro 14 with M3 chip', GETUTCDATE()),
    (@Product2Id, 'iPhone 15 Pro', 'iphone-15-pro', 'ELEC-002', 'iPhone 15 Pro. Forged in titanium with the A17 Pro chip, a customizable Action button, and the most powerful iPhone camera system ever.', 'Latest iPhone with titanium design', 1199.99, NULL, 800.00, 50, 10, 0.19, @ElectronicsId, 1, 1, 'iPhone 15 Pro - Titanium', 'Get the new iPhone 15 Pro', GETUTCDATE()),
    (@Product3Id, 'Sony WH-1000XM5', 'sony-wh-1000xm5', 'ELEC-003', 'Industry-leading noise cancellation headphones with exceptional sound quality. 30 hours of battery life and speak-to-chat technology.', 'Premium noise-canceling headphones', 399.99, 449.99, 250.00, 100, 15, 0.25, @ElectronicsId, 1, 0, 'Sony WH-1000XM5 Wireless Headphones', 'Best noise-canceling headphones', GETUTCDATE()),
    (@Product4Id, 'Samsung 65" OLED TV', 'samsung-65-oled', 'ELEC-004', 'Experience stunning picture quality with Samsung''s latest OLED technology. Neural Quantum Processor 4K for incredible detail.', '65 inch OLED Smart TV', 2499.99, 2799.99, 1800.00, 15, 3, 22.5, @ElectronicsId, 1, 1, 'Samsung 65 inch OLED TV', 'Premium OLED TV with AI upscaling', GETUTCDATE()),

    -- Clothing
    (@Product5Id, 'Classic Cotton T-Shirt', 'classic-cotton-tshirt', 'CLTH-001', 'Premium 100% cotton t-shirt with a comfortable fit. Pre-shrunk fabric that maintains shape wash after wash.', 'Comfortable everyday t-shirt', 29.99, NULL, 8.00, 200, 30, 0.2, @ClothingId, 1, 0, 'Classic Cotton T-Shirt', 'Premium cotton t-shirt for everyday wear', GETUTCDATE()),
    (@Product6Id, 'Slim Fit Chinos', 'slim-fit-chinos', 'CLTH-002', 'Modern slim fit chinos with stretch fabric for comfort. Perfect for casual or smart casual occasions.', 'Modern stretch chinos', 59.99, 79.99, 25.00, 150, 20, 0.4, @ClothingId, 1, 0, 'Slim Fit Chinos - Multiple Colors', 'Comfortable stretch chinos', GETUTCDATE()),
    (@Product7Id, 'Leather Crossbody Bag', 'leather-crossbody-bag', 'CLTH-003', 'Genuine leather crossbody bag with adjustable strap. Multiple compartments for organization.', 'Stylish leather bag', 149.99, 179.99, 60.00, 45, 10, 0.6, @ClothingId, 1, 1, 'Leather Crossbody Bag', 'Premium leather bag for everyday use', GETUTCDATE()),

    -- Home & Garden
    (@Product8Id, 'Modern Coffee Table', 'modern-coffee-table', 'HOME-001', 'Minimalist design coffee table with solid oak top and powder-coated steel legs. Perfect centerpiece for your living room.', 'Minimalist oak coffee table', 299.99, 349.99, 150.00, 30, 5, 15.0, @HomeId, 1, 1, 'Modern Oak Coffee Table', 'Stylish coffee table for modern homes', GETUTCDATE()),
    (@Product9Id, 'Indoor Plant Set', 'indoor-plant-set', 'HOME-002', 'Set of 3 low-maintenance indoor plants in decorative ceramic pots. Perfect for beginners. Includes care guide.', 'Set of 3 easy-care plants', 49.99, NULL, 20.00, 75, 15, 3.5, @HomeId, 1, 0, 'Indoor Plant Set - 3 Plants', 'Easy-care indoor plants with pots', GETUTCDATE()),

    -- Books
    (@Product10Id, 'The Art of Clean Code', 'art-of-clean-code', 'BOOK-001', 'A comprehensive guide to writing maintainable, readable, and efficient code. Essential for every software developer.', 'Essential coding guide', 39.99, NULL, 12.00, 120, 20, 0.5, @BooksId, 1, 0, 'The Art of Clean Code - Programming Book', 'Learn to write better code', GETUTCDATE()),

    -- Sports
    (@Product11Id, 'Yoga Mat Premium', 'yoga-mat-premium', 'SPRT-001', 'Extra thick 6mm yoga mat with non-slip surface. Eco-friendly materials with carrying strap included.', 'Non-slip eco-friendly yoga mat', 49.99, 59.99, 18.00, 80, 15, 1.2, @SportsId, 1, 0, 'Premium Yoga Mat 6mm', 'Eco-friendly yoga mat', GETUTCDATE()),
    (@Product12Id, 'Adjustable Dumbbells Set', 'adjustable-dumbbells', 'SPRT-002', 'Space-saving adjustable dumbbells from 5-50 lbs. Quick-change weight system for efficient workouts.', 'Adjustable weights 5-50 lbs', 349.99, 399.99, 180.00, 25, 5, 25.0, @SportsId, 1, 1, 'Adjustable Dumbbells 5-50 lbs', 'Home gym adjustable weights', GETUTCDATE());

-- =============================================
-- Insert Product Images
-- =============================================
INSERT INTO ProductImages (Id, ProductId, Url, AltText, IsPrimary, SortOrder, CreatedAt)
VALUES
    -- MacBook Pro images
    (NEWID(), @Product1Id, 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=800', 'MacBook Pro 14 inch front view', 1, 1, GETUTCDATE()),
    (NEWID(), @Product1Id, 'https://images.unsplash.com/photo-1611186871348-b1ce696e52c9?w=800', 'MacBook Pro keyboard close-up', 0, 2, GETUTCDATE()),

    -- iPhone images
    (NEWID(), @Product2Id, 'https://images.unsplash.com/photo-1695048133142-1a20484d2569?w=800', 'iPhone 15 Pro titanium', 1, 1, GETUTCDATE()),
    (NEWID(), @Product2Id, 'https://images.unsplash.com/photo-1592899677977-9c10ca588bbd?w=800', 'iPhone camera system', 0, 2, GETUTCDATE()),

    -- Sony headphones
    (NEWID(), @Product3Id, 'https://images.unsplash.com/photo-1618366712010-f4ae9c647dcb?w=800', 'Sony WH-1000XM5 headphones', 1, 1, GETUTCDATE()),

    -- Samsung TV
    (NEWID(), @Product4Id, 'https://images.unsplash.com/photo-1593359677879-a4bb92f829d1?w=800', 'Samsung OLED TV in living room', 1, 1, GETUTCDATE()),

    -- T-Shirt
    (NEWID(), @Product5Id, 'https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=800', 'Classic white cotton t-shirt', 1, 1, GETUTCDATE()),
    (NEWID(), @Product5Id, 'https://images.unsplash.com/photo-1583743814966-8936f5b7be1a?w=800', 'T-shirt fabric detail', 0, 2, GETUTCDATE()),

    -- Chinos
    (NEWID(), @Product6Id, 'https://images.unsplash.com/photo-1473966968600-fa801b869a1a?w=800', 'Slim fit chinos', 1, 1, GETUTCDATE()),

    -- Crossbody bag
    (NEWID(), @Product7Id, 'https://images.unsplash.com/photo-1548036328-c9fa89d128fa?w=800', 'Leather crossbody bag', 1, 1, GETUTCDATE()),

    -- Coffee table
    (NEWID(), @Product8Id, 'https://images.unsplash.com/photo-1533090481720-856c6e3c1fdc?w=800', 'Modern coffee table in living room', 1, 1, GETUTCDATE()),

    -- Plant set
    (NEWID(), @Product9Id, 'https://images.unsplash.com/photo-1459411552884-841db9b3cc2a?w=800', 'Indoor plant set', 1, 1, GETUTCDATE()),

    -- Book
    (NEWID(), @Product10Id, 'https://images.unsplash.com/photo-1532012197267-da84d127e765?w=800', 'Programming book', 1, 1, GETUTCDATE()),

    -- Yoga mat
    (NEWID(), @Product11Id, 'https://images.unsplash.com/photo-1601925260368-ae2f83cf8b7f?w=800', 'Premium yoga mat', 1, 1, GETUTCDATE()),

    -- Dumbbells
    (NEWID(), @Product12Id, 'https://images.unsplash.com/photo-1638536532686-d610adfc8e5c?w=800', 'Adjustable dumbbells set', 1, 1, GETUTCDATE());

-- =============================================
-- Insert Sample Customers
-- =============================================
INSERT INTO WebCustomers (Id, Email, FirstName, LastName, Phone, Address, City, State, PostalCode, Country, OrderCount, TotalSpent, IsActive, CreatedAt)
VALUES
    (NEWID(), 'john.doe@example.com', 'John', 'Doe', '+1-555-0101', '123 Main Street', 'New York', 'NY', '10001', 'United States', 5, 2499.95, 1, GETUTCDATE()),
    (NEWID(), 'jane.smith@example.com', 'Jane', 'Smith', '+1-555-0102', '456 Oak Avenue', 'Los Angeles', 'CA', '90001', 'United States', 3, 899.97, 1, GETUTCDATE()),
    (NEWID(), 'robert.wilson@example.com', 'Robert', 'Wilson', '+1-555-0103', '789 Pine Road', 'Chicago', 'IL', '60601', 'United States', 8, 4599.92, 1, GETUTCDATE()),
    (NEWID(), 'emily.johnson@example.com', 'Emily', 'Johnson', '+1-555-0104', '321 Elm Street', 'Houston', 'TX', '77001', 'United States', 2, 349.98, 1, GETUTCDATE()),
    (NEWID(), 'michael.brown@example.com', 'Michael', 'Brown', '+1-555-0105', '654 Maple Drive', 'Phoenix', 'AZ', '85001', 'United States', 0, 0, 1, GETUTCDATE());

-- =============================================
-- Insert Coupons
-- =============================================
INSERT INTO Coupons (Id, Code, Description, DiscountType, DiscountValue, MinimumOrderAmount, MaximumDiscount, UsageLimit, UsedCount, StartDate, EndDate, IsActive, CreatedAt)
VALUES
    (NEWID(), 'WELCOME10', 'Welcome discount - 10% off first order', 1, 10.00, 50.00, 100.00, 1000, 0, GETUTCDATE(), DATEADD(MONTH, 3, GETUTCDATE()), 1, GETUTCDATE()),
    (NEWID(), 'SAVE20', 'Save $20 on orders over $100', 0, 20.00, 100.00, NULL, 500, 0, GETUTCDATE(), DATEADD(MONTH, 1, GETUTCDATE()), 1, GETUTCDATE()),
    (NEWID(), 'SUMMER25', 'Summer sale - 25% off', 1, 25.00, 75.00, 200.00, 200, 0, GETUTCDATE(), DATEADD(MONTH, 2, GETUTCDATE()), 1, GETUTCDATE()),
    (NEWID(), 'FREESHIP', 'Free shipping on orders over $50', 0, 0.00, 50.00, NULL, NULL, 0, GETUTCDATE(), DATEADD(YEAR, 1, GETUTCDATE()), 1, GETUTCDATE()),
    (NEWID(), 'VIP50', 'VIP exclusive - $50 off $200+', 0, 50.00, 200.00, NULL, 50, 0, GETUTCDATE(), DATEADD(MONTH, 6, GETUTCDATE()), 1, GETUTCDATE());

-- =============================================
-- Insert Shipping Methods
-- =============================================
INSERT INTO ShippingMethods (Id, Name, Description, Rate, EstimatedDays, IsActive, SortOrder, CreatedAt)
VALUES
    (NEWID(), 'Standard Shipping', 'Delivery in 5-7 business days', 5.99, '5-7 business days', 1, 1, GETUTCDATE()),
    (NEWID(), 'Express Shipping', 'Delivery in 2-3 business days', 12.99, '2-3 business days', 1, 2, GETUTCDATE()),
    (NEWID(), 'Next Day Delivery', 'Delivery by next business day', 24.99, '1 business day', 1, 3, GETUTCDATE()),
    (NEWID(), 'Free Shipping', 'Free standard shipping on orders over $50', 0.00, '5-7 business days', 1, 0, GETUTCDATE());

-- =============================================
-- Insert Payment Methods
-- =============================================
INSERT INTO WebPaymentMethods (Id, Name, Description, ProcessorType, IsActive, SortOrder, CreatedAt)
VALUES
    (NEWID(), 'Credit Card', 'Pay with Visa, Mastercard, or American Express', 'Stripe', 1, 1, GETUTCDATE()),
    (NEWID(), 'PayPal', 'Pay securely with your PayPal account', 'PayPal', 1, 2, GETUTCDATE()),
    (NEWID(), 'Bank Transfer', 'Direct bank transfer payment', 'Manual', 1, 3, GETUTCDATE()),
    (NEWID(), 'Cash on Delivery', 'Pay when your order arrives', 'COD', 1, 4, GETUTCDATE());

-- =============================================
-- Insert eCommerce Settings
-- =============================================
INSERT INTO EcommerceSettings (Id, SettingKey, SettingValue, Description, CreatedAt)
VALUES
    (NEWID(), 'StoreName', 'Algora Shop', 'The name of the online store', GETUTCDATE()),
    (NEWID(), 'StoreLogo', '/images/logo.png', 'Store logo URL', GETUTCDATE()),
    (NEWID(), 'StoreEmail', 'support@algorashop.com', 'Store contact email', GETUTCDATE()),
    (NEWID(), 'StorePhone', '+1-800-ALGORA', 'Store contact phone', GETUTCDATE()),
    (NEWID(), 'StoreAddress', '123 Business Ave, Suite 100, San Francisco, CA 94102', 'Store physical address', GETUTCDATE()),
    (NEWID(), 'Currency', 'USD', 'Default store currency', GETUTCDATE()),
    (NEWID(), 'CurrencySymbol', '$', 'Currency symbol', GETUTCDATE()),
    (NEWID(), 'TaxRate', '8.5', 'Default tax rate percentage', GETUTCDATE()),
    (NEWID(), 'EnableTax', 'true', 'Enable tax calculation', GETUTCDATE()),
    (NEWID(), 'FreeShippingThreshold', '50', 'Minimum order amount for free shipping', GETUTCDATE()),
    (NEWID(), 'EnableReviews', 'true', 'Enable product reviews', GETUTCDATE()),
    (NEWID(), 'EnableWishlist', 'true', 'Enable wishlist feature', GETUTCDATE()),
    (NEWID(), 'LowStockThreshold', '10', 'Default low stock alert threshold', GETUTCDATE()),
    (NEWID(), 'OrderPrefix', 'ORD', 'Prefix for order numbers', GETUTCDATE()),
    (NEWID(), 'MaxCartQuantity', '99', 'Maximum quantity per item in cart', GETUTCDATE());

PRINT 'eCommerce sample data inserted successfully!';
GO
