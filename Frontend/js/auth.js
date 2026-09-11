// ==========================================
// Authentication - Backend Connected
// ==========================================

const API_BASE_URL = window.location.origin === 'null' ? 'http://localhost:8000' : window.location.origin;

// Handle Registration
document.getElementById('registerForm')?.addEventListener('submit', async function(e) {
    e.preventDefault();

    const submitButton = this.querySelector('button[type="submit"]');
    if (submitButton) {
        submitButton.disabled = true;
        submitButton.textContent = 'Registering...';
    }
    
    const firstName = document.getElementById('firstName').value;
    const lastName = document.getElementById('lastName').value;
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
    const confirmPassword = document.getElementById('confirmPassword').value;

    if (password !== confirmPassword) {
        showNotification('Passwords do not match!', 'error');
        return;
    }

    if (password.length < 6) {
        showNotification('Password must be at least 6 characters!', 'error');
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/api/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                firstName,
                lastName,
                email,
                password,
                confirmPassword
            })
        });

        const data = await response.json().catch(() => ({}));

        if (response.ok) {
            addUserNotification(email, 'Your account was created successfully. Welcome to Travel Planner!', 'success');
            showNotification('Registration successful! Redirecting to login...');
            setTimeout(() => {
                window.location.href = 'login.html';
            }, 2000);
        } else {
            const message = response.status === 409
                ? 'This email is already registered. Please use another email or log in.'
                : (data.message || 'Registration failed!');
            showNotification(message, 'error');
        }
    } catch (error) {
        console.error('Registration error:', error);
        showNotification('Registration error: ' + error.message, 'error');
    } finally {
        if (submitButton) {
            submitButton.disabled = false;
            submitButton.textContent = 'Register';
        }
    }
});

// Handle login choice modal buttons
document.getElementById('loginAsAdminBtn')?.addEventListener('click', function() {
    document.getElementById('loginEmail').value = 'admin@travelplannerbd.com';
    document.getElementById('loginPassword').value = 'Admin@2026!';
    document.querySelector('#loginForm button[type="submit"]').textContent = 'Login as Admin';
});

// Handle Login
document.getElementById('loginForm')?.addEventListener('submit', async function(e) {
    e.preventDefault();
    
    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;
    const rememberMe = document.getElementById('rememberMe')?.checked || false;

    try {
        const response = await fetch(`${API_BASE_URL}/api/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include', // Include cookies for session
            body: JSON.stringify({
                email,
                password,
                rememberMe
            })
        });

        if (response.ok) {
            addUserNotification(email, 'You signed in successfully. Welcome back!', 'success');
            // Store current user info locally
            const userResponse = await fetch(`${API_BASE_URL}/api/current-user`, {
                credentials: 'include'
            });
            
            if (userResponse.ok) {
                const user = await userResponse.json();
                const isAdmin = Array.isArray(user.roles) && user.roles.includes('Admin');
                const pendingNotifications = getFromLocalStorage(notificationKey(email)) || [];
                const savedNotifications = getFromLocalStorage(notificationKey(user.email)) || [];
                const mergedNotifications = [...pendingNotifications, ...savedNotifications]
                    .filter((item, index, items) => items.findIndex(candidate => candidate.id === item.id) === index)
                    .slice(0, 20);
                saveToLocalStorage(notificationKey(user.email), mergedNotifications);
                saveToLocalStorage('currentUser', { 
                    id: user.id, 
                    firstName: user.firstName, 
                    lastName: user.lastName, 
                    email: user.email 
                });

                if (isAdmin) {
                    window.location.href = 'admin.html';
                    return;
                }
            }

            showNotification('Login successful! Redirecting...');
            setTimeout(() => {
                window.location.href = 'dashboard.html';
            }, 1500);
        } else {
            showNotification('Invalid email or password!', 'error');
        }
    } catch (error) {
        console.error('Login error:', error);
        showNotification('Login error: ' + error.message, 'error');
    }
});

// Check if user is logged in
async function checkAuth() {
    try {
        const response = await fetch(`${API_BASE_URL}/api/current-user`, {
            credentials: 'include'
        });

        const data = await response.json().catch(() => ({}));
        const isAuthenticated = response.ok && data.isAuthenticated === true;
        
        if (!isAuthenticated && 
            !window.location.href.includes('index.html') && 
            !window.location.href.includes('login.html') && 
            !window.location.href.includes('register.html') && 
            !window.location.href.includes('destinations.html')) {
            window.location.href = 'login.html';
        }
    } catch (error) {
        console.error('Auth check error:', error);
        // If server is unreachable, fall back to localStorage check
        if (!isLoggedIn() && 
            !window.location.href.includes('index.html') && 
            !window.location.href.includes('login.html') && 
            !window.location.href.includes('register.html') && 
            !window.location.href.includes('destinations.html')) {
            window.location.href = 'login.html';
        }
    }
}

document.addEventListener('DOMContentLoaded', checkAuth);
