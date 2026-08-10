(() => {
    const detailModalEl = document.getElementById('detailTradeModal');
    const detailModal = detailModalEl ? bootstrap.Modal.getOrCreateInstance(detailModalEl) : null;
    const detailBody = document.getElementById('tradeDetailBody');
    const formatMoney = value => Number(value).toLocaleString('tr-TR', { style: 'currency', currency: 'USD' });
    const formatDate = value => new Intl.DateTimeFormat('tr-TR', { dateStyle: 'medium', timeStyle: 'medium' }).format(new Date(value));
    const escapeHtml = value => String(value).replace(/[&<>'"]/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' })[char]);

    async function showTrade(id) {
        if (!id || id < 1) return;
        detailBody.innerHTML = '<div class="modal-loading">Veri getiriliyor…</div>'; detailModal.show();
        try {
            const response = await fetch(`/Trades/${id}`, { headers: { Accept: 'application/json' } });
            const trade = await response.json();
            if (!response.ok) throw new Error(trade.message || 'İşlem bulunamadı.');
            const pair = escapeHtml(trade.cryptoPair), type = escapeHtml(trade.tradeType);
            detailBody.innerHTML = `<div class="trade-detail-hero"><span class="pair-cell"><i>${pair[0]}</i><strong>${pair}</strong></span><span class="trade-badge ${type.toLowerCase()}">${type}</span><strong>${formatMoney(trade.totalUSD)}</strong></div><dl class="detail-grid"><div><dt>İşlem ID</dt><dd>#${trade.id}</dd></div><div><dt>Kullanıcı</dt><dd>${escapeHtml(trade.userCode)}</dd></div><div><dt>Fiyat</dt><dd>${formatMoney(trade.price)}</dd></div><div><dt>Miktar</dt><dd>${trade.quantity.toLocaleString('tr-TR')}</dd></div><div><dt>Komisyon</dt><dd>${formatMoney(trade.feeUSD)}</dd></div><div><dt>Konum</dt><dd>${escapeHtml(trade.locationCountry)}</dd></div><div><dt>İşlem Hızı</dt><dd>${trade.executionTimeMs} ms</dd></div><div><dt>Tarih</dt><dd>${formatDate(trade.transactionDate)}</dd></div></dl>`;
        } catch (error) { detailBody.innerHTML = `<div class="empty-state"><strong>Kayıt bulunamadı</strong><span>${error.message}</span></div>`; }
    }

    document.getElementById('searchTradeButton')?.addEventListener('click', () => showTrade(Number(document.getElementById('tradeIdSearch').value)));
    document.getElementById('tradeIdSearch')?.addEventListener('keydown', e => { if (e.key === 'Enter') showTrade(Number(e.target.value)); });
    document.querySelectorAll('.view-trade').forEach(button => button.addEventListener('click', () => showTrade(Number(button.dataset.id))));
    document.getElementById('pageSizeSelect')?.addEventListener('change', e => location.href = `/Trades?page=1&pageSize=${e.target.value}`);

    document.getElementById('editTradeModal')?.addEventListener('show.bs.modal', event => {
        const d = event.relatedTarget.dataset;
        document.getElementById('editId').value = d.id; document.getElementById('editIdLabel').textContent = `#${d.id}`;
        document.getElementById('editUser').value = d.user; document.getElementById('editPair').value = d.pair;
        document.getElementById('editType').value = d.type; document.getElementById('editPrice').value = d.price;
        document.getElementById('editQuantity').value = d.quantity; document.getElementById('editFee').value = d.fee;
        document.getElementById('editCountry').value = d.country; document.getElementById('editSpeed').value = d.speed;
        document.getElementById('editDate').value = d.date;
    });

    let pendingForm = null; const confirmBox = document.getElementById('deleteConfirm');
    document.querySelectorAll('.delete-form').forEach(form => form.addEventListener('submit', e => { e.preventDefault(); pendingForm = form; confirmBox.classList.add('show'); confirmBox.setAttribute('aria-hidden', 'false'); document.getElementById('deleteMessage').textContent = `#${form.querySelector('[data-id]').dataset.id} numaralı işlem kalıcı olarak silinecek.`; }));
    document.getElementById('cancelDelete')?.addEventListener('click', () => { pendingForm = null; confirmBox.classList.remove('show'); });
    document.getElementById('confirmDelete')?.addEventListener('click', () => pendingForm?.submit());
})();
