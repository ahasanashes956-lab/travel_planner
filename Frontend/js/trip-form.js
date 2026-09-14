// ==========================================
// Trip Form Functions
// ==========================================

document.addEventListener('DOMContentLoaded', function() {
    loadDestinationSelect().then(prefillDestinationSelection);
});

const TRIPS_API_URL = window.location.origin === 'null' ? 'http://localhost:8000' : window.location.origin;

async function loadDestinationSelect() {
    const select = document.getElementById('destination');
    if (!select) return;

    const renderOptions = (items) => {
        const safeDestinations = Array.isArray(items) ? items : [];
        if (!safeDestinations.length) {
            select.innerHTML = '<option value="">No destinations available</option>';
            return;
        }

        select.innerHTML = '<option value="">Select a destination...</option>' + safeDestinations.map(destination =>
            `<option value="${destination.id}">${destination.name}, ${destination.country || destination.city || ''}</option>`
        ).join('');
    };

    const fallbackFromStaticData = () => {
        if (Array.isArray(window.destinations) && window.destinations.length) {
            renderOptions(window.destinations.map(d => ({ id: d.id, name: d.name, country: d.country || d.city })));
            return true;
        }

        if (Array.isArray(destinations) && destinations.length) {
            renderOptions(destinations.map(d => ({ id: d.id, name: d.name, country: d.country || d.city })));
            return true;
        }

        return false;
    };

    try {
        const response = await fetch(`${TRIPS_API_URL}/api/destinations`, {
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
            if (!fallbackFromStaticData()) {
                select.innerHTML = '<option value="">No destinations available</option>';
            }
            return;
        }

        if (!isJsonResponse) {
            const lowerText = trimmedResponse.toLowerCase();
            if (lowerText.includes('login') || lowerText.includes('<html') || lowerText.includes('<!doctype')) {
                window.location.href = 'login.html';
                return;
            }

            if (fallbackFromStaticData()) {
                return;
            }

            throw new Error('Unexpected response format from the server.');
        }

        if (!response.ok) {
            if (fallbackFromStaticData()) {
                return;
            }
            throw new Error(`Could not load destinations. Server replied with ${response.status}.`);
        }

        let destinationsFromDatabase;
        try {
            destinationsFromDatabase = JSON.parse(trimmedResponse);
        } catch (parseError) {
            console.error('Destination JSON parse error:', parseError, trimmedResponse);
            if (fallbackFromStaticData()) {
                return;
            }
            throw new Error('Destination response was not valid JSON.');
        }

        const safeDestinations = Array.isArray(destinationsFromDatabase) ? destinationsFromDatabase : [];
        if (!safeDestinations.length && fallbackFromStaticData()) {
            return;
        }

        renderOptions(safeDestinations);
    } catch (error) {
        console.error('Destination loading error:', error);
        if (!fallbackFromStaticData()) {
            select.innerHTML = '<option value="">Destinations unavailable</option>';
            showNotification('Could not load destinations from the database.', 'error');
        }
    }
}

function prefillDestinationSelection() {
    const select = document.getElementById('destination');
    const hint = document.getElementById('destinationHint');
    if (!select) return;

    const query = new URLSearchParams(window.location.search);
    const queryDestinationId = query.get('destinationId');
    const selectedId = queryDestinationId || getFromLocalStorage('selectedDestination');
    const selectedName = query.get('destinationName')
        || getFromLocalStorage('selectedDestinationName');

    // The selected name is authoritative because static and database IDs can differ.
    const normalizedName = String(selectedName || '').trim().toLowerCase();
    let option = normalizedName
        ? Array.from(select.options).find(item => item.textContent.trim().toLowerCase().startsWith(`${normalizedName},`))
        : null;

    if (!option && !selectedName && selectedId) {
        option = Array.from(select.options).find(item => String(item.value) === String(selectedId));
    }

    if (option && selectedName) {
        select.innerHTML = '';
        select.appendChild(option);
        select.value = option.value;
        select.disabled = true;
    } else if (option) {
        select.value = option.value;
    } else if (queryDestinationId && selectedName) {
        // Preserve the selected destination even when it is not yet in the API list.
        const selectedOption = document.createElement('option');
        selectedOption.value = queryDestinationId;
        selectedOption.textContent = `${selectedName}`;
        select.innerHTML = '';
        select.appendChild(selectedOption);
        select.value = queryDestinationId;
        select.disabled = true;
    }

    if (hint && selectedName) {
        hint.classList.remove('d-none');
        hint.innerHTML = `<i class="fas fa-route me-2"></i>Trip planning is ready for <strong>${selectedName}</strong>. You can adjust the details below.`;
    }
}

function getTodayDateString() {
    const today = new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const day = String(today.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}

function isDateBefore(dateValue, compareValue) {
    if (!dateValue || !compareValue) return false;

    const candidateDate = new Date(`${dateValue}T00:00:00`);
    const compareDate = new Date(`${compareValue}T00:00:00`);

    return Number.isNaN(candidateDate.getTime()) || Number.isNaN(compareDate.getTime())
        ? false
        : candidateDate < compareDate;
}

document.getElementById('createTripForm')?.addEventListener('submit', async function(e) {
    e.preventDefault();

    const startDate = document.getElementById('startDate').value;
    const endDate = document.getElementById('endDate').value;
    const todayDate = getTodayDateString();

    if (!startDate || !endDate) {
        showNotification('Please select both start and end dates.', 'error');
        return;
    }

    if (isDateBefore(startDate, todayDate) || isDateBefore(endDate, todayDate)) {
        showNotification('Trip dates must be today or later.', 'error');
        return;
    }

    if (isDateBefore(endDate, startDate)) {
        showNotification('End date cannot be before the start date.', 'error');
        return;
    }

    const submitButton = this.querySelector('button[type="submit"]');
    submitButton.disabled = true;

    const trip = {
        title: document.getElementById('tripTitle').value.trim(),
        destinationId: parseInt(document.getElementById('destination').value, 10),
        tripType: document.getElementById('tripType').value,
        startDate,
        endDate,
        numberOfTravelers: parseInt(document.getElementById('travelers').value, 10),
        budgetLimit: parseFloat(document.getElementById('budget').value),
        description: document.getElementById('description').value.trim()
    };

    try {
        const response = await fetch(`${TRIPS_API_URL}/api/trips`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            cache: 'no-store',
            body: JSON.stringify(trip)
        });
        const responseText = await response.text();
        let data = {};
        try {
            data = responseText ? JSON.parse(responseText) : {};
        } catch {
            throw new Error('The server returned an unexpected response.');
        }
        if (!response.ok) {
            showNotification(data.message || 'Trip creation failed.', 'error');
            return;
        }

        let notificationEmail = getCurrentUser()?.email;
        if (!notificationEmail) {
            const currentUserResponse = await fetch(`${TRIPS_API_URL}/api/current-user`, { credentials: 'include' });
            const currentUser = await currentUserResponse.json().catch(() => ({}));
            notificationEmail = currentUser.email;
        }
        addUserNotification(notificationEmail, `Trip "${trip.title}" was created successfully.`, 'success');
        showNotification('Trip created successfully! Redirecting...');
        setTimeout(() => window.location.href = `trips.html?cache=${Date.now()}`, 900);
    } catch (error) {
        console.error('Trip creation error:', error);
        showNotification('Trip creation error: ' + error.message, 'error');
    } finally {
        submitButton.disabled = false;
    }
});
