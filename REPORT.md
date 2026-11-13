# Mexican Restaurant Web Application
## System Design & Project Report

**Module:** ASP.NET Application (CSE205)
**Institution:** Kigali Independent University (ULK)
**Academic Year:** 2025–2026
**Deadline:** 5th May 2026
**Class:** Year 2 / Day / Software Engineering – Group A

---

## Table of Contents
1. [Project Overview](#1-project-overview)
2. [System Architecture](#2-system-architecture)
3. [Database Design](#3-database-design)
4. [Authentication & Authorization](#4-authentication--authorization)
5. [Core Features](#5-core-features)
6. [Stripe Payment Integration](#6-stripe-payment-integration)
7. [UI/UX Design](#7-uiux-design)
8. [Deployment Guide](#8-deployment-guide)
9. [Testing Credentials](#9-testing-credentials)

---

## 1. Project Overview

This project is a full-stack **Mexican Restaurant Web Application** built with ASP.NET Core 8 MVC. It allows customers to browse a menu, add items to a shopping cart, place orders, and pay securely using Stripe. Administrators have a dedicated dashboard to manage menu items and monitor all orders.

### Technologies Used

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 MVC |
| ORM | Entity Framework Core 8 |
| Database | SQL Server (LocalDB / full) or PostgreSQL |
| Authentication | ASP.NET Core Identity |
| Payment | Stripe .NET SDK v43 |
| UI | Bootstrap 5.3, Font Awesome 6 |
| Fonts | Google Fonts (Playfair Display, Lato) |

---

## 2. System Architecture

The application follows the **MVC (Model-View-Controller)** architectural pattern:

```
MexicanRestaurant/
├── Controllers/
│   ├── HomeController.cs        ← Landing page, about, contact
│   ├── MenuController.cs        ← Browse & filter menu items
│   ├── CartController.cs        ← Add, update, remove cart items
│   ├── OrderController.cs       ← Checkout, Stripe, order history
│   ├── AccountController.cs     ← Register, Login, Logout
│   └── AdminController.cs       ← Admin dashboard, CRUD, orders
├── Models/
│   ├── ApplicationUser.cs       ← Extended Identity user
│   ├── MenuItem.cs              ← Food item entity
│   ├── CartItem.cs              ← Shopping cart entity
│   ├── Order.cs                 ← Order entity
│   ├── OrderDetail.cs           ← Line items per order
│   └── ViewModels/ViewModels.cs ← DTOs for views
├── Data/
│   ├── ApplicationDbContext.cs  ← EF Core DbContext + seed data
│   └── DbInitializer.cs         ← Roles & admin user seeder
├── Services/
│   └── StripeService.cs         ← Stripe Checkout session logic
├── Views/                        ← Razor views (cshtml)
├── wwwroot/                      ← Static files (CSS, JS, images)
├── Program.cs                   ← DI container & middleware pipeline
└── appsettings.json             ← Config (connection string, Stripe keys)
```

### Request Flow

```
Browser Request
    ↓
Middleware Pipeline (Auth, Session, Routing)
    ↓
Controller (handles logic, calls services/DB)
    ↓
Model / EF Core → SQL Server / PostgreSQL
    ↓
ViewModel returned to View
    ↓
Razor View renders HTML → Browser Response
```

---

## 3. Database Design

### Entity Relationship Diagram (ERD)

```
AspNetUsers ──────────────────────────────────────────┐
│ Id (PK)                                              │
│ Email, PasswordHash, ...                             │
│ FullName, Address, CreatedAt                         │
└──────┬──────────────────────────────────────────────┘
       │ 1                               1
       │ ↓ (UserId FK)                   ↓ (UserId FK)
  CartItems                           Orders
  │ Id (PK)                           │ Id (PK)
  │ UserId (FK)                       │ UserId (FK)
  │ MenuItemId (FK)                   │ TotalAmount, TaxAmount
  │ Quantity                          │ Status (enum)
  │ AddedAt                           │ StripeSessionId
       │                              │ IsPaid
       │ N (MenuItemId FK)            │ DeliveryAddress
       ↓                              │
  MenuItems ◄────────────────── OrderDetails
  │ Id (PK)                     │ Id (PK)
  │ Name, Description           │ OrderId (FK)
  │ Price, Category             │ MenuItemId (FK)
  │ ImageUrl                    │ ItemName (snapshot)
  │ IsAvailable, IsFeatured     │ UnitPrice (snapshot)
  │ CreatedAt                   │ Quantity
```

### Why price is snapshotted in OrderDetail
When a customer places an order, the `ItemName` and `UnitPrice` are copied from `MenuItem` into `OrderDetail`. This ensures the order history remains accurate even if menu prices change later.

### Tables Summary

| Table | Purpose |
|---|---|
| `AspNetUsers` | Customer & admin accounts (Identity) |
| `AspNetRoles` | Admin, Customer roles |
| `AspNetUserRoles` | Role assignments |
| `MenuItems` | All food & drink items |
| `CartItems` | Active shopping cart per user |
| `Orders` | Placed orders with Stripe payment info |
| `OrderDetails` | Line items (snapshot) for each order |

---

## 4. Authentication & Authorization

The app uses **ASP.NET Core Identity** for authentication.

### Roles
- **Admin** – Full access: dashboard, menu CRUD, all orders, status updates
- **Customer** – Can browse menu, manage own cart, place orders, view own order history

### Role Assignment
- New registrations are automatically assigned the `Customer` role.
- The Admin role is seeded at startup via `DbInitializer.cs`.

### Access Control Examples
```csharp
[Authorize]                    // Any logged-in user
[Authorize(Roles = "Admin")]   // Admin only
```

### Cookie Configuration
```csharp
options.LoginPath = "/Account/Login";
options.ExpireTimeSpan = TimeSpan.FromHours(24);
options.SlidingExpiration = true;
```

---

## 5. Core Features

### 5.1 Menu Browsing
- Displays all available items grouped by category: **Tacos 🌮, Burritos 🌯, Nachos 🧀, Drinks 🥤**
- Filter by category using dynamic filter buttons
- Each card shows: Name, Description, Price, Image (emoji fallback)
- Featured items appear on the homepage hero section

### 5.2 Shopping Cart
- Add items from menu (requires login)
- Quantity update (+/−) per item
- Remove individual items
- Real-time subtotal, 8% tax, and total calculation
- Cart count badge in navbar (fetched via AJAX)

### 5.3 Order Management
- Checkout form collects: Full name, email, delivery address, notes
- Order saved to DB before redirecting to Stripe
- After payment: order marked as `Paid`; cart is cleared
- Order history page for customers
- Admin can view ALL orders and update their status

### 5.4 Admin Panel
- **Dashboard**: Total orders, revenue, customers, pending orders
- **Menu CRUD**: Create, read, update, delete menu items; upload images
- **Order Management**: View all orders, filter by status, update status

---

## 6. Stripe Payment Integration

### How It Works

```
Customer clicks "Pay $X.XX"
    ↓
OrderController.PlaceOrder() is called
    ↓
Order + OrderDetails saved to DB (Status: Pending)
    ↓
StripeService.CreateCheckoutSessionAsync() called
    ↓
Stripe creates a hosted checkout page session
    ↓
User is redirected to stripe.com/pay/...
    ↓
User enters card details on Stripe's secure page
    ↓
On success → redirected to /Order/Success?session_id=...
    ↓
Success action updates Order: IsPaid=true, Status=Paid
    ↓
On cancel → redirected to /Order/Cancel
```

### Configuration (appsettings.json)
```json
"Stripe": {
  "PublishableKey": "pk_test_YOUR_KEY_HERE",
  "SecretKey":      "sk_test_YOUR_KEY_HERE"
}
```

### Getting Stripe Keys (Step-by-Step)
1. Go to https://stripe.com and create a free account
2. In the Stripe Dashboard → **Developers** → **API Keys**
3. Copy your **Publishable key** (starts with `pk_test_`)
4. Copy your **Secret key** (starts with `sk_test_`)
5. Paste them into `appsettings.json`
6. For testing, use Stripe's test card: **4242 4242 4242 4242**, any future expiry, any CVV

### StripeService.cs Key Code
```csharp
var options = new SessionCreateOptions
{
    PaymentMethodTypes = new List<string> { "card" },
    LineItems = lineItems,   // list of items with prices
    Mode = "payment",
    SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
    CancelUrl = cancelUrl,
    Metadata = new Dictionary<string, string>
    {
        { "OrderId", order.Id.ToString() }
    }
};
var session = await new SessionService().CreateAsync(options);
return Redirect(session.Url);  // redirect to Stripe's page
```

---

## 7. UI/UX Design

### Design Theme
- **Dark Mexican aesthetic**: deep browns and terracotta (`#2c1810`, `#1a0a00`)
- **Accent colors**: Red (`#c0392b`), Orange (`#e67e22`), Yellow (`#f39c12`)
- **Typography**: Playfair Display (headings) + Lato (body)
- **Responsive**: Bootstrap 5 grid, mobile-first

### Pages
| Page | Description |
|---|---|
| Home (`/`) | Hero section with floating taco emoji, featured dishes, features strip |
| Menu (`/Menu`) | Category filter, cards layout, emoji fallback for missing images |
| Cart (`/Cart`) | Table with qty controls, order summary with tax |
| Checkout (`/Order/Checkout`) | Customer info form + Stripe redirect |
| Success / Cancel | Feedback pages after Stripe payment |
| Order History | List of all past orders with status badges |
| Login / Register | Auth cards with validation |
| Admin Dashboard | Stats cards + recent orders table |
| Admin Menu CRUD | Add/Edit/Delete with image upload |
| Admin Orders | Filter by status, update order status |

---

## 8. Deployment Guide

### Option A: Local IIS / IIS Express (Development)

```bash
# 1. Clone / open the project in Visual Studio 2022

# 2. Update connection string in appsettings.json:
#    "Server=(localdb)\mssqllocaldb;Database=MexicanRestaurantDb;..."

# 3. Add your Stripe keys to appsettings.json

# 4. Run EF migrations (in Package Manager Console or terminal):
dotnet ef migrations add InitialCreate
dotnet ef database update

# 5. Run the project (F5 in VS or):
dotnet run
```

### Option B: Render (Free Cloud Hosting)

1. Push the project to GitHub
2. Go to https://render.com → New → **Web Service**
3. Connect your GitHub repo
4. Set Build Command: `dotnet publish -c Release -o out`
5. Set Start Command: `dotnet out/MexicanRestaurant.dll`
6. Add **Environment Variables** in Render dashboard:
   - `ConnectionStrings__DefaultConnection` → your PostgreSQL URL
   - `Stripe__PublishableKey` → `pk_live_...`
   - `Stripe__SecretKey` → `sk_live_...`
7. Add a **PostgreSQL** database in Render (free tier)
8. Switch the EF Core provider to `UseNpgsql()` in `Program.cs`

### Option C: Azure App Service

```bash
# Publish to Azure using Visual Studio:
# Right-click project → Publish → Azure → Azure App Service

# Or via CLI:
az webapp up --name mexican-restaurant-app --resource-group myRG --runtime "DOTNET|8.0"
```

---

## 9. Testing Credentials

| Role | Email | Password |
|---|---|---|
| **Admin** | admin@mexicanrestaurant.com | Admin@123! |
| **Customer** | (Register your own account) | — |

### Stripe Test Card
| Field | Value |
|---|---|
| Card Number | 4242 4242 4242 4242 |
| Expiry | Any future date (e.g. 12/29) |
| CVV | Any 3 digits (e.g. 123) |
| ZIP | Any (e.g. 12345) |

---

## Grading Checklist

| Component | Implementation |
|---|---|
| ✅ Functionality (30%) | Menu browsing, cart, orders, admin CRUD all implemented |
| ✅ Database Design (15%) | 7 tables, proper FK constraints, data snapshots in OrderDetails |
| ✅ Stripe Integration (20%) | Full Checkout Session flow with Success/Cancel pages |
| ✅ UI/UX (15%) | Custom Mexican theme, Bootstrap 5, responsive design |
| ✅ Code Quality (10%) | MVC separation, service layer, async/await throughout |
| ✅ Report (10%) | This document |

---

*Prepared for CSE205 – ASP.NET Application*
*Kigali Independent University (ULK), 2025–2026*
