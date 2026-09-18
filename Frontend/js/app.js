// ==========================================
// Sample Data
// ==========================================

const destinations = [
    {
        id: 1,
        name: 'Cox\'s Bazar',
        country: 'Bangladesh',
        city: 'Cox\'s Bazar',
        description: 'Home to the world\'s longest natural sea beach with golden sands and sunsets.',
        image: 'https://images.unsplash.com/photo-1507525428034-b723cf961d3e?q=80&w=1200&auto=format&fit=crop&ixlib=rb-4.0.3&s=6e9c3a5d2b0c6b7d8f4a7a6b9c1d2e3f',
        rating: 4.8,
        reviews: 1024,
        category: 'Beach',
        attractions: 'Inani Beach, Himchari, Laboni Point',
        supported: true
    },
    {
        id: 2,
        name: 'Sundarbans',
        country: 'Bangladesh',
        city: 'Khulna',
        description: 'Largest mangrove forest and home to the Royal Bengal Tiger.',
        image: 'https://images.unsplash.com/photo-1549880338-65ddcdfd017b?q=80&w=1200&auto=format&fit=crop&ixlib=rb-4.0.3&s=9b6a1d3c5f2a7b1c3d4e5f6a7b8c9d0e',
        rating: 4.9,
        reviews: 678,
        category: 'Nature',
        attractions: 'Mangrove Cruises, Wildlife, Sundarbans Tour',
        supported: true
    },
    {
        id: 3,
        name: 'Srimangal',
        country: 'Bangladesh',
        city: 'Srimangal',
        description: 'Tea capital of Bangladesh with rolling tea gardens and serene landscapes.',
        image: 'images/134248610275204802.jpg',
        rating: 4.7,
        reviews: 312,
        category: 'Hill',
        attractions: 'Tea Gardens, Lawachara National Park, Baikka Beel',
        supported: true
    },
    {
        id: 4,
        name: 'Sylhet',
        country: 'Bangladesh',
        city: 'Sylhet',
        description: 'Lush green tea estates, waterfalls and the gateway to northeastern Bangladesh.',
        image: 'images/Sylhet-Scenic-Tour.jpg',
        rating: 4.6,
        reviews: 289,
        category: 'Hill',
        attractions: 'Ratargul Swamp Forest, Jaflong, Lalakhal',
        supported: true
    },
    {
        id: 5,
        name: 'Saint Martin\'s Island',
        country: 'Bangladesh',
        city: 'Teknaf',
        description: 'A small coral island known for clear blue waters and marine life.',
        image: 'images/beach-saint-martins-island-bangladesh.jpg',
        rating: 4.8,
        reviews: 412,
        category: 'Island',
        attractions: 'Coral Beach, Snorkeling, Glass-bottom boats',
        supported: false
    }
];

// Apply admin overrides from localStorage (adminDestinations)
(function applyAdminOverrides() {
    try {
        const overrides = getFromLocalStorage('adminDestinations') || {};
        if (!overrides || Object.keys(overrides).length === 0) return;

        // Apply overrides to the in-memory destinations array
        for (let i = 0; i < destinations.length; i++) {
            const d = destinations[i];
            const over = overrides[d.id];
            if (!over) continue;
            if (over.deleted) {
                // mark deleted by setting a flag; other code should respect this
                d._deleted = true;
                continue;
            }
            // override supported flag if provided
            if (typeof over.supported !== 'undefined') d.supported = over.supported;
            // allow other override fields in future
            Object.keys(over).forEach(k => {
                if (k === 'supported' || k === 'deleted') return;
                d[k] = over[k];
            });
        }
        // Remove deleted entries from array copy used by UI by filtering at render time
    } catch (e) {
        console.error('Failed to apply admin overrides', e);
    }
})();

// Show admin link in navbar when adminSession present
(function showAdminNav() {
    try {
        const navLi = document.getElementById('navAdminLi');
        if (!navLi) return;
        const isAdmin = localStorage.getItem('adminSession') === 'true';
        if (isAdmin) navLi.classList.remove('d-none'); else navLi.classList.add('d-none');
    } catch (e) { /* ignore */ }
})();

// ==========================================
// Global Functions
// ==========================================

function showNotification(message, type = 'success') {
    const alertClass = type === 'success' ? 'alert-success' : 'alert-danger';
    const alertHTML = `
        <div class="alert alert-dismissible fade show ${alertClass} position-fixed top-0 end-0 m-3" style="z-index: 1000;" role="alert">
            <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    document.body.insertAdjacentHTML('beforeend', alertHTML);
    setTimeout(() => {
        const alert = document.querySelector('.position-fixed.alert');
        if (alert) alert.remove();
    }, 3000);
}

function saveToLocalStorage(key, data) {
    localStorage.setItem(key, JSON.stringify(data));
}

function getFromLocalStorage(key) {
    const data = localStorage.getItem(key);
    return data ? JSON.parse(data) : null;
}

function isLoggedIn() {
    return getFromLocalStorage('currentUser') !== null;
}

function getCurrentUser() {
    return getFromLocalStorage('currentUser');
}

function logout() {
    localStorage.removeItem('currentUser');
    window.location.href = 'index.html';
}

function formatCurrency(amount) {
    // Format as Bangladeshi Taka with symbol prefix
    const num = Number(amount) || 0;
    return '৳' + num.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function formatDate(date) {
    return new Date(date).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });
}

function getDateOnly(value) {
    if (!value) return null;
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return null;
    return new Date(date.getFullYear(), date.getMonth(), date.getDate());
}

function renderTripNotifications(trips, elementId = 'tripNotificationBar') {
    const container = document.getElementById(elementId);
    if (!container || !Array.isArray(trips)) return;

    const today = new Date();
    const todayOnly = new Date(today.getFullYear(), today.getMonth(), today.getDate());
    const reminders = trips.map(trip => {
        const endDate = getDateOnly(trip.endDate);
        if (!endDate) return null;
        const daysLeft = Math.ceil((endDate - todayOnly) / 86400000);
        return daysLeft >= 0 && daysLeft <= 3 ? { trip, daysLeft } : null;
    }).filter(Boolean);

    if (reminders.length === 0) {
        container.classList.add('d-none');
        container.innerHTML = '';
        return;
    }

    const message = reminders.length === 1
        ? `${reminders[0].trip.title || 'Your trip'} ends ${reminders[0].daysLeft === 0 ? 'today' : `in ${reminders[0].daysLeft} day${reminders[0].daysLeft === 1 ? '' : 's'}`}.`
        : `${reminders.length} trips are ending within the next 3 days.`;

    container.classList.remove('d-none');
    container.innerHTML = `
        <div class="trip-notification-content">
            <span class="trip-notification-icon"><i class="fas fa-bell"></i></span>
            <div><strong>Trip reminder</strong><span>${message}</span></div>
            <a href="trips.html" class="btn btn-sm btn-light ms-auto">Review trips</a>
        </div>
    `;
}

function notificationKey(email) {
    return `travelNotifications:${String(email || '').toLowerCase()}`;
}

function normalizeNotifications(items) {
    if (!Array.isArray(items)) return [];
    return items
        .filter(item => item && typeof item === 'object')
        .map(item => ({
            ...item,
            read: typeof item.read === 'boolean' ? item.read : false,
            readAt: item.readAt || null
        }));
}

function getUnreadNotificationCount(email) {
    if (!email) return 0;
    const notifications = normalizeNotifications(getFromLocalStorage(notificationKey(email)) || []);
    return notifications.filter(notification => !notification.read).length;
}

function markNotificationsAsRead(email) {
    if (!email) return 0;
    const key = notificationKey(email);
    const notifications = normalizeNotifications(getFromLocalStorage(key) || []);
    if (!notifications.length) return 0;

    const updatedNotifications = notifications.map(notification => ({
        ...notification,
        read: true,
        readAt: new Date().toISOString()
    }));

    saveToLocalStorage(key, updatedNotifications);
    return 0;
}

function addUserNotification(email, message, type = 'info') {
    if (!email || !message) return;
    const key = notificationKey(email);
    const notifications = normalizeNotifications(getFromLocalStorage(key) || []);
    notifications.unshift({
        id: Date.now(),
        message,
        type,
        createdAt: new Date().toISOString(),
        read: false,
        readAt: null
    });
    saveToLocalStorage(key, notifications.slice(0, 20));
}

function notifyAllUsers(message, type = 'info') {
    const recipients = new Set();
    const currentUser = getCurrentUser();
    if (currentUser?.email) recipients.add(currentUser.email);
    (getFromLocalStorage('users') || []).forEach(user => {
        if (user.email) recipients.add(user.email);
    });
    recipients.forEach(email => addUserNotification(email, message, type));
}

function renderUserNotificationBar() {
    const user = getCurrentUser();
    const portalPages = ['dashboard-page', 'trips-page', 'profile-page', 'destinations-page', 'create-trip-page'];
    const isPortalPage = portalPages.some(page => document.body.classList.contains(page));
    if ((!user?.email && !isPortalPage) || document.getElementById('userNotificationBar')) return;

    const notifications = user?.email ? (getFromLocalStorage(notificationKey(user.email)) || []) : [];
    const notificationItems = notifications.length > 0
        ? notifications.slice(0, 3).map(notification => `
                    <div class="user-notification-item">
                        <span>${notification.message}</span>
                        <button type="button" class="user-notification-dismiss" data-notification-id="${notification.id}" aria-label="Dismiss notification">&times;</button>
                    </div>
                `).join('')
        : '<div class="user-notification-item"><span>You are all caught up. No new notifications.</span></div>';

    const bar = document.createElement('div');
    bar.id = 'userNotificationBar';
    bar.className = 'user-notification-bar';
    bar.setAttribute('role', 'status');
    bar.setAttribute('aria-live', 'polite');
    bar.innerHTML = `
        <div class="user-notification-inner">
            <span class="user-notification-bell"><i class="fas fa-bell"></i></span>
            <div class="user-notification-list">
                ${notificationItems}
            </div>
        </div>
    `;

    const navbar = document.querySelector('nav');
    if (navbar) navbar.insertAdjacentElement('afterend', bar);
    else document.body.prepend(bar);

    bar.addEventListener('click', event => {
        if (!event.target.closest('.user-notification-dismiss')) openNotificationPanel();
    });

    bar.addEventListener('click', event => {
        const dismissButton = event.target.closest('.user-notification-dismiss');
        if (!dismissButton) return;
        const remaining = notifications.filter(item => String(item.id) !== dismissButton.dataset.notificationId);
        if (user?.email) saveToLocalStorage(notificationKey(user.email), remaining);
        dismissButton.closest('.user-notification-item')?.remove();
        if (remaining.length === 0) bar.remove();
    });
}

function openNotificationPanel() {
    const panel = document.getElementById('notificationPanel');
    if (panel) panel.classList.toggle('d-none');
}

function renderNotificationNavItem(forceRefresh = false) {
    const navList = document.querySelector('.navbar-nav');
    if (!navList) return;

    const user = getCurrentUser();
    const publicPage = /login|register|forgot-password|reset-password|admin/.test(window.location.pathname);
    if (!user?.email) return;

    const existingItem = document.getElementById('notificationNavItem');
    if (existingItem && !forceRefresh) return;
    if (existingItem && forceRefresh) existingItem.remove();

    const notifications = normalizeNotifications(getFromLocalStorage(notificationKey(user.email)) || []);
    const unreadCount = notifications.filter(notification => !notification.read).length;
    const notificationItems = notifications.length > 0
        ? notifications.map(notification => `
            <div class="notification-panel-item ${notification.read ? 'read' : 'unread'}">
                <i class="fas fa-circle-info"></i>
                <span>${notification.message}</span>
            </div>
        `).join('')
        : '<div class="notification-panel-empty">No new notifications</div>';

    navList.insertAdjacentHTML('beforeend', `
        <li class="nav-item notification-nav-item" id="notificationNavItem">
            <button type="button" class="notification-nav-button" aria-label="Open notifications">
                <i class="fas fa-bell"></i>
                ${unreadCount > 0 ? `<span class="notification-count">${unreadCount > 9 ? '9+' : unreadCount}</span>` : ''}
            </button>
            <div id="notificationPanel" class="notification-panel d-none">
                <div class="notification-panel-header">
                    <strong>Notifications</strong>
                    <span>${unreadCount} unread</span>
                </div>
                <div class="notification-panel-list">${notificationItems}</div>
            </div>
        </li>
    `);

    document.querySelector('.notification-nav-button')?.addEventListener('click', event => {
        event.stopPropagation();
        if (user?.email) {
            markNotificationsAsRead(user.email);
            renderNotificationNavItem(true);
        }
        openNotificationPanel();
    });
}

// ==========================================
// Page Load Functions
// ==========================================

function renderCategoryFilters() {
    const filtersContainer = document.getElementById('categoryFilters');
    if (!filtersContainer) return;

    const categories = Array.from(new Set((destinations || []).map(d => d.category)));
    categories.unshift('All');

    filtersContainer.innerHTML = categories.map(cat => `
        <button class="btn btn-sm btn-outline-secondary filter-chip" data-cat="${cat}">${cat}</button>
    `).join('');

    filtersContainer.querySelectorAll('.filter-chip').forEach(btn => {
        btn.addEventListener('click', () => {
            const cat = btn.getAttribute('data-cat');
            document.querySelectorAll('.filter-chip').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            if (cat === 'All') loadPopularDestinations();
            else loadPopularDestinations(destinations.filter(d => d.category === cat));
            setTimeout(initAnimations, 50);
        });
    });

    const first = filtersContainer.querySelector('.filter-chip');
    if (first) first.classList.add('active');
}

function bindHeroSearch() {
    const form = document.getElementById('heroSearchForm');
    const input = document.getElementById('heroSearchInput');
    if (!form || !input) return;

    form.addEventListener('submit', (e) => {
        e.preventDefault();
        const q = input.value.trim().toLowerCase();
        if (!q) {
            loadPopularDestinations();
            return;
        }
        const results = destinations.filter(d => (
            d.name.toLowerCase().includes(q) ||
            d.country.toLowerCase().includes(q) ||
            d.city.toLowerCase().includes(q) ||
            d.attractions.toLowerCase().includes(q)
        ));
        if (results.length === 0) showNotification('No results found', 'danger');
        loadPopularDestinations(results);
        setTimeout(initAnimations, 50);
    });
}

function bindNewsletter() {
    const form = document.getElementById('newsletterForm');
    const input = document.getElementById('newsletterEmail');
    if (!form || !input) return;

    form.addEventListener('submit', (e) => {
        e.preventDefault();
        const email = input.value.trim();
        if (!email || !/^\S+@\S+\.\S+$/.test(email)) {
            showNotification('Please enter a valid email address', 'danger');
            return;
        }

        const list = getFromLocalStorage('newsletter') || [];
        if (!list.includes(email)) {
            list.push(email);
            saveToLocalStorage('newsletter', list);
            showNotification('Thanks for subscribing!', 'success');
            input.value = '';
        } else {
            showNotification('This email is already subscribed', 'danger');
        }
    });
}

document.addEventListener('DOMContentLoaded', function() {
    if ('serviceWorker' in navigator) {
        window.addEventListener('load', () => {
            navigator.serviceWorker.register('/service-worker.js').catch(err => console.warn('SW registration failed:', err));
        });
    }

    loadPopularDestinations();
    updateNavigation();
    renderNotificationNavItem();
    initAnimations();
    if (typeof renderCategoryFilters === 'function') {
        renderCategoryFilters();
    }
    if (typeof bindHeroSearch === 'function') {
        bindHeroSearch();
    }
    if (typeof bindNewsletter === 'function') {
        bindNewsletter();
    }
});

function updateNavigation() {
    const user = getCurrentUser();
    const loginBtn = document.querySelector('.btn-primary[href="login.html"]');
    const logoutBtn = document.querySelector('.btn-danger');
    
    if (loginBtn && user) {
        loginBtn.style.display = 'none';
    }
}

function loadPopularDestinations(list = null) {
    const container = document.getElementById('popularDestinations');
    if (!container) return;

    const data = (list && Array.isArray(list)) ? list : destinations.slice(0, 6);

    const favorites = getFromLocalStorage('favorites') || [];

    container.innerHTML = data.map(dest => `
        <div class="col-12 col-sm-6 col-md-4 col-lg-3">
            <div class="card border-0 shadow-sm h-100 hover-card">
                <div class="position-relative">
                    <img loading="lazy" src="${dest.image}" class="card-img-top" style="height: 200px; object-fit: cover;" alt="${dest.name}" />
                    <div class="position-absolute top-0 end-0 bg-danger text-white p-2 m-2 rounded">
                        <i class="fas fa-star"></i> ${dest.rating}
                    </div>
                    <button class="btn btn-sm btn-light position-absolute top-0 start-0 m-2 favorite-btn" data-id="${dest.id}" title="Toggle favorite">
                        <i class="fa-heart ${favorites.includes(dest.id) ? 'fas text-danger' : 'far text-muted'}"></i>
                    </button>
                </div>
                <div class="card-body">
                    <h5 class="card-title text-dark">${dest.name}</h5>
                    <p class="card-text text-muted small mb-2">${dest.country}</p>
                    <span class="badge bg-primary">${dest.category}</span>
                    <div class="mt-3">
                        <a href="#" class="btn btn-outline-primary btn-sm me-2" onclick="showDestinationDetails(${dest.id}); return false;">View</a>
                        <a href="#" class="btn btn-primary btn-sm" onclick="planTrip(${dest.id}); return false;">Plan</a>
                    </div>
                </div>
            </div>
        </div>
    `).join('');

    // Bind favorite buttons
    container.querySelectorAll('.favorite-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            const id = parseInt(btn.getAttribute('data-id'));
            toggleFavorite(id, btn);
        });
    });
}

function planTrip(id) {
    const destination = getDestinationByClientId(id);
    if (!destination) return;

    saveToLocalStorage('selectedDestination', destination.id);
    saveToLocalStorage('selectedDestinationName', destination.name);
    window.location.href = `create-trip.html?destinationId=${encodeURIComponent(destination.id)}&destinationName=${encodeURIComponent(destination.name)}`;
}

function getDestinationByClientId(id) {
    return destinations.find(destination => String(destination.uiId ?? destination.id) === String(id));
}

function toggleFavorite(id, btnElement) {
    const favorites = getFromLocalStorage('favorites') || [];
    const idx = favorites.indexOf(id);
    if (idx === -1) {
        favorites.push(id);
        showNotification('Added to favorites', 'success');
    } else {
        favorites.splice(idx, 1);
        showNotification('Removed from favorites', 'success');
    }
    saveToLocalStorage('favorites', favorites);

    // update icon
    const icon = btnElement.querySelector('i');
    if (icon) {
        icon.classList.toggle('fas', idx === -1);
        icon.classList.toggle('far', idx !== -1);
        icon.classList.toggle('text-danger', idx === -1);
        icon.classList.toggle('text-muted', idx !== -1);
    }
}

function showDestinationDetails(id) {
    const dest = getDestinationByClientId(id);
    if (!dest) return;

    const modal = document.getElementById('destinationModal');
    if (!modal) return;

    const modalTitle = document.getElementById('modalTitle');
    const modalBody = document.getElementById('modalBody');

    if (modalTitle) modalTitle.textContent = dest.name;
    if (modalBody) {
        modalBody.innerHTML = `
            <img src="${dest.image}" class="img-fluid rounded mb-3" alt="${dest.name}" />
            <h5 class="fw-bold">${dest.name}, ${dest.country}</h5>
            <p class="text-muted">${dest.description}</p>
            <div class="mb-3">
                <strong>Rating:</strong> ⭐ ${dest.rating}/5.0 (${dest.reviews} reviews)<br>
                <strong>Category:</strong> ${dest.category}<br>
                <strong>Main Attractions:</strong> ${dest.attractions}
            </div>
            <button class="btn btn-primary w-100" onclick="planTrip('${dest.uiId ?? dest.id}'); return false;">Plan Trip Here</button>
        `;
    }

    const bsModal = new bootstrap.Modal(modal);
    bsModal.show();
}

// Animate cards on scroll with IntersectionObserver
function initAnimations() {
    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                entry.target.classList.add('slide-in');
                entry.target.style.animationDelay = `${index * 60}ms`;
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.12 });

    // Observe cards (supports dynamically inserted cards)
    const container = document.getElementById('popularDestinations');
    if (!container) return;

    // Use a slight delay to ensure elements exist in DOM after insertion
    setTimeout(() => {
        container.querySelectorAll('.card').forEach(card => {
            card.classList.add('fade-in');
            observer.observe(card);
        });
    }, 100);
}
