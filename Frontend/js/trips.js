// ==========================================
// Trips Functions
// ==========================================

const TRIPS_API_URL = window.location.origin === 'null' ? 'http://localhost:8000' : window.location.origin;

document.addEventListener('DOMContentLoaded', function() {
    loadTrips();
    bindExpenseForm();
    bindTripFilters();
});

function bindTripFilters() {
    const search = document.getElementById('tripSearch');
    const status = document.getElementById('tripStatusFilter');
    if (!search || !status) return;

    const applyFilters = () => {
        const query = search.value.trim().toLowerCase();
        const selectedStatus = status.value;
        const cards = document.querySelectorAll('#tripsList [data-trip-card]');
        let visibleCount = 0;

        cards.forEach(card => {
            const matchesQuery = !query || card.dataset.tripSearch.includes(query);
            const matchesStatus = selectedStatus === 'all' || card.dataset.tripStatus === selectedStatus;
            const visible = matchesQuery && matchesStatus;
            card.classList.toggle('d-none', !visible);
            if (visible) visibleCount++;
        });

        const resultCount = document.getElementById('tripResultCount');
        if (resultCount && cards.length > 0) {
            resultCount.textContent = `${visibleCount} of ${cards.length} trips`;
        }
    };

    search.addEventListener('input', applyFilters);
    status.addEventListener('change', applyFilters);
    window.applyTripFilters = applyFilters;
}

function bindExpenseForm() {
    const modal = document.getElementById('expenseModal');
    if (!modal) return;

    const form = document.getElementById('expenseForm');
    if (!form) return;

    form.addEventListener('submit', async function (event) {
        event.preventDefault();

        const tripId = document.getElementById('expenseTripId').value;
        const name = document.getElementById('expenseName').value.trim();
        const amount = parseFloat(document.getElementById('expenseAmount').value);

        if (!tripId || !name || Number.isNaN(amount) || amount <= 0) {
            showNotification('Please enter a valid expense name and amount.', 'danger');
            return;
        }

        try {
            const response = await fetch(`${TRIPS_API_URL}/api/trips/${tripId}/expenses`, {
                method: 'POST',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    name,
                    amount
                })
            });

            const data = await response.json().catch(() => ({}));

            if (!response.ok) {
                throw new Error(data.message || 'Failed to add expense');
            }

            const modalInstance = bootstrap.Modal.getInstance(modal);
            if (modalInstance) {
                modalInstance.hide();
            }
            form.reset();
            const tripTitle = document.querySelector(`[data-trip-id="${tripId}"] .card-title`)?.textContent || 'your trip';
            addUserNotification(getCurrentUser()?.email, `Expense added to ${tripTitle}.`, 'info');
            showNotification('Expense added successfully!', 'success');
            await loadTrips();
        } catch (error) {
            console.error('Add expense error:', error);
            showNotification(error.message || 'Something went wrong while adding the expense.', 'danger');
        }
    });

    document.addEventListener('click', function (event) {
        const button = event.target.closest('.add-expense-btn');
        if (!button) return;

        const tripId = button.dataset.tripId;
        document.getElementById('expenseTripId').value = tripId;
        const tripTitle = button.dataset.tripTitle || 'Trip';
        const modalTitle = document.getElementById('expenseModalLabel');
        if (modalTitle) {
            modalTitle.textContent = `Add Expense - ${tripTitle}`;
        }

        const modalInstance = bootstrap.Modal.getOrCreateInstance(modal);
        modalInstance.show();
    });
}

async function loadTrips() {
    const container = document.getElementById('tripsList');
    if (!container) return;

    const selectedTripId = Number.parseInt(new URLSearchParams(window.location.search).get('tripId') || '', 10);
    const selectedTrip = Array.isArray(window.__tripsData) ? window.__tripsData.find(trip => Number(trip.id) === selectedTripId) : null;

    const showEmptyState = () => {
        container.innerHTML = `
            <div class="col-12">
                <div class="alert alert-info text-center py-5">
                    <i class="fas fa-info-circle fa-3x mb-3 d-block"></i>
                    <h5>No trips yet!</h5>
                    <p class="mb-3">Start planning your first adventure today.</p>
                    <a href="create-trip.html" class="btn btn-primary">Create Your First Trip</a>
                </div>
            </div>
        `;
    };

    try {
        const response = await fetch(`${TRIPS_API_URL}/api/trips`, {
            credentials: 'include',
            cache: 'no-store'
        });

        const responseText = await response.text();
        const trimmedResponse = responseText.trim();
        const isJsonResponse = trimmedResponse.startsWith('{') || trimmedResponse.startsWith('[');

        if (response.status === 401 || response.status === 403 || response.redirected) {
            window.location.href = 'login.html';
            return;
        }

        if (!trimmedResponse) {
            showEmptyState();
            return;
        }

        if (!isJsonResponse) {
            if (trimmedResponse.toLowerCase().includes('login') || trimmedResponse.toLowerCase().includes('<html')) {
                window.location.href = 'login.html';
                return;
            }
            throw new Error('Unexpected response format from the server.');
        }

        if (!response.ok) {
            throw new Error(`Could not load trips. Server replied with ${response.status}.`);
        }

        let trips;
        try {
            trips = JSON.parse(trimmedResponse);
        } catch (parseError) {
            // If server returned HTML (login page or error page), redirect to login
            const lower = trimmedResponse.toLowerCase();
            if (lower.includes('login') || lower.includes('<html') || lower.includes('<!doctype')) {
                window.location.href = 'login.html';
                return;
            }

            // Unknown response — surface the original error to console and show empty state
            console.error('Failed to parse trips JSON response', parseError, trimmedResponse);
            showEmptyState();
            return;
        }
        const safeTrips = Array.isArray(trips) ? trips : [];
        window.__tripsData = safeTrips;
        const currentTrips = safeTrips;
        const duplicateKeys = new Set();
        const tripRows = currentTrips.map(trip => {
            const key = [
                (trip.title || '').trim().toLowerCase(),
                trip.destination?.id || '',
                trip.startDate ? new Date(trip.startDate).toISOString().slice(0, 10) : '',
                trip.endDate ? new Date(trip.endDate).toISOString().slice(0, 10) : ''
            ].join('|');
            const isDuplicate = duplicateKeys.has(key);
            duplicateKeys.add(key);
            return { trip, isDuplicate };
        });

        if (tripRows.length === 0) {
            showEmptyState();
            return;
        }

        container.innerHTML = `${tripRows.map(({ trip, isDuplicate }) => {
            const budgetLimit = Number(trip.budgetLimit) || 0;
            const totalSpent = Number(trip.totalSpent) || 0;
            const remainingBudget = budgetLimit - totalSpent;
            const progressPercent = budgetLimit > 0 ? Math.min(100, (totalSpent / budgetLimit) * 100) : 0;
            const isSelectedTrip = Number(trip.id) === selectedTripId;
            const tripSearchText = `${trip.title || ''} ${trip.destination?.name || ''} ${trip.destination?.country || ''}`.toLowerCase();

            return `
                <div class="col-md-6 col-lg-4 ${isSelectedTrip ? 'trip-card-selected' : ''}" data-trip-id="${trip.id}" data-trip-card data-trip-search="${tripSearchText}" data-trip-status="${(trip.status || 'planned').toLowerCase()}">
                    <div class="card border-0 shadow-sm h-100 hover-card ${isSelectedTrip ? 'border border-primary shadow' : ''}">
                        <img src="${getTripImage(trip)}" alt="${trip.destination?.name || trip.title || 'Trip'}" class="card-img-top" style="height: 200px; object-fit: cover;" onerror="this.onerror=null; this.src='/images/travel-placeholder.svg';">
                        <div class="card-body">
                            <h5 class="card-title fw-bold">${trip.title || 'Untitled trip'}</h5>
                            ${isDuplicate ? '<span class="badge bg-danger mb-2"><i class="fas fa-copy me-1"></i>Duplicate</span>' : ''}
                            <p class="card-text text-muted small">${trip.destination?.name || 'Destination unavailable'}, ${trip.destination?.country || ''}</p>

                            <div class="mb-3">
                                <small class="text-muted d-block">
                                    <i class="fas fa-calendar-alt me-1"></i>
                                    ${trip.startDate ? formatDate(trip.startDate) : 'Date not set'} - ${trip.endDate ? formatDate(trip.endDate) : 'Date not set'}
                                </small>
                                <small class="text-muted d-block">
                                    <i class="fas fa-users me-1"></i>
                                    ${trip.numberOfTravelers || 0} Travelers
                                </small>
                            </div>

                            <div class="d-flex justify-content-between align-items-center gap-2 mb-3">
                                <div>
                                    <span class="badge bg-info me-1">${trip.tripType || 'Trip'}</span>
                                    <span class="badge ${trip.status === 'Planned' ? 'bg-warning' : trip.status === 'Ongoing' ? 'bg-success' : 'bg-secondary'}">
                                        ${trip.status || 'Planned'}
                                    </span>
                                </div>

                                <button type="button" class="btn btn-sm btn-success add-expense-btn d-flex align-items-center gap-1" data-trip-id="${trip.id}" data-trip-title="${(trip.title || 'Trip').replace(/"/g, '&quot;')}">
                                    <i class="fas fa-plus"></i>
                                    <span>Expense</span>
                                </button>
                            </div>

                            <div class="d-grid gap-2 mb-3">
                                <a href="/trip/edit/${trip.id}" class="btn btn-sm btn-outline-primary">
                                    <i class="fas fa-edit me-1"></i>Edit Trip
                                </a>
                                <button type="button" class="btn btn-sm ${isDuplicate ? 'btn-outline-danger delete-duplicate-btn' : 'btn-outline-secondary delete-trip-btn'}" data-trip-id="${trip.id}">
                                    <i class="fas fa-trash me-1"></i>${isDuplicate ? 'Delete Duplicate' : 'Delete Trip'}
                                </button>
                            </div>

                            <div class="progress mb-3" style="height: 20px;">
                                <div class="progress-bar" role="progressbar" style="width: ${progressPercent}%">
                                    ${Math.round(progressPercent)}%
                                </div>
                            </div>
                            <small class="text-muted d-block">Budget: ${formatCurrency(budgetLimit)} | Spent: ${formatCurrency(totalSpent)}</small>
                            <small class="${remainingBudget < 0 ? 'text-danger' : 'text-success'} d-block mt-1">
                                Remaining: ${formatCurrency(remainingBudget)}
                            </small>
                        </div>
                    </div>
                </div>
            `;
        }).join('')}`;

        if (window.applyTripFilters) window.applyTripFilters();
    } catch (error) {
        console.error('Trip loading error:', error);

        const currentUser = getCurrentUser();
        if (!currentUser) {
            window.location.href = 'login.html';
            return;
        }

        container.innerHTML = '<div class="col-12"><div class="alert alert-danger text-center">Could not load trips from the database.</div></div>';
    }
}

document.addEventListener('click', async function(event) {
    const button = event.target.closest('.delete-duplicate-btn, .delete-trip-btn');
    if (!button) return;

    if (!window.confirm('Delete this trip permanently?')) return;

    button.disabled = true;
    try {
        const response = await fetch(`${TRIPS_API_URL}/api/trips/${button.dataset.tripId}`, {
            method: 'DELETE',
            credentials: 'include'
        });
        const data = await response.json().catch(() => ({}));
        if (!response.ok) throw new Error(data.message || 'Could not delete duplicate trip.');
        addUserNotification(getCurrentUser()?.email, 'A trip was deleted from your account.', 'info');
        showNotification('Trip deleted successfully!', 'success');
        button.closest('[data-trip-card]')?.remove();
        await loadTrips();
    } catch (error) {
        button.disabled = false;
        showNotification(error.message, 'danger');
    }
});

function getTripImage(trip) {
    if (trip.coverImageUrl) return trip.coverImageUrl;
    if (trip.destination?.imageUrl) return trip.destination.imageUrl;

    const destinationName = trip.destination?.name?.trim().toLowerCase();
    const destination = (typeof destinations !== 'undefined' && destinations || [])
        .find(item => item.id === trip.destination?.id || item.name.trim().toLowerCase() === destinationName);
    return destination?.image || '/images/travel-placeholder.svg';
}
