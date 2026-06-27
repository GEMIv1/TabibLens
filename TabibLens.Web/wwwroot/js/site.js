/*Sidebar Toggle (Mobile)*/
function toggleSidebar() {
    const sidebar = document.querySelector('.sidebar');
    const overlay = document.querySelector('.sidebar-overlay');
    sidebar?.classList.toggle('open');
    overlay?.classList.toggle('open');
}

function closeSidebar() {
    const sidebar = document.querySelector('.sidebar');
    const overlay = document.querySelector('.sidebar-overlay');
    sidebar?.classList.remove('open');
    overlay?.classList.remove('open');
}

/*Toast Notifications*/
function showToast(message, type = 'info', duration = 4000) {
    let container = document.querySelector('.toast-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'toast-container';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;

    const icons = { success: '✓', error: '✕', info: 'ℹ' };
    toast.innerHTML = `<span>${icons[type] || 'ℹ'}</span><span>${message}</span>`;

    container.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(20px)';
        toast.style.transition = 'all 0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, duration);
}

/*File Upload Preview*/
function initUploadZone(zoneId, inputId, previewId) {
    const zone = document.getElementById(zoneId);
    const input = document.getElementById(inputId);
    const preview = document.getElementById(previewId);

    if (!zone || !input) return;

    zone.addEventListener('click', () => input.click());

    zone.addEventListener('dragover', (e) => {
        e.preventDefault();
        zone.classList.add('dragover');
    });

    zone.addEventListener('dragleave', () => {
        zone.classList.remove('dragover');
    });

    zone.addEventListener('drop', (e) => {
        e.preventDefault();
        zone.classList.remove('dragover');
        if (e.dataTransfer.files.length > 0) {
            input.files = e.dataTransfer.files;
            showPreview(input.files[0], preview, zone);
        }
    });

    input.addEventListener('change', () => {
        if (input.files.length > 0) {
            showPreview(input.files[0], preview, zone);
        }
    });
}

function showPreview(file, previewEl, zoneEl) {
    if (!file || !previewEl) return;

    const reader = new FileReader();
    reader.onload = (e) => {
        previewEl.innerHTML = `<img src="${e.target.result}" alt="Preview" />`;
        previewEl.classList.remove('hidden');
        if (zoneEl) {
            zoneEl.querySelector('.upload-placeholder')?.classList.add('hidden');
        }
    };
    reader.readAsDataURL(file);
}

/*Modal Helpers*/
function openModal(modalId) {
    document.getElementById(modalId)?.classList.add('open');
}

function closeModal(modalId) {
    document.getElementById(modalId)?.classList.remove('open');
}

/*Close modal on backdrop click*/
document.addEventListener('click', (e) => {
    if (e.target.classList.contains('modal-backdrop')) {
        e.target.classList.remove('open');
    }
});

/*Chat Helpers*/
function scrollToBottom(elementId) {
    const el = document.getElementById(elementId);
    if (el) {
        el.scrollTop = el.scrollHeight;
    }
}

function autoResizeTextarea(textarea) {
    textarea.style.height = 'auto';
    textarea.style.height = Math.min(textarea.scrollHeight, 120) + 'px';
}

/*Password Strength*/
function checkPasswordStrength(password) {
    let score = 0;
    if (password.length >= 6) score++;
    if (password.length >= 10) score++;
    if (/[A-Z]/.test(password)) score++;
    if (/[a-z]/.test(password)) score++;
    if (/[0-9]/.test(password)) score++;
    if (/[^A-Za-z0-9]/.test(password)) score++;

    const bar = document.querySelector('.password-strength-bar');
    if (!bar) return;

    const percentage = (score / 6) * 100;
    bar.style.width = percentage + '%';

    if (score <= 2) {
        bar.style.background = 'var(--color-danger)';
    } else if (score <= 4) {
        bar.style.background = 'var(--color-warning)';
    } else {
        bar.style.background = 'var(--color-success)';
    }
}

/*Confirm Dialog*/
function confirmAction(message) {
    return confirm(message);
}

/*Format Date*/
function formatDate(dateStr) {
    if (!dateStr) return '—';
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

/*Initialize on DOM Ready*/
document.addEventListener('DOMContentLoaded', () => {
    /*Close sidebar overlay*/
    document.querySelector('.sidebar-overlay')?.addEventListener('click', closeSidebar);

    /*Auto-resize textareas*/
    document.querySelectorAll('textarea.auto-resize').forEach(ta => {
        ta.addEventListener('input', () => autoResizeTextarea(ta));
    });
});
