// Admin JS - simple client-side admin for demo

document.addEventListener('DOMContentLoaded', () => {
	initAdmin();
});

function initAdmin() {
	const panel = document.getElementById('adminPanel');
	const logoutBtn = document.getElementById('adminLogout');

	fetch('/api/current-user', { credentials: 'include', cache: 'no-store' })
			.then(response => response.ok ? response.json() : null)
			.then(user => {
				if (!user?.isAuthenticated) {
					window.location.href = 'login.html?returnUrl=%2Fadmin.html';
					return;
				}
				if (!Array.isArray(user.roles) || !user.roles.includes('Admin')) {
					document.body.innerHTML = '<main class="container py-5"><div class="alert alert-warning"><strong>Admin session required.</strong><p class="mb-3">This browser is currently signed in as a regular user. Sign in with the admin account to open the admin dashboard.</p><a class="btn btn-primary" href="login.html?returnUrl=%2Fadmin.html">Go to Admin Login</a></div></main>';
					return;
				}
				showAdminPanel();
				loadAdminData();
			});

	if (logoutBtn) {
		logoutBtn.addEventListener('click', () => {
			fetch('/api/logout', { method: 'POST', credentials: 'include' })
				.finally(() => { window.location.href = 'login.html'; });
		});
	}

	document.getElementById('destinationForm')?.addEventListener('submit', createDestination);
}

function showAdminPanel() {
	document.getElementById('adminPanel').style.display = 'block';
}

function renderDestList() {
	const container = document.getElementById('adminDestList');
	const list = window.destinations || [];
	const overrides = getFromLocalStorage('adminDestinations') || {};

	container.innerHTML = list.map(d => {
		const over = overrides[d.id] || {};
		const supported = (typeof over.supported !== 'undefined') ? over.supported : (d.supported !== false);
		return `
			<div class="d-flex align-items-center justify-content-between p-2 border-bottom">
				<div>
					<strong>${d.name}</strong><div class="small text-muted">${d.city}, ${d.country}</div>
				</div>
				<div class="d-flex align-items-center gap-2">
					<div class="form-check form-switch">
						<input class="form-check-input" type="checkbox" id="adm_dest_${d.id}" ${supported ? 'checked' : ''} onchange="toggleDestSupported(${d.id}, this.checked)">
						<label class="form-check-label small" for="adm_dest_${d.id}">Supported</label>
					</div>
					<button class="btn btn-sm btn-outline-danger" onclick="deleteDestination(${d.id})">Delete</button>
				</div>
			</div>
		`;
	}).join('');
}

function toggleDestSupported(id, isSupported) {
	const overrides = getFromLocalStorage('adminDestinations') || {};
	overrides[id] = overrides[id] || {};
	overrides[id].supported = !!isSupported;
	saveToLocalStorage('adminDestinations', overrides);
	notifyAllUsers(`${dNameForNotification(id)} destination availability was updated by an administrator.`, 'info');
	showNotification('Updated destination availability', 'success');
	// trigger re-render on other pages by reloading
	setTimeout(() => location.reload(), 600);
}

function deleteDestination(id) {
	if (!confirm('Delete this destination from list (local demo only)?')) return;
	// mark deleted in overrides
	const overrides = getFromLocalStorage('adminDestinations') || {};
	overrides[id] = overrides[id] || {};
	overrides[id].deleted = true;
	saveToLocalStorage('adminDestinations', overrides);
	notifyAllUsers(`${dNameForNotification(id)} was removed from supported destinations by an administrator.`, 'warning');
	showNotification('Destination deleted (local demo)', 'success');
	setTimeout(() => location.reload(), 600);
}

function renderUserList() {
	const container = document.getElementById('adminUserList');
	const users = getFromLocalStorage('users') || [];
	if (users.length === 0) {
		container.innerHTML = '<div class="text-muted small">No users yet.</div>';
		return;
	}
	container.innerHTML = users.map((u, idx) => `
		<div class="d-flex align-items-center justify-content-between p-2 border-bottom">
			<div>
				<strong>${u.email}</strong>
				<div class="small text-muted">${u.isAdmin ? 'Admin' : 'User'}</div>
			</div>
			<div class="d-flex gap-2">
				<button class="btn btn-sm btn-outline-danger" onclick="removeUser(${idx})">Remove</button>
			</div>
		</div>
	`).join('');
}


function removeUser(idx) {
	const users = getFromLocalStorage('users') || [];
	if (!users[idx]) return;
	if (!confirm('Remove user?')) return;
	const removedUser = users[idx];
	users.splice(idx,1);
	saveToLocalStorage('users', users);
	renderUserList();
	addUserNotification(removedUser.email, 'Your demo account was removed by an administrator.', 'warning');
	showNotification('User removed', 'success');
}

function dNameForNotification(id) {
	const destinationList = typeof destinations !== 'undefined' ? destinations : [];
	return destinationList.find(destination => destination.id === id)?.name || 'A destination';
}

async function loadAdminData() {
	try {
		const [usersResponse, destinationsResponse] = await Promise.all([
			fetch('/api/admin/users', { credentials: 'include', cache: 'no-store' }),
			fetch('/api/admin/destinations', { credentials: 'include', cache: 'no-store' })
		]);
		if (!usersResponse.ok || !destinationsResponse.ok) throw new Error('Admin data could not be loaded.');

		const users = await usersResponse.json();
		const destinations = await destinationsResponse.json();
		const paymentsResponse = await fetch('/api/admin/payments', { credentials: 'include', cache: 'no-store' });
		if (!paymentsResponse.ok) throw new Error('Payment data could not be loaded.');
		const payments = await paymentsResponse.json();
		renderAdminUsers(users);
		renderAdminDestinations(destinations);
		renderAdminPayments(payments);
		document.getElementById('totalUsers').textContent = users.length;
		document.getElementById('pendingUsers').textContent = users.filter(user => user.status === 'Pending').length;
		document.getElementById('approvedUsers').textContent = users.filter(user => user.status === 'Approved').length;
		document.getElementById('destinationTotal').textContent = destinations.length;
		document.getElementById('destinationRefresh').textContent = `${destinations.length} published`;
	} catch (error) {
		console.error('Admin data error:', error);
		showNotification(error.message, 'danger');
	}
}

function renderAdminPayments(payments) {
	const container = document.getElementById('adminPaymentList');
	const count = document.getElementById('paymentRefresh');
	if (!container) return;
	if (!payments.length) {
		container.innerHTML = '<div class="user-meta">No payments yet.</div>';
		if (count) count.textContent = '0 payments';
		return;
	}

	if (count) count.textContent = `${payments.length} payments`;
	container.innerHTML = payments.map(payment => {
		const statusClass = payment.status === 'Paid' ? 'approved' : payment.status === 'Pending' ? 'pending' : 'rejected';
		return `<div class="user-row payment-row"><div class="payment-copy"><strong><i class="fas fa-receipt"></i>${escapeAdminText(payment.tripTitle || 'Trip payment')}</strong><span class="user-meta">${escapeAdminText(payment.userName || payment.email || 'Unknown user')} · ${escapeAdminText(payment.destination || '')}</span><span class="user-meta payment-reference"><i class="fas fa-hashtag"></i>${escapeAdminText(payment.transactionId)} · ${escapeAdminText(payment.paymentMethod)}</span></div><div class="user-actions payment-actions"><strong class="payment-amount">${escapeAdminText(payment.currency)} ${Number(payment.amount || 0).toLocaleString()}</strong><span class="status-pill ${statusClass}">${escapeAdminText(payment.status)}</span></div></div>`;
	}).join('');
}

function renderAdminUsers(users) {
	const container = document.getElementById('adminUserList');
	if (!container) return;
	if (!users.length) {
		container.innerHTML = '<div class="user-meta">No registered users yet.</div>';
		return;
	}

	container.innerHTML = users.map(user => {
		const statusClass = user.status.toLowerCase();
		const isAdmin = user.roles?.includes('Admin');
		return `
			<div class="user-row">
					<div><strong>${escapeAdminText(user.name || 'Unnamed user')}</strong><span class="user-meta">${escapeAdminText(user.email || '')} · ${user.tripCount} trips</span></div>
					<div class="user-actions"><span class="status-pill ${statusClass}"><i class="fas ${statusClass === 'approved' ? 'fa-check' : statusClass === 'pending' ? 'fa-clock' : 'fa-xmark'}"></i>${user.status}</span>${isAdmin || statusClass !== 'pending' ? '<span class="user-meta">Final status</span>' : `<button class="btn btn-sm btn-outline-success" data-user-status="approved" data-user-id="${user.id}">Approve</button><button class="btn btn-sm btn-outline-danger" data-user-status="rejected" data-user-id="${user.id}">Reject</button>`}</div>
			</div>`;
	}).join('');

	container.querySelectorAll('[data-user-status]').forEach(button => {
		button.addEventListener('click', () => updateUserStatus(button.dataset.userId, button.dataset.userStatus, button));
	});
}

function renderAdminDestinations(items) {
	const container = document.getElementById('adminDestList');
	if (!container) return;
	container.innerHTML = items.map(destination => {
		const imageUrl = destination.imageUrl && !destination.imageUrl.toLowerCase().includes('placeholder')
			? destination.imageUrl
			: '/images/Sylhet-Scenic-Tour.jpg';
		return `
		<div class="destination-row" data-destination-id="${destination.id}"><img class="destination-thumb" src="${escapeAdminAttribute(imageUrl)}" alt="${escapeAdminAttribute(destination.name || 'Destination')}" onerror="this.onerror=null;this.src='/images/Sylhet-Scenic-Tour.jpg';"><div class="destination-copy"><strong>${escapeAdminText(destination.name)}</strong><div>${escapeAdminText(destination.country || '')} · ${escapeAdminText(destination.category || 'Uncategorised')}</div></div><div class="destination-actions"><span class="status-pill ${destination.isPublished ? 'approved' : 'rejected'}">${destination.isPublished ? 'Published' : 'Unpublished'}</span><button class="btn btn-sm btn-outline-success btn-publish-destination" data-publish-destination="${destination.id}" data-published="${destination.isPublished}" title="${destination.isPublished ? 'Unpublish destination' : 'Publish destination'}"><i class="fas ${destination.isPublished ? 'fa-eye-slash' : 'fa-eye'}"></i></button><button class="btn btn-sm btn-delete-destination" data-delete-destination="${destination.id}" title="Delete destination"><i class="fas fa-trash-can"></i></button></div></div>
		`;
	}).join('');

	container.querySelectorAll('[data-delete-destination]').forEach(button => {
		button.addEventListener('click', () => deleteAdminDestination(button.dataset.deleteDestination, button));
	});
	container.querySelectorAll('[data-publish-destination]').forEach(button => {
		button.addEventListener('click', () => toggleDestinationPublished(button.dataset.publishDestination, button.dataset.published !== 'true'));
	});
}

async function toggleDestinationPublished(id, isPublished) {
	try {
		const response = await fetch(`/api/admin/destinations/${id}/publish`, {
			method: 'POST',
			credentials: 'include',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({ isPublished })
		});
		const data = await response.json().catch(() => ({}));
		if (!response.ok) throw new Error(data.message || 'Could not update destination publication.');
		showNotification(data.message, 'success');
		await loadAdminData();
	} catch (error) {
		showNotification(error.message, 'danger');
	}
}

async function deleteAdminDestination(id, button) {
	if (!window.confirm('Delete this destination? It will disappear from the public Destinations page.')) return;
	const row = button.closest('[data-destination-id]');
	if (row) row.remove();
	const total = document.getElementById('destinationTotal');
	const libraryCount = document.getElementById('destinationRefresh');
	if (total) total.textContent = Math.max(0, Number(total.textContent) - 1);
	if (libraryCount) libraryCount.textContent = `${Math.max(0, Number(total?.textContent || 0))} published`;

	try {
		const response = await fetch(`/api/admin/destinations/${id}`, { method: 'DELETE', credentials: 'include' });
		const data = await response.json().catch(() => ({}));
		if (!response.ok) throw new Error(data.message || 'Could not delete destination.');
		showNotification('Destination deleted.', 'success');
	} catch (error) {
		showNotification(error.message, 'danger');
		await loadAdminData();
	}
}

async function updateUserStatus(id, status, button) {
	const row = button?.closest('.user-row');
	const statusPill = row?.querySelector('.status-pill');
	if (row) {
		row.querySelectorAll('[data-user-status]').forEach(action => { action.disabled = true; });
	}
	if (statusPill) {
		statusPill.className = `status-pill ${status}`;
		statusPill.innerHTML = `<i class="fas ${status === 'approved' ? 'fa-check' : 'fa-xmark'}"></i>${status === 'approved' ? 'Approved' : 'Rejected'}`;
	}
	const actions = row?.querySelector('.user-actions');
	if (actions) {
		actions.querySelectorAll('[data-user-status]').forEach(action => action.remove());
		const finalLabel = document.createElement('span');
		finalLabel.className = 'user-meta';
		finalLabel.textContent = 'Final status';
		actions.appendChild(finalLabel);
	}
	const pendingCount = document.getElementById('pendingUsers');
	const approvedCount = document.getElementById('approvedUsers');
	if (status === 'approved') {
		if (pendingCount) pendingCount.textContent = Math.max(0, Number(pendingCount.textContent) - 1);
		if (approvedCount) approvedCount.textContent = Number(approvedCount.textContent) + 1;
	}

	let response;
	try {
		response = await fetch(`/api/admin/users/${id}/status`, {
			method: 'POST', credentials: 'include', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ status })
		});
	} catch (error) {
		showNotification('Could not update user status.', 'danger');
		await loadAdminData();
		return;
	}
	const data = await response.json().catch(() => ({}));
	if (!response.ok) {
		showNotification(data.message || 'Could not update user.', 'danger');
		await loadAdminData();
		return;
	}
	showNotification(status === 'approved' ? 'User approved successfully.' : 'User rejected successfully.', 'success');
	await loadAdminData();
}

async function createDestination(event) {
	event.preventDefault();
	const payload = {
		name: document.getElementById('destinationName').value.trim(),
		country: document.getElementById('destinationCountry').value.trim(),
		region: document.getElementById('destinationRegion').value.trim(),
		category: document.getElementById('destinationCategory').value,
		imageUrl: document.getElementById('destinationImage').value.trim(),
		description: document.getElementById('destinationDescription').value.trim()
	};
	const response = await fetch('/api/admin/destinations', {
		method: 'POST', credentials: 'include', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload)
	});
	const data = await response.json().catch(() => ({}));
	if (!response.ok) { showNotification(data.message || 'Could not publish destination.', 'danger'); return; }
	document.getElementById('destinationForm').reset();
	showNotification('Destination published successfully.', 'success');
	await loadAdminData();
}

function escapeAdminText(value) {
	return String(value ?? '').replace(/[&<>'"]/g, character => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[character]));
}

function escapeAdminAttribute(value) {
	return escapeAdminText(value).replace(/`/g, '&#96;');
}
