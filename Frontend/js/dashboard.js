// ==========================================
// Dashboard Functions
// ==========================================

document.addEventListener('DOMContentLoaded', function() {
    const user = getCurrentUser();

    bindReviewForm();
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
        const endDate = toDate(t.endDate);
        return status !== 'completed' && status !== 'ended' && (!endDate || endDate >= todayDateOnly);
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

    const dashboardTripIds = new Set();
    let dashboardTrips = [
        ...activeTrips.map(trip => ({ trip, label: 'Active now' })),
        ...upcomingTrips.map(trip => ({ trip, label: 'Upcoming' }))
    ].filter(({ trip }) => {
        if (dashboardTripIds.has(trip.id)) return false;
        dashboardTripIds.add(trip.id);
        return true;
    });

    if (dashboardTrips.length === 0 && trips.length > 0) {
        dashboardTrips = trips
            .slice()
            .sort((a, b) => new Date(b.createdAt || 0) - new Date(a.createdAt || 0))
            .slice(0, 3)
            .map(trip => ({ trip, label: 'Saved trip' }));
        dashboardTrips.forEach(({ trip }) => dashboardTripIds.add(trip.id));
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
        .sort((a, b) => new Date(b.createdAt || 0) - new Date(a.createdAt || 0));

    const recentContainer = document.getElementById('recentTrips');
    if (recentTrips.length > 0) {
        recentContainer.innerHTML = recentTrips.map(trip => {
            const isCompleted = ['completed', 'ended'].includes(String(trip.status || '').toLowerCase()) ||
                (toDate(trip.endDate) && toDate(trip.endDate) < todayDateOnly);
            const review = trip.review;
            const reviewAction = isCompleted
                ? (review
                    ? `<span class="badge bg-success"><i class="fas fa-star me-1"></i>${review.rating}/5 Reviewed</span>`
                    : `<button type="button" class="btn btn-sm btn-outline-warning review-trip-btn" data-trip-id="${trip.id}" data-trip-title="${escapeHtml(trip.title || 'Trip')}"><i class="fas fa-star me-1"></i>Rate & Review</button>`)
                : '';
            return `
            <div class="d-flex justify-content-between align-items-center mb-3 pb-3 border-bottom">
                <div class="min-width-0">
                    <h6 class="mb-1 fw-bold text-truncate">${trip.title || 'Untitled trip'}</h6>
                    <small class="text-muted d-block">${trip.destination?.name || 'Destination unavailable'}</small>
                    <small class="text-muted">Created: ${formatDate(trip.createdAt || trip.startDate)}</small>
                </div>
                <div class="d-flex align-items-center gap-2 flex-shrink-0">
                    ${reviewAction}
                    <a href="trips.html?tripId=${trip.id}" class="btn btn-sm btn-outline-success">
                        <i class="fas fa-arrow-right me-1"></i>View Details
                    </a>
                </div>
            </div>
        `;
        }).join('');
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

function bindReviewForm() {
    const modal = document.getElementById('reviewModal');
    const form = document.getElementById('reviewForm');
    if (!modal || !form) return;

    document.addEventListener('click', event => {
        const button = event.target.closest('.review-trip-btn');
        if (!button) return;

        document.getElementById('reviewTripId').value = button.dataset.tripId;
        document.getElementById('reviewModalLabel').textContent = `Review ${button.dataset.tripTitle || 'trip'}`;
        document.getElementById('reviewMessage').hidden = true;
        form.reset();
        document.getElementById('reviewTripId').value = button.dataset.tripId;
        bootstrap.Modal.getOrCreateInstance(modal).show();
    });

    form.addEventListener('submit', async event => {
        event.preventDefault();
        const submitButton = document.getElementById('reviewSubmit');
        const message = document.getElementById('reviewMessage');
        submitButton.disabled = true;

        try {
            const response = await fetch(`${window.location.origin}/api/trips/${document.getElementById('reviewTripId').value}/review`, {
                method: 'POST',
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    rating: Number(document.getElementById('reviewRating').value),
                    content: document.getElementById('reviewContent').value
                })
            });
            const data = await response.json().catch(() => ({}));
            if (!response.ok) throw new Error(data.message || 'Could not submit review.');

            message.className = 'alert alert-success mt-3 mb-0';
            message.textContent = data.message;
            message.hidden = false;
            await updateDashboardStats();
            setTimeout(() => bootstrap.Modal.getOrCreateInstance(modal).hide(), 700);
        } catch (error) {
            message.className = 'alert alert-danger mt-3 mb-0';
            message.textContent = error.message || 'Could not submit review.';
            message.hidden = false;
        } finally {
            submitButton.disabled = false;
        }
    });
}

function escapeHtml(value) {
    return String(value).replace(/[&<>'"]/g, character => ({
        '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;'
    }[character]));
}
