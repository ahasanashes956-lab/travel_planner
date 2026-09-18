// ==========================================
// Dashboard Functions
// ==========================================

document.addEventListener('DOMContentLoaded', function() {
    const user = getCurrentUser();
    
    if (user) {
        document.getElementById('userGreeting').textContent = user.firstName;
        updateDashboardStats();
    }
});

async function updateDashboardStats() {
    try {
        const response = await fetch('/api/trips', {
            credentials: 'include',
            cache: 'no-store'
        });

        if (response.status === 401 || response.status === 403) {
            window.location.href = 'login.html';
            return;
        }

        if (!response.ok) {
            throw new Error(`Could not load trips. Server replied with ${response.status}.`);
        }

        const trips = await response.json();
    const today = new Date();
    const todayDateOnly = new Date(today.getFullYear(), today.getMonth(), today.getDate());

    const toDate = (value) => {
        if (!value) return null;
        const date = new Date(value);
        return Number.isNaN(date.getTime()) ? null : date;
    };

    const activeTrips = trips.filter(t => {
        const status = String(t.status || '').toLowerCase();
        return status !== 'completed';
    });

    const totalBudget = trips.reduce((sum, t) => sum + Number(t.budgetLimit || 0), 0);
    const totalSpent = trips.reduce((sum, t) => sum + Number(t.totalSpent || 0), 0);

    document.getElementById('totalTrips').textContent = trips.length;
    document.getElementById('activeTrips').textContent = activeTrips.length;
    document.getElementById('totalBudget').textContent = formatCurrency(totalBudget);
    document.getElementById('totalSpent').textContent = formatCurrency(totalSpent);

    const upcomingTrips = trips
        .filter(t => {
            const startDate = toDate(t.startDate);
            return startDate && startDate > todayDateOnly;
        })
        .sort((a, b) => toDate(a.startDate) - toDate(b.startDate));

    let dashboardTrips = [
        ...activeTrips.map(trip => ({ trip, label: 'Active now' })),
        ...upcomingTrips.map(trip => ({ trip, label: 'Upcoming' }))
    ];

    if (dashboardTrips.length === 0 && trips.length > 0) {
        dashboardTrips = trips
            .slice()
            .sort((a, b) => new Date(b.createdAt || 0) - new Date(a.createdAt || 0))
            .slice(0, 3)
            .map(trip => ({ trip, label: 'Saved trip' }));
    }

    const upcomingContainer = document.getElementById('upcomingTrips');
    if (dashboardTrips.length > 0) {
        upcomingContainer.innerHTML = dashboardTrips.map(({ trip, label }) => `
            <div class="d-flex justify-content-between align-items-center gap-3 mb-3 pb-3 border-bottom">
                <div class="min-width-0">
                    <div class="d-flex align-items-center gap-2 mb-1">
                        <h6 class="mb-0 fw-bold text-truncate">${trip.title || 'Untitled trip'}</h6>
                        <span class="badge ${label === 'Active now' ? 'bg-success' : 'bg-info'}">${label}</span>
                    </div>
                    <small class="text-muted d-block">${trip.destination?.name || 'Destination unavailable'}${trip.destination?.country ? `, ${trip.destination.country}` : ''}</small>
                    <small class="text-muted">${formatDate(trip.startDate)} - ${formatDate(trip.endDate)}</small>
                </div>
                <a href="trips.html?tripId=${trip.id}" class="btn btn-sm btn-outline-primary flex-shrink-0">
                    <i class="fas fa-arrow-right me-1"></i>View Details
                </a>
            </div>
        `).join('');
    } else {
        upcomingContainer.innerHTML = '<p class="text-muted">No trips yet. <a href="create-trip.html">Create one now!</a></p>';
    }

    const recentTrips = trips
        .slice()
        .sort((a, b) => new Date(b.createdAt || 0) - new Date(a.createdAt || 0))
        .slice(0, 3);

    const recentContainer = document.getElementById('recentTrips');
    if (recentTrips.length > 0) {
        recentContainer.innerHTML = recentTrips.map(trip => `
            <div class="d-flex justify-content-between align-items-center mb-3 pb-3 border-bottom">
                <div class="min-width-0">
                    <h6 class="mb-1 fw-bold text-truncate">${trip.title || 'Untitled trip'}</h6>
                    <small class="text-muted d-block">${trip.destination?.name || 'Destination unavailable'}</small>
                    <small class="text-muted">Created: ${formatDate(trip.createdAt || trip.startDate)}</small>
                </div>
                <a href="trips.html?tripId=${trip.id}" class="btn btn-sm btn-outline-success flex-shrink-0">
                    <i class="fas fa-arrow-right me-1"></i>View Details
                </a>
            </div>
        `).join('');
    } else {
        recentContainer.innerHTML = '<p class="text-muted">No trips created yet.</p>';
    }
    } catch (error) {
        console.error('Dashboard trip loading error:', error);
        document.getElementById('totalTrips').textContent = '0';
        document.getElementById('activeTrips').textContent = '0';
        document.getElementById('totalBudget').textContent = formatCurrency(0);
        document.getElementById('totalSpent').textContent = formatCurrency(0);
    }
}
