// Travel Planner JavaScript Functions

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
    }).format(amount);
}

// Format date
function formatDate(date) {
    return new Date(date).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });
}

// Show toast notification
function showToast(message, type = 'info') {
    const toastHTML = `
        <div class="toast align-items-center text-white bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;

    const toastContainer = document.createElement('div');
    toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
    toastContainer.innerHTML = toastHTML;
    document.body.appendChild(toastContainer);

    const toast = new bootstrap.Toast(toastContainer.querySelector('.toast'));
    toast.show();

    setTimeout(() => toastContainer.remove(), 3000);
}

// Validate form
function validateForm(formId) {
    const form = document.getElementById(formId);
    if (!form.checkValidity()) {
        event.preventDefault();
        event.stopPropagation();
    }
    form.classList.add('was-validated');
}

// Confirm delete action
function confirmDelete(message = 'Are you sure you want to delete this item?') {
    return confirm(message);
}

// Initialize tooltips
document.addEventListener('DOMContentLoaded', function () {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});

// Initialize popovers
document.addEventListener('DOMContentLoaded', function () {
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
});

// Chart.js configuration
Chart.defaults.color = '#999';
Chart.defaults.borderColor = '#e1e8ed';
Chart.defaults.font.family = "'Poppins', sans-serif";
Chart.defaults.font.weight = '500';

// Budget expense chart
function createBudgetChart(elementId, budgetData) {
    const ctx = document.getElementById(elementId).getContext('2d');
    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: Object.keys(budgetData),
            datasets: [{
                data: Object.values(budgetData),
                backgroundColor: [
                    '#667eea',
                    '#764ba2',
                    '#26de81',
                    '#fc5c65',
                    '#fed330',
                    '#45b7d1'
                ],
                borderRadius: 10,
                borderColor: 'white',
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        padding: 20,
                        font: {
                            size: 12,
                            weight: 'bold'
                        }
                    }
                }
            }
        }
    });
}

// Trip statistics chart
function createTripStatsChart(elementId, trips) {
    const ctx = document.getElementById(elementId).getContext('2d');
    const monthlyTrips = {};
    
    trips.forEach(trip => {
        const month = new Date(trip.startDate).toLocaleString('en-US', { month: 'short' });
        monthlyTrips[month] = (monthlyTrips[month] || 0) + 1;
    });

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: Object.keys(monthlyTrips),
            datasets: [{
                label: 'Trips',
                data: Object.values(monthlyTrips),
                borderColor: '#667eea',
                backgroundColor: 'rgba(102, 126, 234, 0.1)',
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                pointRadius: 5,
                pointBackgroundColor: '#667eea',
                pointBorderColor: 'white',
                pointBorderWidth: 2
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    display: true,
                    labels: {
                        font: {
                            size: 12,
                            weight: 'bold'
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1
                    }
                }
            }
        }
    });
}

// Expense breakdown chart
function createExpenseChart(elementId, expenses) {
    const categoryTotals = {};
    
    expenses.forEach(expense => {
        categoryTotals[expense.category] = (categoryTotals[expense.category] || 0) + expense.amount;
    });

    const ctx = document.getElementById(elementId).getContext('2d');
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: Object.keys(categoryTotals),
            datasets: [{
                label: 'Expenses ($)',
                data: Object.values(categoryTotals),
                backgroundColor: [
                    '#667eea',
                    '#764ba2',
                    '#26de81',
                    '#fc5c65',
                    '#fed330'
                ],
                borderRadius: 10,
                borderSkipped: false
            }]
        },
        options: {
            responsive: true,
            indexAxis: 'y',
            plugins: {
                legend: {
                    display: false
                }
            }
        }
    });
}

// Smooth scroll to element
function scrollToElement(elementId, offset = 0) {
    const element = document.getElementById(elementId);
    if (element) {
        const elementPosition = element.getBoundingClientRect().top + window.pageYOffset;
        window.scrollTo({
            top: elementPosition - offset,
            behavior: 'smooth'
        });
    }
}

// Add active class to current navigation link
document.addEventListener('DOMContentLoaded', function () {
    const currentPath = window.location.pathname;
    document.querySelectorAll('.nav-link').forEach(link => {
        if (link.getAttribute('href') === currentPath || link.getAttribute('href') === currentPath + '/') {
            link.classList.add('active');
        }
    });
});
