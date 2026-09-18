// ==========================================
// Destinations - Dashboard & Grid
// ==========================================

document.addEventListener('DOMContentLoaded', function() {
    initDestinationsDashboard();
});

async function initDestinationsDashboard() {
    await loadServerDestinations();
    renderFilters();
    const visible = getVisibleDestinations(destinations);
    renderDashboardCards(visible);
    renderGrid(visible);
    updateDestCount(visible.length);

    // Bind search input (top-right search on this page)
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', () => {
            const q = searchInput.value.trim().toLowerCase();
            if (!q) {
                const visible = getVisibleDestinations(destinations);
                renderDashboardCards(visible);
                renderGrid(visible);
                updateDestCount(visible.length);
                return;
            }
            const filtered = destinations.filter(d => (
                d.name.toLowerCase().includes(q) ||
                d.city.toLowerCase().includes(q) ||
                d.country.toLowerCase().includes(q) ||
                (d.attractions || '').toLowerCase().includes(q)
            ));
            const visibleFiltered = getVisibleDestinations(filtered);
            renderDashboardCards(visibleFiltered);
            renderGrid(visibleFiltered);
            updateDestCount(visibleFiltered.length);
        });
    }

    // Hero search binding (large search in hero)
    const heroInput = document.getElementById('searchInputHero');
    const heroBtn = document.getElementById('searchHeroBtn');
    if (heroInput) {
        heroInput.addEventListener('keyup', (e) => {
            if (e.key === 'Enter') heroBtn && heroBtn.click();
        });
    }
    if (heroBtn) {
        heroBtn.addEventListener('click', () => {
            const q = (heroInput && heroInput.value || '').trim().toLowerCase();
            if (!q) {
                const visible = getVisibleDestinations(destinations);
                renderDashboardCards(visible);
                renderGrid(visible);
                updateDestCount(visible.length);
                return;
            }
            const filtered = destinations.filter(d => (
                d.name.toLowerCase().includes(q) ||
                d.city.toLowerCase().includes(q) ||
                d.country.toLowerCase().includes(q) ||
                (d.attractions || '').toLowerCase().includes(q)
            ));
            const visibleFiltered = getVisibleDestinations(filtered);
            renderDashboardCards(visibleFiltered);
            renderGrid(visibleFiltered);
            updateDestCount(visibleFiltered.length);
        });
    }

    // Bind notify form (polished modal) submission
    const notifyForm = document.getElementById('notifyForm');
    if (notifyForm) {
        notifyForm.addEventListener('submit', (e) => {
            e.preventDefault();
            const emailInput = document.getElementById('notifyEmail');
            const destIdInput = document.getElementById('notifyDestId');
            const email = emailInput && emailInput.value.trim();
            const destId = destIdInput && parseInt(destIdInput.value);
            if (!email || !/^\S+@\S+\.\S+$/.test(email)) {
                showNotification('Please enter a valid email address', 'danger');
                return;
            }
            const dest = destinations.find(d => d.id === destId) || { id: destId };
            const list = getFromLocalStorage('notifyList') || [];
            list.push({ id: dest.id, email: email, name: dest.name || '', date: new Date().toISOString() });
            saveToLocalStorage('notifyList', list);

            // close notify modal and availability modal if open
            const notifyModalEl = document.getElementById('notifyModal');
            const availModalEl = document.getElementById('availabilityModal');
            const bsNotify = notifyModalEl ? bootstrap.Modal.getInstance(notifyModalEl) : null;
            const bsAvail = availModalEl ? bootstrap.Modal.getInstance(availModalEl) : null;
            if (bsNotify) bsNotify.hide();
            if (bsAvail) bsAvail.hide();

            // clear form
            if (emailInput) emailInput.value = '';
            if (destIdInput) destIdInput.value = '';

            showNotification('Thanks — we will notify you when this destination becomes available', 'success');
        });
    }
}

async function loadServerDestinations() {
    try {
        const response = await fetch('/api/destinations/catalog', { cache: 'no-store' });
        if (!response.ok) return;

        const serverDestinations = await response.json();
        const existingKeys = new Set(destinations.map(destination => `${destination.name}|${destination.country}`.toLowerCase()));
        serverDestinations.forEach(destination => {
            const key = `${destination.name}|${destination.country}`.toLowerCase();
            if (!existingKeys.has(key)) {
                const hasIdCollision = destinations.some(existing => existing.id === destination.id);
                destinations.push({
                    ...destination,
                    image: destination.imageUrl || '/images/travel-placeholder.svg',
                    city: destination.region || destination.country || '',
                    rating: destination.averageRating || 0,
                    reviews: destination.reviewCount || 0,
                    uiId: hasIdCollision ? `server-${destination.id}` : destination.id
                });
            }
        });
    } catch (error) {
        console.warn('Could not load database destinations:', error);
    }
}

function renderFilters() {
    const container = document.getElementById('dashboardFilters');
    if (!container) return;

    const categories = Array.from(new Set(getVisibleDestinations(destinations).map(d => d.category)));
    categories.unshift('All');

    container.innerHTML = categories.map(cat => `
        <button class="btn btn-sm filter-chip" data-cat="${cat}">${cat}</button>
    `).join('');

    container.querySelectorAll('.filter-chip').forEach(btn => {
        btn.addEventListener('click', () => {
            container.querySelectorAll('.filter-chip').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            const cat = btn.getAttribute('data-cat');
            if (cat === 'All') {
                const visible = getVisibleDestinations(destinations);
                renderDashboardCards(visible);
                renderGrid(visible);
                updateDestCount(visible.length);
            } else {
                const filtered = getVisibleDestinations(destinations).filter(d => d.category === cat);
                renderDashboardCards(filtered);
                renderGrid(filtered);
                updateDestCount(filtered.length);
            }
        });
    });

    // activate All by default
    const first = container.querySelector('.filter-chip');
    if (first) first.classList.add('active');
}

// Helper: filter out deleted destinations
function getVisibleDestinations(list) {
    if (!Array.isArray(list)) return [];
    return list.filter(d => !d._deleted);
}

function renderDashboardCards(list) {
    const container = document.getElementById('dashboardList');
    if (!container) return;

    container.innerHTML = list.map(dest => `
        <div class="col-12 col-md-6 col-lg-4 mb-3">
            <div class="card h-100 border-0 shadow-sm d-flex flex-row align-items-center">
                <div style="flex:0 0 110px; max-width:110px;">
                    <img src="${dest.image || '/images/travel-placeholder.svg'}" alt="${dest.name}" style="height:90px; width:110px; object-fit:cover; border-radius:8px;" onerror="this.onerror=null; this.src='/images/travel-placeholder.svg';" />
                </div>
                <div class="p-3" style="flex:1;">
                    <div class="d-flex justify-content-between align-items-start">
                        <div>
                            <h6 class="mb-1 fw-bold">${dest.name}</h6>
                            <small class="text-muted">${dest.city}</small>
                        </div>
                        <div class="text-end">
                            <span class="status-pill ${dest.supported === false ? 'unsupported' : 'supported'}">${dest.supported === false ? 'Review' : 'Ready'}</span>
                        </div>
                    </div>
                    <div class="mt-2 d-flex flex-wrap gap-2">
                        <button class="select-btn" onclick="selectDestination('${dest.uiId ?? dest.id}')" data-id="${dest.uiId ?? dest.id}">Select</button>
                        <button class="btn btn-outline-primary btn-sm" onclick="showDestinationDetails('${dest.uiId ?? dest.id}');">View</button>
                        <button class="btn btn-sm plan-btn" onclick="startTripPlanning('${dest.uiId ?? dest.id}');">Plan</button>
                    </div>
                </div>
            </div>
        </div>
    `).join('');

    // mark previously selected
    const sel = getFromLocalStorage('selectedDestination');
    if (sel) markSelectedButton(sel);
}

function renderGrid(list) {
    const container = document.getElementById('destinationsList');
    if (!container) return;

    container.innerHTML = list.map(dest => `
        <div class="col-md-6 col-lg-4">
            <div class="card border-0 shadow-sm h-100 hover-card" onclick="showDestinationDetails('${dest.uiId ?? dest.id}')">
                <div class="position-relative">
                    <img src="${dest.image || '/images/travel-placeholder.svg'}" class="card-img-top" style="height: 250px; object-fit: cover;" alt="${dest.name}" onerror="this.onerror=null; this.src='/images/travel-placeholder.svg';" />
                    <div class="position-absolute top-0 start-0 bg-dark bg-opacity-50 text-white p-2 m-3 rounded">
                        <span class="badge bg-warning text-dark">${dest.rating} ⭐</span>
                    </div>
                </div>
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <h5 class="card-title text-dark fw-bold mb-0">${dest.name}</h5>
                        <span class="status-pill ${dest.supported === false ? 'unsupported' : 'supported'}">${dest.supported === false ? 'Review' : 'Ready'}</span>
                    </div>
                    <p class="card-text text-muted"><i class="fas fa-map-marker-alt"></i> ${dest.city}, ${dest.country}</p>
                    <p class="card-text text-truncate">${dest.description}</p>
                </div>
            </div>
        </div>
    `).join('');
}

function updateDestCount(count) {
    const el = document.getElementById('destCount');
    if (el) el.textContent = `${count} destinations`;
}

function selectDestination(id) {
    const dest = getDestinationByClientId(id);
    if (!dest) return;

    if (dest.supported === false) {
        showUnsupportedModal(dest);
        return;
    }

    startTripPlanning(dest);
}

function startTripPlanning(destOrId) {
    const dest = typeof destOrId === 'object' ? destOrId : getDestinationByClientId(destOrId);
    if (!dest) return;

    saveToLocalStorage('selectedDestination', dest.id);
    saveToLocalStorage('selectedDestinationName', dest.name);
    markSelectedButton(dest.id);
    showNotification(`Trip planning ready for ${dest.name}. Redirecting...`, 'success');

    setTimeout(() => {
        window.location.href = `create-trip.html?destinationId=${encodeURIComponent(dest.id)}&destinationName=${encodeURIComponent(dest.name)}`;
    }, 800);
}

function markSelectedButton(id) {
    document.querySelectorAll('.select-btn').forEach(btn => {
        const bid = parseInt(btn.getAttribute('data-id'));
        if (bid === id) btn.classList.add('selected'); else btn.classList.remove('selected');
    });
}

// reuse helper functions from app.js
function saveToLocalStorage(key, data) {
    localStorage.setItem(key, JSON.stringify(data));
}

function getFromLocalStorage(key) {
    const data = localStorage.getItem(key);
    try { return data ? JSON.parse(data) : null; } catch (e) { return null; }
}

// -------------------------
// Availability helpers
// -------------------------
function showUnsupportedModal(dest) {
    const modalEl = document.getElementById('availabilityModal');
    if (!modalEl) {
        showNotification('This destination is not supported yet.', 'danger');
        return;
    }

    const availabilityContent = document.getElementById('availabilityContent');
    if (availabilityContent) {
        availabilityContent.innerHTML = `
            <div class="alert alert-warning rounded-4">
                <h6 class="fw-bold mb-2"><i class="fas fa-exclamation-triangle me-2"></i>${dest.name} is not available for planning yet</h6>
                <p class="mb-0">We can still guide you to a nearby supported destination or notify you when this area becomes available.</p>
            </div>
            <div id="suggestionsList" class="mb-3"></div>
            <div class="d-flex gap-2 flex-wrap">
                <button id="viewSupportedBtn" class="btn btn-outline-primary">View Supported Areas</button>
                <button id="openNotifyModalBtn" class="btn btn-primary">Notify Me When Available</button>
            </div>
        `;
    }

    const suggestionsList = document.getElementById('suggestionsList');
    suggestionsList.innerHTML = '';

    const nearby = suggestNearby(dest);
    if (nearby.length > 0) {
        const html = `
            <p class="mb-2">We recommend these nearby supported destinations:</p>
            <div class="list-group">
                ${nearby.map(d => `<button class="list-group-item list-group-item-action" onclick="selectDestination(${d.id});return false;">${d.name} — ${d.city}</button>`).join('')}
            </div>
        `;
        suggestionsList.innerHTML = html;
    } else {
        suggestionsList.innerHTML = '<p class="text-muted">No nearby supported destinations found.</p>';
    }

    const viewBtn = document.getElementById('viewSupportedBtn');
    const openNotifyBtn = document.getElementById('openNotifyModalBtn');

    if (viewBtn) {
        viewBtn.onclick = () => { viewSupportedAreas(); };
    }

    if (openNotifyBtn) {
        openNotifyBtn.onclick = () => { openNotifyModal(dest); };
    }

    const bsModal = new bootstrap.Modal(modalEl);
    bsModal.show();
}

function suggestNearby(dest) {
    // Find supported destinations in the same city or same category, prefer same city
    const sameCity = destinations.filter(d => d.supported !== false && d.city.toLowerCase() === dest.city.toLowerCase() && d.id !== dest.id);
    if (sameCity.length > 0) return sameCity.slice(0,3);

    const sameCategory = destinations.filter(d => d.supported !== false && d.category === dest.category && d.id !== dest.id);
    if (sameCategory.length > 0) return sameCategory.slice(0,3);

    // fallback top-rated supported
    const top = destinations.filter(d => d.supported !== false && d.id !== dest.id).sort((a,b)=> b.rating - a.rating);
    return top.slice(0,3);
}

function viewSupportedAreas() {
    // Simple behavior: filter dashboard to supported destinations
    const supported = destinations.filter(d => d.supported !== false);
    renderDashboardCards(supported);
    renderGrid(supported);
    updateDestCount(supported.length);
    // close modal
    const modalEl = document.getElementById('availabilityModal');
    if (modalEl) {
        const bs = bootstrap.Modal.getInstance(modalEl);
        if (bs) bs.hide();
    }
    showNotification('Showing supported areas', 'success');
}

function openNotifyModal(dest) {
    const notifyModalEl = document.getElementById('notifyModal');
    const destInput = document.getElementById('notifyDestId');
    if (destInput) destInput.value = dest.id;
    const bs = new bootstrap.Modal(notifyModalEl);
    bs.show();
}

