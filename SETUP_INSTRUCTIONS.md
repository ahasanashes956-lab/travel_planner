# 🚀 Smart Travel Planner - Setup Instructions

## Prerequisites
- .NET 8 SDK (or later)
- SQL Server (LocalDB, Express, or Full)
- Visual Studio 2022 or VS Code
- Git (optional)

## Step-by-Step Installation

### 1. Open the Project
- Open the folder `c:\Users\ashfa\Travel Planning\Travel` in Visual Studio Code or Visual Studio 2022

### 2. Restore NuGet Packages
```bash
dotnet restore
```

### 3. Configure Database Connection
Edit `appsettings.json` and update the connection string if needed:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TravelPlannerDb;Trusted_Connection=true;MultipleActiveResultSets=true;Encrypt=false"
}
```

### 4. Configure Email Settings (Optional)
For email functionality, update `appsettings.Development.json`:
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "Port": 587,
  "SenderEmail": "your-email@gmail.com",
  "SenderPassword": "your-app-password"
}
```

**Note**: For Gmail, use an App Password (not your regular password)

### 5. Create Database
Open Package Manager Console or Terminal:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Or use Package Manager Console:
```powershell
Add-Migration InitialCreate
Update-Database
```

### 6. Run the Application
```bash
dotnet run
```

The application will be available at:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001

## 📝 Initial Setup

### Create Admin Account (Manual)
1. Register a new account
2. Update the user in database to have Admin role (optional)

### Sample Data
The database seeder automatically adds sample destinations when you first run the app.

## 🎨 Customize the Application

### Change Color Scheme
Edit `wwwroot/css/site.css` and update the CSS variables:
```css
:root {
    --primary-color: #667eea;
    --secondary-color: #764ba2;
    --success-color: #26de81;
}
```

### Add Google Maps
Update `appsettings.json` with your Google Maps API key:
```json
"GoogleMapsApiKey": "YOUR_API_KEY_HERE"
```

### Update Site Title
Edit `Views/Shared/_Layout.cshtml` to change the site name in navigation.

## 📚 Project Structure

```
Travel/
├── Models/                      # Database models
├── Controllers/                 # MVC controllers
├── Views/                       # Razor views
├── Data/                        # Database context
├── Services/                    # Business logic
├── Repositories/                # Data access
├── wwwroot/                     # Static files (CSS, JS, Images)
├── TravelPlanner.csproj         # Project file
├── Program.cs                   # Entry point
├── appsettings.json             # Configuration
└── README.md                    # Documentation
```

## 🔑 Key Features to Test

### 1. Authentication
- Register new account
- Login
- Forgot password
- Profile management

### 2. Trip Management
- Create trip
- Edit trip
- View trip details
- Delete trip

### 3. Expense Tracking
- Add expenses
- View budget progress
- Expense reports

### 4. Destinations
- Browse destinations
- Search destinations
- View attractions
- Read reviews

## 🛠️ Troubleshooting

### Database Connection Error
- Ensure SQL Server LocalDB is running
- Check connection string in `appsettings.json`
- Verify SQL Server is installed: `sqllocaldb info`

### Port Already in Use
- Change port in `launchSettings.json`
- Or stop other applications using the port

### Missing NuGet Packages
```bash
dotnet restore --force
```

### Migration Issues
```bash
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 📦 Dependencies

Core packages included:
- `Microsoft.EntityFrameworkCore` - ORM
- `Microsoft.EntityFrameworkCore.SqlServer` - SQL Server provider
- `Microsoft.AspNetCore.Identity` - Authentication
- `MailKit` - Email sending
- `MimeKit` - Email MIME handling

## 🚀 Deployment

### Publish for Production
```bash
dotnet publish -c Release -o ./publish
```

### Azure Deployment
1. Create Azure SQL Database
2. Update connection string
3. Deploy to Azure App Service

### IIS Deployment
1. Publish application
2. Create IIS website
3. Configure connection string
4. Restart IIS

## 📖 Documentation

- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [Bootstrap Documentation](https://getbootstrap.com/docs)

## 💡 Development Tips

### Enable Detailed Error Pages
In `appsettings.Development.json`, set logging level to `Debug`

### SQL Query Logging
Enable EF Core logging to see SQL queries:
```csharp
.UseSqlServer(connectionString)
.LogTo(Console.WriteLine)
```

### Live Reload
Use dotnet watch:
```bash
dotnet watch run
```

## 🐛 Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| Connection timeout | Check SQL Server is running |
| Port 5001 in use | Change port or kill process |
| Migration fails | Delete migrations folder and recreate |
| Email not sending | Check SMTP settings and firewall |
| Page not loading | Clear browser cache or restart app |

## 📞 Support

For issues or questions:
1. Check the README.md
2. Review application logs
3. Check browser console for errors
4. Contact development team

## 🎯 Next Steps

After setup:
1. ✅ Test all authentication features
2. ✅ Create a sample trip
3. ✅ Add expenses and test budget tracking
4. ✅ Explore all pages and features
5. ✅ Customize colors and branding
6. ✅ Configure email service
7. ✅ Test on different devices/browsers

---

**Happy Development! 🚀**
