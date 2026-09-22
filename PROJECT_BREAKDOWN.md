# Project Git Branch & Push Plan (12 Parts)

এই plan-এ পুরো Smart Travel Planner project-কে ১২টি logical part-এ ভাগ করা হয়েছে:

- **Database:** Part 01-02 = ২টি part
- **Backend:** Part 03-06 = ৪টি part
- **Frontend & Features:** Part 07-12 = ৬টি part

প্রতিটি branch latest intended base branch থেকে তৈরি করুন। একই file একাধিক part-এ commit না করে তার primary owner part-এ রাখুন।

---

## Part 01: Domain Models and Database Entities

- **Branch Name:** `feature/part-01-domain-models`
- **Focus Area:** Database / Schemas
- **Files/Folders to Commit:**
  - `Models/`
- **Description:** User, trip, destination, itinerary, activity, accommodation, transportation, expense, review, favorite, payment এবং chat-এর domain entities। এগুলো database table ও application data contract-এর ভিত্তি।
- **Git Commands:**
  ```bash
  git checkout main
  git checkout -b feature/part-01-domain-models
  git add Models/
  git commit -m "feat: define travel planner domain models"
  git push -u origin feature/part-01-domain-models
  ```

---

## Part 02: Database Context, Relationships, and Seeding

- **Branch Name:** `feature/part-02-database-setup`
- **Focus Area:** Database / Schemas
- **Files/Folders to Commit:**
  - `Data/ApplicationDbContext.cs`
  - `Services/DatabaseSeeder.cs`
  - `ERD.md`
  - `Program.cs`
  - `TravelPlanner.csproj`
  - `WebApp.csproj`
  - `appsettings.json`
  - `appsettings.Development.json`
  - `.gitignore`
  - `SETUP_INSTRUCTIONS.md`
  - `web.config`
  - `appstartup.txt`
- **Description:** SQL Server connection, EF Core DbSets, precision rules, foreign keys, cascade behavior, database provisioning, compatibility SQL patches এবং seed data এই part-এ থাকবে। Project-এ বর্তমানে startup-based `EnsureCreated` setup আছে; আলাদা `Migrations/` folder নেই।
- **Git Commands:**
  ```bash
  git checkout main
  git checkout -b feature/part-02-database-setup
  git add Data/ApplicationDbContext.cs Services/DatabaseSeeder.cs ERD.md Program.cs TravelPlanner.csproj WebApp.csproj appsettings.json appsettings.Development.json .gitignore SETUP_INSTRUCTIONS.md web.config appstartup.txt
  git commit -m "feat: configure database context and seed data"
  git push -u origin feature/part-02-database-setup
  ```

> **Security:** Real passwords, SMTP credentials, API keys এবং production connection strings commit করবেন না। User Secrets বা environment variables ব্যবহার করুন।

---

## Part 03: Authentication, Identity, and Email Backend

- **Branch Name:** `feature/part-03-auth-backend`
- **Focus Area:** Backend / Authentication
- **Files/Folders to Commit:**
  - `Controllers/AccountController.cs`
  - `Services/EmailService.cs`
  - `Services/IEmailService.cs`
- **Description:** Registration, login, logout, email confirmation, forgot password, reset password, current-user, profile update এবং profile photo upload-এর API ও business logic।
- **Git Commands:**
  ```bash
  git checkout main
  git checkout -b feature/part-03-auth-backend
  git add Controllers/AccountController.cs Services/EmailService.cs Services/IEmailService.cs
  git commit -m "feat: implement authentication and email backend"
  git push -u origin feature/part-03-auth-backend
  ```

---

## Part 04: Trip, Expense, Dashboard, and Repository Backend

- **Branch Name:** `feature/part-04-trip-backend`
- **Focus Area:** Backend / Business Logic
- **Files/Folders to Commit:**
  - `Controllers/TripController.cs`
  - `Controllers/DashboardController.cs`
  - `Repositories/ITripRepository.cs`
  - `Repositories/TripRepository.cs`
  - `Repositories/IExpenseRepository.cs`
  - `Repositories/ExpenseRepository.cs`
  - `Repositories/IAccommodationRepository.cs`
  - `Repositories/AccommodationRepository.cs`
  - `Models/ViewModels/TripCreateViewModel.cs`
- **Description:** Trip CRUD, date and budget validation, trip status, trip review, expense management, destination selection এবং dashboard data access এই part-এর backend scope।
- **Git Commands:**
  ```bash
  git checkout main
  git checkout -b feature/part-04-trip-backend
  git add Controllers/TripController.cs Controllers/DashboardController.cs Repositories/ Models/ViewModels/TripCreateViewModel.cs
  git commit -m "feat: implement trip and expense backend"
  git push -u origin feature/part-04-trip-backend
  ```

---

## Part 05: Destination and Admin Management Backend

- **Branch Name:** `feature/part-05-destination-admin-backend`
- **Focus Area:** Backend / Business Logic
- **Files/Folders to Commit:**
  - `Controllers/DestinationController.cs`
  - `Controllers/AdminController.cs`
  - `Repositories/IDestinationRepository.cs`
  - `Repositories/DestinationRepository.cs`
- **Description:** Destination catalog, search, published destination details, attractions/images/reviews data access এবং Admin user/destination management, publishing ও deletion API এই part-এ থাকবে।
- **Git Commands:**
  ```bash
  git checkout main
  git checkout -b feature/part-05-destination-admin-backend
  git add Controllers/DestinationController.cs Controllers/AdminController.cs Repositories/IDestinationRepository.cs Repositories/DestinationRepository.cs
  git commit -m "feat: add destination and admin backend"
  git push -u origin feature/part-05-destination-admin-backend
  ```

---

## Part 06: Payment and AI Chat Backend

- **Branch Name:** `feature/part-06-payment-chat-backend`
- **Focus Area:** Backend / External API Integration
- **Files/Folders to Commit:**
  - `Controllers/PaymentController.cs`
  - `Controllers/ChatController.cs`
  - `Services/ChatService.cs`
  - `Services/IChatService.cs`
- **Description:** Trip/expense payment initiation, mock payment result handling, payment status updates, authenticated AI chat endpoint, request validation, timeout handling এবং external AI client logic। Payment এবং chat model/configuration Part 01-02-এর foundation থেকে আসবে।
- **Git Commands:**
  ```bash
  git checkout main
  git checkout -b feature/part-06-payment-chat-backend
  git add Controllers/PaymentController.cs Controllers/ChatController.cs Services/ChatService.cs Services/IChatService.cs
  git commit -m "feat: implement payment and AI chat backend"
  git push -u origin feature/part-06-payment-chat-backend
  ```

---

## Part 07: Frontend Shell, Public Pages, and Shared State

- **Branch Name:** `feature/part-07-frontend-shell`
- **Focus Area:** Frontend / UI
- **Files/Folders to Commit:**
  - `Frontend/index.html`
  - `Frontend/privacy.html`
  - `Frontend/js/app.js`
  - `Frontend/css/style.css`
  - `Frontend/css/ui-polish.css`
  - `Frontend/manifest.webmanifest`
  - `Frontend/service-worker.js`
  - `Frontend/images/`
  - `Frontend/icon.svg`
- **Description:** Public landing page, shared navigation/state, global responsive styling, PWA manifest, service worker এবং common image assets-এর frontend foundation।
- **Git Commands:**
  ```bash
  git checkout main
  git checkout -b feature/part-07-frontend-shell
  git add Frontend/index.html Frontend/privacy.html Frontend/js/app.js Frontend/css/style.css Frontend/css/ui-polish.css Frontend/manifest.webmanifest Frontend/service-worker.js Frontend/images/ Frontend/icon.svg
  git commit -m "feat: build frontend shell and shared UI"
  git push -u origin feature/part-07-frontend-shell
  ```

---

## Part 08: Authentication and Profile Frontend Features

- **Branch Name:** `feature/part-08-frontend-auth-profile`
- **Focus Area:** Frontend / Features and API Integration
- **Files/Folders to Commit:**
  - `Frontend/login.html`
  - `Frontend/register.html`
  - `Frontend/forgot-password.html`
  - `Frontend/reset-password.html`
  - `Frontend/profile.html`
  - `Frontend/js/auth.js`
  - `Frontend/js/profile.js`
- **Description:** Login, registration, logout, password reset, authentication guard, current-user state, profile update এবং profile photo upload-এর UI ও API integration।
- **Git Commands:**
  ```bash
  git checkout main
  git checkout -b feature/part-08-frontend-auth-profile
  git add Frontend/login.html Frontend/register.html Frontend/forgot-password.html Frontend/reset-password.html Frontend/profile.html Frontend/js/auth.js Frontend/js/profile.js
  git commit -m "feat: integrate authentication and profile frontend"
  git push -u origin feature/part-08-frontend-auth-profile
  ```

---

## Part 09: Trip Planning, Dashboard, and Expense Frontend Features

- **Branch Name:** `feature/part-09-frontend-trip-dashboard`
- **Focus Area:** Frontend / Features and API Integration
- **Files/Folders to Commit:**
  - `Frontend/dashboard.html`
  - `Frontend/trips.html`
  - `Frontend/create-trip.html`
  - `Frontend/js/dashboard.js`
  - `Frontend/js/trips.js`
  - `Frontend/js/trip-form.js`
- **Description:** Dashboard statistics, trip listing, trip creation, trip status, budget display, expense actions, trip review এবং trip API integration-এর complete frontend flow।
- **Git Commands:**
  ```bash
  git checkout main
  git checkout -b feature/part-09-frontend-trip-dashboard
  git add Frontend/dashboard.html Frontend/trips.html Frontend/create-trip.html Frontend/js/dashboard.js Frontend/js/trips.js Frontend/js/trip-form.js
  git commit -m "feat: add trip dashboard and expense frontend"
  git push -u origin feature/part-09-frontend-trip-dashboard
  ```

---

## Part 10: Destination Explorer Frontend Feature

- **Branch Name:** `feature/part-10-frontend-destinations`
- **Focus Area:** Frontend / Features and API Integration
- **Files/Folders to Commit:**
  - `Frontend/destinations.html`
  - `Frontend/js/destinations.js`
- **Description:** Destination catalog, search/filter, destination cards, ratings, attractions, images এবং destination catalog API integration এই feature branch-এ থাকবে।
- **Git Commands:**
  ```bash
  git checkout main
  git checkout -b feature/part-10-frontend-destinations
  git add Frontend/destinations.html Frontend/js/destinations.js
  git commit -m "feat: build destination explorer frontend"
  git push -u origin feature/part-10-frontend-destinations
  ```

---

## Part 11: Admin Console and Payment Frontend Features

- **Branch Name:** `feature/part-11-frontend-admin-payment`
- **Focus Area:** Frontend / Features and API Integration
- **Files/Folders to Commit:**
  - `Frontend/admin.html`
  - `Frontend/js/admin.js`
  - `Frontend/css/admin.css`
- **Description:** Admin authorization UI, user status management, destination create/delete/publish controls, payment overview এবং admin API integration।
- **Git Commands:**
  ```bash
  git checkout main
  git checkout -b feature/part-11-frontend-admin-payment
  git add Frontend/admin.html Frontend/js/admin.js Frontend/css/admin.css
  git commit -m "feat: add admin and payment frontend"
  git push -u origin feature/part-11-frontend-admin-payment
  ```

---

## Part 12: AI Chat UI, MVC Views, and Final Documentation

- **Branch Name:** `feature/part-12-frontend-chat-mvc-docs`
- **Focus Area:** Frontend / Features and Presentation
- **Files/Folders to Commit:**
  - `Frontend/css/chatbot.css`
  - `Frontend/js/chatbot.js`
  - `Frontend/README.md`
  - `Frontend/start.bat`
  - `Frontend/start.ps1`
  - `Frontend/start.sh`
  - `Views/`
  - `Controllers/HomeController.cs`
  - `wwwroot/`
  - `API_ROUTES.md`
  - `PROJECT_DOCUMENTATION.md`
  - `README.md`
  - `PROJECT_BREAKDOWN.md`
- **Description:** AI travel assistant widget, Razor MVC fallback pages, shared MVC assets, home presentation, frontend launcher scripts এবং final project documentation এই part-এ থাকবে।
- **Git Commands:**
  ```bash
  git checkout main
  git checkout -b feature/part-12-frontend-chat-mvc-docs
  git add Frontend/css/chatbot.css Frontend/js/chatbot.js Frontend/README.md Frontend/start.bat Frontend/start.ps1 Frontend/start.sh Views/ Controllers/HomeController.cs wwwroot/ API_ROUTES.md PROJECT_DOCUMENTATION.md README.md PROJECT_BREAKDOWN.md
  git commit -m "feat: complete chat UI MVC views and documentation"
  git push -u origin feature/part-12-frontend-chat-mvc-docs
  ```

---

## Recommended Integration Order

1. Merge Part 01 and Part 02 for models, schema relationships, database setup, and seed data.
2. Merge Parts 03-06 for authentication, trip, destination, admin, payment, and chat APIs.
3. Merge Part 07 for the shared frontend foundation.
4. Merge Parts 08-12 for authentication, trips, destinations, admin, chat UI, MVC views, and documentation.
5. Validate after integration:
   ```bash
   dotnet restore
   dotnet build .\TravelPlanner.csproj
   dotnet run --project .\TravelPlanner.csproj
   ```
6. Open the application at `http://localhost:8000`.

## Branching and Security Notes

- Create each branch from the latest intended base branch.
- Merge dependency branches before testing dependent frontend branches.
- Review `git diff --cached` before every commit.
- Never commit `bin/`, `obj/`, user uploads, credentials, passwords, or API keys.
- The application currently provisions the database during startup. Add and review EF migrations separately before production use.