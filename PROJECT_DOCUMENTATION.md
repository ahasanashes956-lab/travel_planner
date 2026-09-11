# Project Connection Audit

Audit scope: frontend files under `Frontend/`, MVC/API controllers, repositories, services, `Program.cs`, and `ApplicationDbContext`. This document records the connections found in the source as of 2026-09-11. No application code was changed for this audit.

## Architecture Summary

- The application is an ASP.NET Core MVC app serving the static frontend from `Frontend/`.
- The frontend calls same-origin `/api/*` endpoints. `auth.js`, `profile.js`, `trip-form.js`, and `trips.js` fall back to `http://localhost:8000` only when opened directly from a file URL.
- API endpoints use cookie-based ASP.NET Identity authentication. API unauthenticated redirects are converted to HTTP 401 in `Program.cs`.
- Data access is SQL Server through Entity Framework Core. The configured database is `SmartTravelPlanner` on `.\SQLEXPRESS`.
- The app also has MVC view actions which are separate from the static frontend API flow.

## Frontend API Calls

| Frontend origin | Method and URL | Backend endpoint | Response/use |
|---|---|---|---|
| `Frontend/js/auth.js` registration form | POST `/api/register` | `AccountController.RegisterApi` | Creates an Identity user; frontend shows success and redirects to login. |
| `Frontend/js/auth.js` login form | POST `/api/login` | `AccountController.LoginApi` | Signs in with an Identity cookie; frontend then requests current user and redirects admin users to `admin.html` or others to `dashboard.html`. |
| `Frontend/js/auth.js` after login | GET `/api/current-user` | `AccountController.GetCurrentUser` | Reads user identity, profile fields, and roles for local state and admin routing. |
| `Frontend/js/auth.js` auth guard | GET `/api/current-user` | `AccountController.GetCurrentUser` | Determines whether protected static pages redirect to login. |
| `Frontend/js/admin.js` initialization | GET `/api/current-user` | `AccountController.GetCurrentUser` | Checks authentication and the `Admin` role before showing the admin panel. |
| `Frontend/js/admin.js` logout | POST `/api/logout` | `AccountController.LogoutApi` | Clears the Identity cookie, then redirects to login. |
| `Frontend/js/admin.js` admin load | GET `/api/admin/users` | `AdminController.GetUsers` | Loads non-admin users and statuses for approval UI. Requires Admin role. |
| `Frontend/js/admin.js` admin load | GET `/api/admin/destinations` | `AdminController.GetDestinations` | Loads published destinations for the admin library. Requires Admin role. |
| `Frontend/js/admin.js` user status | POST `/api/admin/users/{id}/status` | `AdminController.UpdateUserStatus` | Updates `IsActive` and `IsVerified`; frontend reloads admin data. |
| `Frontend/js/admin.js` create destination | POST `/api/admin/destinations` | `AdminController.CreateDestination` | Inserts a destination; frontend clears the form and reloads the list. |
| `Frontend/js/admin.js` delete destination | DELETE `/api/admin/destinations/{id}` | `AdminController.DeleteDestination` | Deletes a destination; frontend removes the row and reloads on failure. |
| `Frontend/js/dashboard.js` | GET `/api/trips` | `TripController.GetTripsApi` | Computes dashboard totals and renders active/upcoming/recent trip cards. |
| `Frontend/js/destinations.js` | GET `/api/destinations/catalog` | `DestinationController.Catalog` | Merges database destinations into static destination data and renders filters/cards. |
| `Frontend/js/profile.js` profile load | GET `/api/current-user` | `AccountController.GetCurrentUser` | Populates profile fields and photo. |
| `Frontend/js/profile.js` photo input | POST `/api/profile/photo` multipart field `photo` | `AccountController.UploadProfilePhoto` | Saves the file under `wwwroot/uploads/profiles`, updates Identity profile URL, and returns the URL. |
| `Frontend/js/profile.js` profile form | PUT `/api/profile` | `AccountController.UpdateProfileApi` | Updates profile fields and returns the updated user for local storage/UI. |
| `Frontend/js/trip-form.js` destination select | GET `/api/destinations` | `TripController.GetDestinationSummariesApi` | Supplies destination IDs/names for trip creation; static data is a fallback. |
| `Frontend/js/trip-form.js` create form | POST `/api/trips` | `TripController.CreateTripApi` | Creates a trip and returns its ID/status; frontend redirects to My Trips. |
| `Frontend/js/trip-form.js` notification fallback | GET `/api/current-user` | `AccountController.GetCurrentUser` | Obtains an email only when local current-user data is missing. |
| `Frontend/js/trips.js` list | GET `/api/trips` | `TripController.GetTripsApi` | Renders trips, budget totals, duplicate markers, edit links, and delete/expense controls. |
| `Frontend/js/trips.js` expense form | POST `/api/trips/{tripId}/expenses` | `TripController.AddExpenseApi` | Inserts an expense and increments trip spending; frontend reloads trips. |
| `Frontend/js/trips.js` delete button | DELETE `/api/trips/{tripId}` | `TripController.DeleteTripApi` | Deletes an authorized trip; frontend removes and reloads the card. |
| `Frontend/forgot-password.html` inline script | POST `/api/forgot-password` | `AccountController.ForgotPasswordApi` | Generates a reset URL and navigates to it. |
| `Frontend/reset-password.html` inline script | POST `/api/reset-password` | `AccountController.ResetPasswordApi` | Resets the Identity password using email/token and redirects to login. |

The table contains 24 call rows because current-user is intentionally called from multiple pages and flows. The service worker explicitly bypasses caching for all `/api/` requests.

## Backend Endpoint Map

### API endpoints

| Endpoint | Controller action | Service/repository/data path |
|---|---|---|
| POST `/api/register` | `AccountController.RegisterApi` | `UserManager<ApplicationUser>` -> Identity user tables. |
| POST `/api/login` | `AccountController.LoginApi` | `UserManager` role/user lookup and `SignInManager` cookie; updates user login data. |
| GET `/api/current-user` | `AccountController.GetCurrentUser` | `UserManager.GetUserAsync` and `GetRolesAsync`; no custom repository. |
| POST `/api/profile/photo` | `AccountController.UploadProfilePhoto` | Local filesystem `wwwroot/uploads/profiles` plus `UserManager.UpdateAsync`. |
| PUT `/api/profile` | `AccountController.UpdateProfileApi` | `UserManager.UpdateAsync` -> Identity user table. |
| POST `/api/forgot-password` | `AccountController.ForgotPasswordApi` | `UserManager` token generation only; does not call `IEmailService`. |
| POST `/api/reset-password` | `AccountController.ResetPasswordApi` | `UserManager.ResetPasswordAsync` -> Identity user/token data. |
| POST `/api/logout` | `AccountController.LogoutApi` | `SignInManager.SignOutAsync`; Identity cookie. |
| GET `/api/trips` | `TripController.GetTripsApi` | `ITripRepository.GetUserTripsAsync`; reads Trips, Destinations, and Expenses. |
| GET `/api/destinations` | `TripController.GetDestinationSummariesApi` | `IDestinationRepository.GetDestinationSummariesAsync`; reads Destinations. |
| POST `/api/trips` | `TripController.CreateTripApi` | `ITripRepository`, `IDestinationRepository`; reads user/destination and inserts Trips. |
| POST `/api/trips/{tripId}/expenses` | `TripController.AddExpenseApi` | `ITripRepository.GetTripByIdAsync`, `IExpenseRepository.CreateExpenseAsync`, then `ITripRepository.UpdateTripAsync`; reads/writes Trips and Expenses. |
| DELETE `/api/trips/{tripId}` | `TripController.DeleteTripApi` | `ITripRepository.DeleteTripAsync(id, userId, isAdmin)`; deletes Trips and cascades configured children. |
| GET `/api/destinations/catalog` | `DestinationController.Catalog` | `IDestinationRepository.GetAllDestinationsAsync`; reads Destinations, Attractions, Images, and related Reviews. |
| GET `/api/admin/users` | `AdminController.GetUsers` | `UserManager.Users.Include(user => user.Trips)` and role lookups; reads Identity users/roles and Trips. |
| POST `/api/admin/users/{id}/status` | `AdminController.UpdateUserStatus` | `UserManager.FindByIdAsync`, role lookup, and `UpdateAsync`; updates Identity user data. |
| GET `/api/admin/destinations` | `AdminController.GetDestinations` | `IDestinationRepository.GetAllDestinationsAsync`; reads Destinations and related collections. |
| POST `/api/admin/destinations` | `AdminController.CreateDestination` | `IDestinationRepository.CreateDestinationAsync`; inserts Destinations. |
| DELETE `/api/admin/destinations/{id}` | `AdminController.DeleteDestination` | `IDestinationRepository.DeleteDestinationAsync`; deletes Destinations and cascades configured children. |

### MVC/view endpoints

These actions are backend endpoints even though the current static frontend usually links directly to `.html` pages or calls APIs.

| Route pattern | Controller actions | Data/service path |
|---|---|---|
| `/`, `/Home/Index` | `HomeController.Index` | `IDestinationRepository.GetPopularDestinationsAsync` -> Destinations and Images. |
| `/Home/Privacy`, `/Home/Error` | `HomeController.Privacy`, `HomeController.Error` | No database/service operation. |
| `/Account/Register` GET/POST | `AccountController.Register` | Identity user creation; successful MVC registration calls `IEmailService.SendConfirmationEmailAsync`. |
| `/Account/ConfirmEmail` | `AccountController.ConfirmEmail` | Identity user lookup/confirmation/update. |
| `/Account/Login` GET/POST | `AccountController.Login` | Identity user lookup, sign-in, and last-login update. |
| `/Account/ForgotPassword` GET/POST | `AccountController.ForgotPassword` | Identity token generation and `IEmailService.SendPasswordResetEmailAsync` when user exists. |
| `/Account/ResetPassword` GET/POST | `AccountController.ResetPassword` | Identity password reset. |
| `/Account/Logout` POST | `AccountController.Logout` | Identity cookie sign-out. |
| `/Dashboard/Index` | `DashboardController.Index` | `ITripRepository.GetUserTripsAsync`, Reviews lookup; reads Trips, Expenses, Destinations, Reviews. |
| `/Dashboard/SubmitTripReview` POST | `DashboardController.SubmitTripReview` | Direct `ApplicationDbContext`; reads Trips/Reviews and inserts Reviews. |
| `/Dashboard/Profile` | `DashboardController.Profile` | Identity user lookup. |
| `/Dashboard/UpdateProfile` POST | `DashboardController.UpdateProfile` | Identity user update. |
| `/Destination/Index` | `DestinationController.Index` | Destination repository; reads Destinations, Attractions, Images. |
| `/Destination/Details/{id}` | `DestinationController.Details` | Destination repository; reads Destinations, Attractions, Images, Reviews. |
| `/Destination/Search` | `DestinationController.Search` | Destination repository search; reads Destinations and Attractions. |
| `/Trip/Index` | `TripController.Index` | Trip repository; reads user Trips and Destinations/Expenses through projection. |
| `/Trip/Create` GET/POST | `TripController.Create` | Destination repository plus trip repository; reads Destinations and inserts Trips. |
| `/Trip/Details/{id}` | `TripController.Details` | Trip repository; reads Trips, Destinations, Expenses. |
| `/Trip/Edit/{id}` GET/POST | `TripController.Edit` | Trip/destination repositories; reads and updates Trips. This is the route generated by `Frontend/js/trips.js`. |
| `/Trip/Delete/{id}` POST | `TripController.Delete` | Trip repository; deletes Trips for the current user. |

## Database Operations and Storage

### Application tables

| Table/entity | Operations found | Owning code |
|---|---|---|
| `Trips` | User trip queries; insert/update/delete; count; direct review ownership lookup. | `TripRepository`, `TripController`, `DashboardController`, `AdminController`. |
| `Destinations` | Full/list/detail/search/popular/summary reads; insert/update/delete. | `DestinationRepository`, `DestinationController`, `HomeController`, `TripController`, `AdminController`. |
| `Expenses` | Trip totals, list/detail/category totals, insert/update/delete; included in trip projections. | `ExpenseRepository`, `TripRepository`, `TripController`, `DashboardController`. |
| `Reviews` | User trip-review dictionary/read, duplicate check/read, insert; related destination/trip reads. | `DashboardController`, `DestinationRepository`, `Program.cs`. |
| `Attractions` | Included in destination list/detail/catalog/search reads; seeded. | `DestinationRepository`, `DestinationController`, `DatabaseSeeder`. |
| `DestinationImages` | Included in destination list/detail/popular reads. | `DestinationRepository`. |
| `Accommodations` | Repository CRUD methods only; no controller or frontend caller found. | `AccommodationRepository`; registered in DI by `Program.cs`. |
| `Itineraries`, `Activities`, `Transportation`, `PackingItems`, `Favorites` | DbSets and relationships exist, but no controller/repository/frontend operation was found. | `ApplicationDbContext` and model relationships only. |

### Identity tables and filesystem

- `ApplicationUser` operations use ASP.NET Identity tables, principally `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, and token-related Identity storage. Registration, login, role checks, profile updates, approval, and password reset all use `UserManager`/`SignInManager`.
- `Program.cs` runs `EnsureCreated`, seeds users/roles and destinations, and executes raw SQL against `sys.tables`, `Reviews`, `Trips`, `sys.foreign_keys`, and `sys.indexes` to add the optional review-trip relationship/index.
- Profile photos are stored as files under `wwwroot/uploads/profiles`; the URL is persisted on the Identity user. Old profile files are deleted when replaced.
- `Expense.Category`, `PaymentMethod`, `IsPaid`, `Notes`, and `CreatedAt` are ignored by EF. `Expense.Description` maps to the legacy `Name` column and `ExpenseDate` maps to `IncurredAt`.
- `Trip.TotalSpent`, `CoverImageUrl`, `IsPublic`, and `UpdatedAt` are ignored by EF. `TotalSpent` is calculated from Expenses in repository projections and `CoverImageUrl` is only used by the API/UI if supplied elsewhere.
- `Destination.City`, `State`, `Latitude`, `Longitude`, `WeatherType`, `ReviewCount`, and `IsPopular` are ignored/not mapped. Seeded values for those properties do not persist. This makes `GetPopularDestinationsAsync` unable to identify seeded popular destinations, and `SearchDestinationsAsync` references the unmapped `City` property.

## External Calls and Resource Origins

| Origin | Where it starts | How the response/resource is used |
|---|---|---|
| Gmail SMTP (`smtp.gmail.com:587`) | `Services/EmailService.cs` via MVC `AccountController.Register` and MVC `ForgotPassword` | Sends confirmation/reset HTML emails. The configured sender values are placeholders in `appsettings*.json`; the frontend API forgot-password flow does not use this service. |
| Unsplash image URLs | `DatabaseSeeder`, `DestinationController`, `AdminController`, `Frontend/js/app.js`, and page markup | Browser loads destination/hero images; controller fallback image URLs are returned in API JSON. No response data is parsed. |
| `via.placeholder.com` | `DatabaseSeeder` attraction seed data | Browser loads seeded attraction placeholder images; no response data is parsed. |
| cdnjs Bootstrap 5.3, Font Awesome 6.4, Chart.js 3.9.1 | HTML pages and `Views/Shared/_Layout.cshtml` | Browser loads CSS/JS libraries. Chart.js is loaded on the dashboard but no current frontend script was found creating a chart. |
| Google Fonts (`fonts.googleapis.com`) | HTML pages and MVC layout | Browser loads Poppins font CSS. |
| Google Maps | No call found | `GoogleMapsApiKey` is configured as a placeholder and documented in setup instructions, but no map script, SDK load, or API request uses it. |

## Broken, Unused, Duplicate, and Incomplete Connections

### Broken or risky

1. **Profile photo success path throws after a successful upload.** `Frontend/js/profile.js` calls `addUserNotification(profileData.email, ...)`, but `profileData` is local to `loadUserProfile` and is not defined in the upload handler. The API upload and database update can succeed, but the user sees an error afterward.
2. **Destination search uses an unmapped EF property.** `DestinationRepository.SearchDestinationsAsync` filters on `d.City`, while `Destination.City` is `[NotMapped]`. This is an incomplete database connection and may fail query translation; city values are not stored in the database.
3. **Popular destinations are not backed by persisted `IsPopular`.** `Destination.IsPopular` is `[NotMapped]`, so `HomeController.Index` can return no seeded popular destinations despite the seeder assigning `true`.
4. **API forgot-password bypasses email delivery.** `AccountController.ForgotPasswordApi` generates and returns a reset URL but never calls `IEmailService.SendPasswordResetEmailAsync`; the MVC endpoint does call SMTP. This is a functional split between the two frontend/backend flows.
5. **SMTP configuration is not usable as committed.** Sender email/password are placeholder values, so MVC email flows cannot authenticate until deployment configuration is supplied.
6. **Expense writes silently discard several submitted fields.** `AddExpenseApi` sets category, paid state, notes, and created time, but those properties are ignored by EF. Only the mapped legacy name, amount, currency, date, and trip relationship persist.
7. **The expense route is declared three times.** `TripController.AddExpenseApi` has three `[HttpPost]` attributes, two of which normalize to the same `/api/trips/{tripId}/expenses` route. This can create duplicate/ambiguous endpoint metadata; only one constrained route is needed.

### Unused or incomplete

1. `AccommodationRepository` is registered and implements CRUD, but no controller, API endpoint, MVC action, or frontend caller uses it.
2. `IExpenseRepository` contains list/detail/update/delete/category methods, but the application exposes only create through the trip API. There are no expense edit/delete/list endpoints in the frontend API.
3. `ITripRepository.GetAllTripsAsync`, `GetTotalExpensesAsync`, and `GetTripCountAsync` have no callers found. `DashboardController` also injects `IExpenseRepository` but never uses the field.
4. `Itineraries`, `Activities`, `Transportation`, `PackingItems`, and `Favorites` have model/DbSet relationships but no reachable application connection found.
5. `DashboardController.SubmitTripReview` is a backend MVC endpoint, but the static frontend has no review form or caller for it.
6. `DashboardController.Profile`/`UpdateProfile` are MVC profile endpoints, while the static frontend uses `/api/current-user` and `/api/profile`; the two profile flows are parallel rather than unified.
7. Admin’s older `renderDestList`, `toggleDestSupported`, `deleteDestination`, `renderUserList`, and `removeUser` functions operate on localStorage/demo data and are not used by the current server-backed render functions. They can mislead maintenance and are a duplicate admin connection model.
8. Admin destination availability/deletion has two models: old localStorage overrides in `app.js`/`admin.js`, and real server deletion through `/api/admin/destinations/{id}`. Only server deletion is used by the current rendered admin library; availability overrides remain local-only.
9. Chart.js is loaded but no current `Frontend/js` chart construction was found, so the external dependency is unused in the static dashboard.
10. `GoogleMapsApiKey` is configured but has no consumer.
11. `Program.cs` enables `AllowAnyOrigin` CORS while frontend requests include credentials. Same-origin operation works, but cross-origin credentialed requests are incompatible with wildcard origins and are not a supported connection as configured.

## Audit Conclusion

The main static frontend workflows for authentication, destinations, trips, expenses, profile data, and admin content are connected to corresponding API actions. The largest connection risks are the split demo/server admin model, the profile-photo client exception, the password-reset API not sending email, unmapped destination fields used by repository queries, and the large set of model/repository features with no endpoint or frontend connection.