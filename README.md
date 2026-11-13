# El Rancho

A simple ASP.NET Core 8 MVC restaurant application for a fictional Mexican food brand. The app lets customers browse the menu, add products to the cart, complete checkout with Stripe, and view their order history. Admin users can manage menu items and review customer orders.

## Overview

El Rancho is designed to showcase a full e-commerce flow for a restaurant website using modern ASP.NET Core practices. It includes identity-based authentication, role-based authorization, database-backed menu management, and a checkout experience integrated with Stripe test payments.

## Tech Stack

- ASP.NET Core 8 MVC
- Entity Framework Core
- ASP.NET Core Identity
- SQL Server / LocalDB
- Stripe API
- Bootstrap 5

## Features

- Customer registration and login
- Role-based access for Admin and Customer
- Menu browsing with categories and item details
- Shopping cart with quantity updates and item removal
- Secure checkout flow with Stripe test payments
- Order history for users
- Admin dashboard for menu and order management
- Responsive restaurant-themed UI

## Project Structure

```text
Controllers/      - MVC controllers for Home, Menu, Cart, Order, Account, and Admin
Data/             - DbContext and database seeding logic
Models/           - Application models such as MenuItem, CartItem, Order, and User
Services/         - Stripe payment integration
Views/            - Razor views for the web interface
wwwroot/          - CSS, JavaScript, and static assets
appsettings.json  - App configuration and connection strings
database_script.sql - Optional SQL script for manual database setup
Program.cs        - ASP.NET Core app startup configuration
```

## Quick Start

### 1. Prerequisites

- .NET 8 SDK
- SQL Server LocalDB or another supported SQL server
- Visual Studio 2022 or VS Code

### 2. Clone the project

```bash
git clone <your-repo-url>
cd MexicanRestaurant
```

### 3. Configure the database

Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MexicanRestaurantDb;Trusted_Connection=True;"
}
```

If you want to use a different provider, update the database configuration in `Program.cs` accordingly.

### 4. Configure Stripe

Add your test keys in `appsettings.json`:

```json
"Stripe": {
  "PublishableKey": "pk_test_YOUR_KEY_HERE",
  "SecretKey": "sk_test_YOUR_KEY_HERE"
}
```

You can generate test keys at the Stripe Dashboard.

### 5. Run the app

```bash
dotnet restore
dotnet ef database update
dotnet run
```

Then open the local URL shown in the terminal.

## Default Admin Account

```text
Email: admin@mexicanrestaurant.com
Password: Admin@123!
```

## Stripe Test Card

Use the following test values in Stripe checkout:

- Card number: `4242 4242 4242 4242`
- Expiration: any future date
- CVC: any 3 digits

## Notes

- Keep real Stripe secrets out of source control and use environment variables or user secrets in production.
- The app seeds sample menu data and an admin user automatically when the database is initialized.
- If the database is empty, Entity Framework will handle migration setup when the app starts.

## Optional: Manual Database Setup

A `database_script.sql` file is included if you want to initialize the database manually instead of using EF migrations.

## Summary

El Rancho is a polished restaurant ordering demo that combines a storefront experience with admin management and payment processing, making it a strong ASP.NET Core sample project for learning or portfolio use.