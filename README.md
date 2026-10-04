# real-estate-management-system

A simple Real Estate Management System built with ASP.NET Core (MVC / Razor) and Entity Framework Core.

This repository contains a teaching/demo real-estate marketplace with role-based users (Buyer, Seller, Admin), property listings, inquiries and basic dashboard pages.

---

## Technology

- .NET 9 / ASP.NET Core MVC (Razor views)
- Entity Framework Core (SQL Server LocalDB)
- ASP.NET Core Identity for authentication and roles
- Bootstrap for frontend styling

---

## Quick start

Prerequisites:
- .NET 9 SDK
- SQL Server LocalDB (or change connection string to your SQL server)

Steps:

1. Restore packages

   dotnet restore

2. Build

   dotnet build

3. Database (EF Core)

   - The project uses EF Core migrations to evolve the database schema. If you have existing data, do not drop the database.
   - To apply migrations locally:

	 dotnet ef database update --project realEstate

   - If you add or modify models, create a migration and apply it:

	 dotnet ef migrations add <Name> --project realEstate
	 dotnet ef database update --project realEstate

4. Run the app

   dotnet run --project realEstate

   Or open the solution `realEstate.sln` in Visual Studio and run.

---

## Authentication & Roles

This project uses ASP.NET Core Identity. Default roles used in the app are:

- Admin
- Seller
- Buyer

There is a role-selection login landing page at `/Account/RoleLogin` which directs users to the login flow with an intended role. After login the user is redirected to their role-specific dashboard (Admin > Seller > Buyer priority).

Registration allows selecting a role; the app assigns the chosen role at registration time.

---

## Features (current)

- Role-based login landing (RoleLogin)
- Role-aware login redirect logic
- Property details and action links updated to use role-aware login for unauthenticated users

Work in progress (not yet finalized):

- Full Seller property submission UI (multi-section form)
- Admin property approval workflow and dashboard statistics
- Amenities and image upload integration for properties

---

## Development notes

- Do not commit or push database migrations that drop or recreate existing tables if you rely on existing data.
- When working with roles, ensure RoleManager is configured and roles exist before assigning.

If you want me to update this README further (add environment-specific instructions, docker support, or a feature matrix), tell me what to include.

---

## License

This project does not include an explicit license file. Add a LICENSE if you intend to publish.

