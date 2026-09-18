# API Routes Reference

## Authentication Routes
- `GET /account/register` - Registration page
- `POST /account/register` - Register new user
- `GET /account/login` - Login page
- `POST /account/login` - Login user
- `GET /account/logout` - Logout user
- `GET /account/forgotpassword` - Forgot password page
- `POST /account/forgotpassword` - Send reset email
- `GET /account/resetpassword?token=xxx` - Reset password page
- `POST /account/resetpassword` - Reset password

## Dashboard Routes
- `GET /dashboard` - Dashboard home
- `GET /dashboard/profile` - View profile
- `POST /dashboard/profile` - Update profile

## Trip Routes
- `GET /trip` - List all user trips
- `GET /trip/create` - Create trip form
- `POST /trip/create` - Create new trip
- `GET /trip/details/{id}` - Trip details
- `GET /trip/edit/{id}` - Edit trip form
- `POST /trip/edit/{id}` - Update trip
- `POST /trip/delete/{id}` - Delete trip

## Destination Routes
- `GET /destination` - List all destinations
- `GET /destination/details/{id}` - Destination details
- `GET /destination/search?query=xxx` - Search destinations

## Home Routes
- `GET /` - Home page
- `GET /home/privacy` - Privacy policy
- `GET /home/error` - Error page

## API Response Codes
- `200 OK` - Successful request
- `201 Created` - Resource created
- `204 No Content` - Successful, no content
- `400 Bad Request` - Invalid input
- `401 Unauthorized` - Not logged in
- `403 Forbidden` - Access denied
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error
