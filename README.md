# Smart Travel Planner

Smart Travel Planner is a full-stack travel planning application for creating trips, exploring destinations, organizing itineraries, and tracking travel expenses in one place.

Repository: https://github.com/elahimahi/travel_planner

## Project Goal

- Make travel planning simple, organized, and easy to manage.
- Help users manage destinations, schedules, budgets, bookings, and activities.
- Give administrators tools to manage users and destination content.

## Key Features

### User Features

- Registration, login, logout, email confirmation, and password reset.
- Profile management with profile photo upload.
- Create, update, view, and delete trips.
- Select destinations, dates, trip type, travelers, and trip status.
- Build day-by-day itineraries and manage activities.
- Track budgets and add categorized expenses.
- Manage accommodation and transportation details.
- Browse destinations, attractions, images, reviews, and favorites.
- Packing checklist, weather information, and responsive design.

### Admin Features

- Role-based access control for Admin and User roles.
- Manage users and account status.
- Add, edit, publish, and manage destinations.
- Review application data through admin dashboards.

## Technology Stack

- **Backend:** ASP.NET Core MVC on .NET 10
- **Language:** C#
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Authentication:** ASP.NET Core Identity with cookie authentication
- **Frontend:** HTML, CSS, JavaScript, and Bootstrap 5
- **Email:** MailKit and MimeKit
- **Charts:** Chart.js

## Project Structure

```text
Travel/
├── Controllers/       API and MVC controllers
├── Data/              Entity Framework database context
├── Frontend/          Static HTML, CSS, and JavaScript frontend
├── Models/            Application and view models
├── Repositories/      Data access repositories
├── Services/          Email, chat, and database seeding services
├── Views/             Razor MVC views
├── wwwroot/           Public assets and uploaded files
├── Program.cs         Application configuration and startup
└── appsettings.json   Application and database configuration
```

## Requirements

- .NET 10 SDK
- SQL Server, SQL Server Express, or LocalDB
- Visual Studio 2022 or Visual Studio Code
- Git

## Setup and Run

1. Clone the repository:

```powershell
git clone https://github.com/elahimahi/travel_planner.git
cd travel_planner
```

2. Configure `ConnectionStrings:DefaultConnection` in `appsettings.json`.

3. Restore packages and build the application:

```powershell
dotnet restore
dotnet build .\TravelPlanner.csproj
```

4. Start the application:

```powershell
dotnet run --project .\TravelPlanner.csproj
```

5. Open the application at:

```text
http://localhost:8000
```

The application creates or updates required database objects during startup and seeds initial data when the database is configured correctly.

## Configuration

- Set `ConnectionStrings:DefaultConnection` for your SQL Server instance.
- Configure `EmailSettings` for email confirmation and password reset emails.
- Keep passwords, API keys, and other secrets out of source control.

## API Overview

The frontend communicates with same-origin API routes, including:

- `/api/register` and `/api/login` for authentication.
- `/api/current-user` and `/api/profile` for user profiles.
- `/api/trips` for trip and expense management.
- `/api/destinations` for destination data.
- `/api/admin/*` for administrator operations.

## Security

- ASP.NET Core Identity password hashing.
- Cookie-based authentication and role authorization.
- CSRF protection for applicable requests.
- Entity Framework Core parameterized database queries.
- Input validation and protected admin endpoints.

## Contributing

1. Fork the repository.
2. Create a feature branch.
3. Make focused changes and test locally.
4. Commit and push your changes.
5. Open a pull request with a clear description.

## License

No license file is currently included in this repository. Add a license before distributing the project publicly.