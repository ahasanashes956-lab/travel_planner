// ==========================================
// Profile Functions
// ==========================================

const API_BASE_URL = window.location.origin === 'null' ? 'http://localhost:8000' : window.location.origin;

document.addEventListener('DOMContentLoaded', function() {
    loadUserProfile();
});

async function loadUserProfile() {
    try {
        const response = await fetch(`${API_BASE_URL}/api/current-user`, {
            credentials: 'include'
        });

        if (!response.ok) {
            window.location.href = 'login.html';
            return;
        }

        const profileData = await response.json();

        document.getElementById('firstName').value = profileData.firstName || '';
        document.getElementById('lastName').value = profileData.lastName || '';
        document.getElementById('email').value = profileData.email || '';
        document.getElementById('phone').value = profileData.phone || '';
        document.getElementById('city').value = profileData.city || '';
        document.getElementById('country').value = profileData.country || '';
        document.getElementById('address').value = profileData.address || '';
        document.getElementById('bio').value = profileData.bio || '';

        document.getElementById('profileName').textContent = `${profileData.firstName || ''} ${profileData.lastName || ''}`.trim();
        document.getElementById('profileEmail').textContent = profileData.email || '';
        setProfilePhoto(profileData.profilePictureUrl);
    } catch (error) {
        console.error('Profile loading error:', error);
        showNotification('Could not load profile.', 'error');
    }
}

function setProfilePhoto(photoUrl) {
    const photo = document.getElementById('profilePhoto');
    const placeholder = document.getElementById('profilePhotoPlaceholder');
    if (!photo || !placeholder) return;

    if (photoUrl) {
        photo.src = photoUrl;
        photo.classList.remove('d-none');
        placeholder.classList.add('d-none');
    } else {
        photo.removeAttribute('src');
        photo.classList.add('d-none');
        placeholder.classList.remove('d-none');
    }
}

document.getElementById('profilePhotoInput')?.addEventListener('change', async function() {
    const file = this.files?.[0];
    if (!file) return;

    const formData = new FormData();
    formData.append('photo', file);

    try {
        const response = await fetch(`${API_BASE_URL}/api/profile/photo`, {
            method: 'POST',
            credentials: 'include',
            body: formData
        });
        const data = await response.json().catch(() => ({}));
        if (!response.ok) {
            showNotification(data.message || 'Photo upload failed.', 'error');
            return;
        }

        setProfilePhoto(`${data.profilePictureUrl}?v=${Date.now()}`);
        addUserNotification(profileData.email, 'Your profile photo was updated successfully.', 'success');
        showNotification('Profile photo updated successfully!');
    } catch (error) {
        console.error('Profile photo upload error:', error);
        showNotification('Photo upload error: ' + error.message, 'error');
    } finally {
        this.value = '';
    }
});

document.getElementById('profileForm')?.addEventListener('submit', async function(e) {
    e.preventDefault();

    const profileData = {
        firstName: document.getElementById('firstName').value,
        lastName: document.getElementById('lastName').value,
        email: document.getElementById('email').value,
        phone: document.getElementById('phone').value,
        city: document.getElementById('city').value,
        country: document.getElementById('country').value,
        address: document.getElementById('address').value,
        bio: document.getElementById('bio').value
    };

    try {
        const response = await fetch(`${API_BASE_URL}/api/profile`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify(profileData)
        });
        const data = await response.json();

        if (!response.ok) {
            showNotification(data.message || 'Profile update failed.', 'error');
            return;
        }

        saveToLocalStorage('currentUser', data.user);
        addUserNotification(data.user.email, 'Your profile information was updated successfully.', 'success');
        showNotification('Profile updated successfully!');
        document.getElementById('profileName').textContent = `${data.user.firstName || ''} ${data.user.lastName || ''}`.trim();
    } catch (error) {
        console.error('Profile update error:', error);
        showNotification('Profile update error: ' + error.message, 'error');
    }
});

document.getElementById('changePasswordForm')?.addEventListener('submit', async function(e) {
    e.preventDefault();

    const message = document.getElementById('changePasswordMessage');
    const submitButton = document.getElementById('changePasswordSubmit');
    const currentPassword = document.getElementById('currentPassword').value;
    const newPassword = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmNewPassword').value;

    message.hidden = false;
    message.className = 'alert alert-danger mb-3';

    if (newPassword !== confirmPassword) {
        message.textContent = 'Passwords do not match.';
        return;
    }

    if (newPassword.length < 10 || !/[A-Z]/.test(newPassword) || !/[a-z]/.test(newPassword) ||
        !/[0-9]/.test(newPassword) || !/[^a-zA-Z0-9]/.test(newPassword)) {
        message.textContent = 'New password must be at least 10 characters and include uppercase, lowercase, a number, and a special character.';
        return;
    }

    submitButton.disabled = true;
    submitButton.textContent = 'Updating...';

    try {
        const response = await fetch(`${API_BASE_URL}/api/change-password`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify({ currentPassword, newPassword, confirmPassword })
        });
        const data = await response.json().catch(() => ({}));

        if (!response.ok) {
            message.textContent = data.message || 'Password change failed.';
            return;
        }

        message.className = 'alert alert-success mb-3';
        message.textContent = data.message;
        document.getElementById('changePasswordForm').reset();
        setTimeout(() => bootstrap.Modal.getInstance(document.getElementById('changePasswordModal'))?.hide(), 900);
    } catch (error) {
        message.textContent = 'Could not change password. Please try again.';
        console.error('Password change error:', error);
    } finally {
        submitButton.disabled = false;
        submitButton.textContent = 'Update Password';
    }
});
