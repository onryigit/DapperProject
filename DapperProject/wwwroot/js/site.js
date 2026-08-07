(() => {
    const sidebar = document.getElementById('sidebar');
    const backdrop = document.getElementById('sidebarBackdrop');
    const toggle = document.getElementById('menuToggle');
    const closeSidebar = () => { sidebar?.classList.remove('open'); backdrop?.classList.remove('show'); };
    toggle?.addEventListener('click', () => { sidebar?.classList.toggle('open'); backdrop?.classList.toggle('show'); });
    backdrop?.addEventListener('click', closeSidebar);

    const clock = document.getElementById('liveClock');
    const tick = () => { if (clock) clock.textContent = new Intl.DateTimeFormat('tr-TR', { hour: '2-digit', minute: '2-digit', second: '2-digit' }).format(new Date()); };
    tick(); setInterval(tick, 1000);

    document.querySelectorAll('.toast-message button').forEach(button => button.addEventListener('click', () => button.parentElement.remove()));
})();
