// ── Navbar scroll effect ────────────────────────────────────────
window.addEventListener('scroll', () => {
    const nav = document.getElementById('mainNavbar');
    if (nav) {
        nav.style.boxShadow = window.scrollY > 30
            ? '0 4px 20px rgba(0,0,0,0.4)'
            : '0 2px 15px rgba(0,0,0,0.3)';
    }
});

// ── Auto-dismiss alerts after 4s ───────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) bsAlert.close();
        }, 4000);
    });
});
