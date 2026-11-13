-- ══════════════════════════════════════════════════════════════════
-- Mexican Restaurant Web App - Database Script
-- Compatible with SQL Server (LocalDB / Full SQL Server)
-- ══════════════════════════════════════════════════════════════════

-- Run using: dotnet ef database update
-- Or manually apply this SQL script to your database.

-- ─── 1. Create Database ──────────────────────────────────────────────────────
-- (Only needed if running manually; EF Migrations handle this automatically)
-- CREATE DATABASE MexicanRestaurantDb;
-- GO
-- USE MexicanRestaurantDb;
-- GO

-- ─── 2. ASP.NET Identity Tables (auto-created by EF migrations) ──────────────
-- AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims,
-- AspNetRoleClaims, AspNetUserLogins, AspNetUserTokens

-- ─── 3. MenuItems ────────────────────────────────────────────────────────────
CREATE TABLE MenuItems (
    Id           INT           IDENTITY(1,1) PRIMARY KEY,
    Name         NVARCHAR(100) NOT NULL,
    Description  NVARCHAR(500) NOT NULL,
    Price        DECIMAL(18,2) NOT NULL,
    Category     NVARCHAR(50)  NOT NULL,
    ImageUrl     NVARCHAR(500) NULL,
    IsAvailable  BIT           NOT NULL DEFAULT 1,
    IsFeatured   BIT           NOT NULL DEFAULT 0,
    CreatedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

-- ─── 4. CartItems ────────────────────────────────────────────────────────────
CREATE TABLE CartItems (
    Id         INT           IDENTITY(1,1) PRIMARY KEY,
    UserId     NVARCHAR(450) NOT NULL,
    MenuItemId INT           NOT NULL,
    Quantity   INT           NOT NULL DEFAULT 1,
    AddedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_CartItems_Users      FOREIGN KEY (UserId)     REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CartItems_MenuItems  FOREIGN KEY (MenuItemId) REFERENCES MenuItems(Id)   ON DELETE RESTRICT
);

-- ─── 5. Orders ───────────────────────────────────────────────────────────────
CREATE TABLE Orders (
    Id                   INT            IDENTITY(1,1) PRIMARY KEY,
    UserId               NVARCHAR(450)  NOT NULL,
    OrderDate            DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    TotalAmount          DECIMAL(18,2)  NOT NULL,
    TaxAmount            DECIMAL(18,2)  NOT NULL DEFAULT 0,
    Status               INT            NOT NULL DEFAULT 0, -- enum: 0=Pending, 1=Paid, 2=Preparing, 3=Ready, 4=Delivered, 5=Cancelled
    StripeSessionId      NVARCHAR(200)  NULL,
    StripePaymentIntentId NVARCHAR(200) NULL,
    IsPaid               BIT            NOT NULL DEFAULT 0,
    DeliveryAddress      NVARCHAR(500)  NULL,
    Notes                NVARCHAR(500)  NULL,
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE RESTRICT
);

-- ─── 6. OrderDetails ─────────────────────────────────────────────────────────
CREATE TABLE OrderDetails (
    Id         INT           IDENTITY(1,1) PRIMARY KEY,
    OrderId    INT           NOT NULL,
    MenuItemId INT           NOT NULL,
    ItemName   NVARCHAR(100) NOT NULL,  -- snapshot of name at order time
    UnitPrice  DECIMAL(18,2) NOT NULL,  -- snapshot of price at order time
    Quantity   INT           NOT NULL,
    CONSTRAINT FK_OrderDetails_Orders    FOREIGN KEY (OrderId)    REFERENCES Orders(Id)    ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetails_MenuItems FOREIGN KEY (MenuItemId) REFERENCES MenuItems(Id) ON DELETE RESTRICT
);

-- ─── 7. Seed Menu Items ───────────────────────────────────────────────────────
INSERT INTO MenuItems (Name, Description, Price, Category, ImageUrl, IsAvailable, IsFeatured) VALUES
('Beef Tacos',        'Crispy corn tortillas stuffed with seasoned ground beef, fresh salsa, cheddar cheese, and lettuce.',             8.99,  'Tacos',   '/images/beef-tacos.jpg',      1, 1),
('Chicken Tacos',     'Grilled chicken breast with avocado crema, pico de gallo, and cilantro in soft flour tortillas.',               9.49,  'Tacos',   '/images/chicken-tacos.jpg',   1, 1),
('Fish Tacos',        'Beer-battered cod with shredded cabbage, chipotle mayo, and fresh lime on corn tortillas.',                     10.99, 'Tacos',   '/images/fish-tacos.jpg',      1, 0),
('Beef Burrito',      'A massive flour tortilla filled with seasoned beef, rice, black beans, sour cream, and salsa.',                 12.99, 'Burritos', '/images/beef-burrito.jpg',    1, 1),
('Veggie Burrito',    'Roasted seasonal vegetables, black beans, Spanish rice, guacamole wrapped in a warm flour tortilla.',           11.49, 'Burritos', '/images/veggie-burrito.jpg',  1, 0),
('Chicken Enchiladas','Three corn tortillas filled with shredded chicken, smothered in red chile sauce and melted cheese.',            13.49, 'Burritos', '/images/enchiladas.jpg',      1, 1),
('Classic Nachos',    'Crispy tortilla chips loaded with cheddar cheese, jalapeños, sour cream, and guacamole.',                      9.99,  'Nachos',  '/images/nachos.jpg',          1, 1),
('Loaded Nachos',     'Everything on Classic Nachos plus seasoned beef, black beans, pico de gallo, and drizzled hot sauce.',         13.99, 'Nachos',  '/images/loaded-nachos.jpg',   1, 0),
('Quesadilla',        'Grilled flour tortilla stuffed with melted Monterey Jack cheese. Served with salsa and sour cream.',           7.99,  'Nachos',  '/images/quesadilla.jpg',      1, 0),
('Horchata',          'Traditional Mexican rice drink with cinnamon and vanilla. Refreshing and sweet.',                              3.99,  'Drinks',  '/images/horchata.jpg',        1, 0),
('Jamaica (Hibiscus Tea)', 'Chilled hibiscus flower tea, lightly sweetened. A Mexican classic full of antioxidants.',                 3.49,  'Drinks',  '/images/jamaica.jpg',         1, 0),
('Mexican Soda',      'Authentic Jarritos fruit soda in your choice of Tamarind, Mandarin, or Lime flavors.',                         2.99,  'Drinks',  '/images/mexican-soda.jpg',    1, 0);

-- ─── 8. Default Admin Role & User ────────────────────────────────────────────
-- These are created automatically by DbInitializer.cs at startup.
-- Admin credentials:
--   Email:    admin@mexicanrestaurant.com
--   Password: Admin@123!

PRINT 'Database script completed successfully.';
GO
