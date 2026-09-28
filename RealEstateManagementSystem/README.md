# Real Estate Management System - Authentication Module

This project contains the authentication and authorization foundation for a full-featured Real Estate Management System. It is built using ASP.NET Core MVC, Entity Framework Core, SQL Server, and ASP.NET Core Identity.

## Features Currently Implemented

- **ASP.NET Core Identity** integration with custom `ApplicationUser`.
- **Role-Based Authorization** (`Admin`, `User`).
- **Registration**: Includes strong password policies and UI validation. New registrations default to the `User` role.
- **Login**: Integrated with cookie authentication, 'Remember Me', lockout protection, and requirement for email verification.
- **Email Verification Flow**: A development-friendly implementation that logs the verification url to the application console.
- **Forgot/Reset Password**: Generates secure tokens and facilitates password resets.
- **Account Lockout**: Triggers a 15-minute lockout after 5 failed login attempts.
- **Database Seeding**: Automatically creates the `Admin` and `User` roles. It seeds a default admin user if none exists (email: `admin@realestate.com`, password: `Admin@12345`).
- **UI**: Modern, responsive layout structured with Bootstrap 5. Navbar links dynamically appear depending on authentication and role privileges.
- **Repository Pattern**: Demonstrated structurally through `IUserRepository` to prepare for advanced operations.

## Database Configurations

By default, the application is configured to use a LocalDB SQL Server instance. You can change this in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RealEstateAuthDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

## Running the Application

1. **Migrations**: 
   The initial migration is already created and updated using EF Tools. If you need to revert or apply them again, use:
   `dotnet ef database update`

2. **Run**:
   Start the application directly from Visual Studio, or via the command line:
   `dotnet run`

3. **Console Email Links**:
   Since a real email provider isn't wired up in this development phase, you must check the running server application's console output for Account Verification and Password Reset links.

## Future Architecture Roadmap

The `ApplicationUser` in this module lays the groundwork for representing both buyers and sellers, renters, and admins under one unified identity system without splitting account types. It integrates seamlessly with:
- **Properties**: 1-to-Many (`UserId` acts as Owner).
- **Transactions**: Foreign keys for `BuyerId`/`SellerId` linking to `ApplicationUser`.
- **Favorites**: Many-to-Many entity bridging `User` and `Property`.
- **Inquiries & Reviews**: References `UserId`.
