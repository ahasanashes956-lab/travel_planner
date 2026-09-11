# Smart Travel Planner - ASP.NET Core MVC

A beautiful, responsive, and feature-rich travel planning application built with **ASP.NET Core MVC**, **SQL Server**, **Entity Framework Core**, and **Bootstrap 5**.

## 🌟 Features

### Authentication & Security
- ✅ User Registration with email verification
- ✅ Secure Login/Logout with session management
- ✅ Forgot Password & Password Reset functionality
- ✅ Role-based Authorization (Admin & User)
- ✅ Secure password hashing with Identity
- ✅ Profile Management

### Trip Management
- ✅ Create, Read, Update, Delete (CRUD) trips
- ✅ Select destinations from catalog
- ✅ Set travel dates and number of travelers
- ✅ Choose trip types (Solo, Family, Friends, Business)
- ✅ Update trip status (Planned, Ongoing, Completed)

### Dashboard
- ✅ Upcoming trips overview
- ✅ Recent trips history
- ✅ Trip statistics and analytics
- ✅ Quick action buttons
- ✅ Budget summary

### Destination Explorer
- ✅ Browse all destinations
- ✅ Search destinations by name, city, or country
- ✅ View destination details with attractions
- ✅ Image gallery
- ✅ Tourist ratings and reviews
- ✅ Add to favorites

### Itinerary Management
- ✅ Day-wise schedule planning
- ✅ Activity management with time slots
- ✅ Personal notes for each day
- ✅ Activity categories

### Budget & Expense Management
- ✅ Set trip budgets
- ✅ Track daily expenses
- ✅ Categorize expenses (Food, Transport, Accommodation, etc.)
- ✅ Budget progress visualization
- ✅ Expense reports and analytics

### Accommodation
- ✅ Add hotel bookings
- ✅ Hotel details and ratings
- ✅ Check-in/Check-out date management
- ✅ Hotel cost tracking

### Transportation
- ✅ Flight, train, and bus information
- ✅ Transportation provider details
- ✅ Ticket number tracking
- ✅ Cost management

### Additional Features
- ✅ Packing checklist
- ✅ Weather information
- ✅ Review system for destinations
- ✅ Favorites/Saved items
- ✅ Responsive design for mobile devices
- ✅ Beautiful modern UI with Bootstrap 5

## 🛠️ Technology Stack

- **Backend**: ASP.NET Core MVC (.NET 8)
- **Language**: C#
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Frontend**: HTML5, CSS3, Bootstrap 5, JavaScript
- **Authentication**: ASP.NET Core Identity
- **Email**: MailKit
- **Charts**: Chart.js

## 📋 Project Structure

```
TravelPlanner/
├── Models/                    # Data models
│   ├── ApplicationUser.cs
│   ├── Trip.cs
│   ├── Destination.cs
│   ├── Itinerary.cs
│   ├── Activity.cs
│   ├── Accommodation.cs
│   ├── Transportation.cs
│   ├── Expense.cs
│   ├── PackingItem.cs
│   ├── Attraction.cs
│   ├── Review.cs
│   ├── Favorite.cs
│   └── DestinationImage.cs
├── Controllers/               # MVC Controllers
│   ├── AccountController.cs
│   ├── DashboardController.cs
│   ├── TripController.cs
│   ├── DestinationController.cs
│   └── HomeController.cs
├── Views/                     # Razor views
│   ├── Shared/
│   ├── Home/
│   ├── Account/
│   ├── Dashboard/
│   ├── Trip/
│   └── Destination/
├── Data/                      # Database context
│   └── ApplicationDbContext.cs
├── Services/                  # Business logic
│   ├── EmailService.cs
│   └── DatabaseSeeder.cs
├── Repositories/              # Data access layer
│   ├── ITripRepository.cs
│   ├── TripRepository.cs
│   ├── IDestinationRepository.cs
│   └── DestinationRepository.cs
├── wwwroot/                   # Static files
│   ├── css/
│   ├── js/
│   └── images/
└── Program.cs                 # Application entry point
```

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or Express)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
```bash
git clone <repository-url>
cd Travel
```

2. **Update Database Connection**
Edit `appsettings.json` and update the connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TravelPlannerDb;Trusted_Connection=true;Encrypt=false"
}
```

3. **Configure Email Settings**
Update email settings in `appsettings.json`:
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "Port": 587,
  "SenderEmail": "your-email@gmail.com",
  "SenderPassword": "your-app-password"
}
```

4. **Create Database**
Open Package Manager Console and run:
```bash
Add-Migration InitialCreate
Update-Database
```

5. **Run the Application**
```bash
dotnet run
```

The application will be available at `https://localhost:5001`

## 📝 Usage

### For Users
1. **Register** a new account
2. **Verify** your email
3. **Login** to your account
4. **Create a trip** and select destination
5. **Manage itinerary** and expenses
6. **Track budget** and plan activities
7. **View dashboard** for trip overview

### For Admins
- Manage destinations
- Manage users
- Approve reviews
- View analytics
- Generate reports

## 🎨 Design Features

- **Modern Color Scheme**: Purple gradient theme with Bootstrap 5
- **Responsive Layout**: Works perfectly on desktop, tablet, and mobile
- **Smooth Animations**: Subtle transitions and hover effects
- **Professional UI**: Clean, intuitive interface with great UX
- **Custom CSS**: Beautiful gradients, shadows, and effects
- **Mobile-First**: Optimized for all screen sizes

## 🔐 Security Features

- Password hashing with ASP.NET Identity
- Email verification
- Role-based authorization
- CSRF protection
- SQL injection prevention with Entity Framework Core
- Secure session management
- Input validation

## 📊 Database Schema

The database uses proper relationships:
- **One-to-Many**: User → Trips, Trip → Expenses
- **Many-to-One**: Trip → Destination
- **Cascade Deletes**: Automatic cleanup of related data
- **Proper Indexing**: Optimized for performance

## 🤝 Contributing

Contributions are welcome! Please follow these steps:
1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 📞 Support

For support, email: support@travelplanner.com
Or visit: www.travelplanner.com

## 👨‍💻 Developer

Built with ❤️ by Travel Planner Team

---

**Happy Travels! 🌍✈️**
