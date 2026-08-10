(() => {
    const data = window.tradePulseDashboard;
    if (!data || typeof ApexCharts === 'undefined') return;
    const text = '#8d98a8', grid = '#28303a', green = '#0ecb81', red = '#f6465d', cyan = '#28c8e5';

    new ApexCharts(document.querySelector('#volumeChart'), {
        chart: { type: 'area', height: 305, toolbar: { show: false }, zoom: { enabled: false }, foreColor: text },
        series: [{ name: 'Hacim', data: data.trend.map(x => [new Date(x.date).getTime(), x.volume]) }],
        colors: [green], stroke: { curve: 'smooth', width: 2.5 },
        fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: .34, opacityTo: .02, stops: [0, 95] } },
        dataLabels: { enabled: false }, grid: { borderColor: grid, strokeDashArray: 4 },
        xaxis: { type: 'datetime', axisBorder: { show: false }, axisTicks: { show: false }, labels: { datetimeUTC: false, format: 'dd MMM' } },
        yaxis: { labels: { formatter: v => '$' + (v / 1e6).toFixed(0) + 'M' } },
        tooltip: { theme: 'dark', x: { format: 'dd MMM yyyy' }, y: { formatter: v => '$' + v.toLocaleString('tr-TR', { maximumFractionDigits: 0 }) } }
    }).render();

    new ApexCharts(document.querySelector('#pairChart'), {
        chart: { type: 'donut', height: 310, foreColor: text },
        series: data.pairs.map(x => x.count), labels: data.pairs.map(x => x.pair),
        colors: [green, '#25a8ff', '#8b5cf6', '#f0b90b', '#fb7185', '#14b8a6', '#f97316', '#64748b'],
        stroke: { width: 3, colors: ['#171c22'] }, legend: { position: 'bottom', fontSize: '11px', markers: { width: 7, height: 7, radius: 7 } },
        dataLabels: { enabled: false }, plotOptions: { pie: { donut: { size: '70%', labels: { show: true, name: { color: text }, value: { color: '#f5f7fa', fontWeight: 700 }, total: { show: true, label: 'TOPLAM', color: text, formatter: () => '1.0M' } } } } },
        tooltip: { theme: 'dark', y: { formatter: v => v.toLocaleString('tr-TR') + ' işlem' } }
    }).render();

    const buy = data.types.find(x => x.type === 'BUY')?.count || 0;
    const sell = data.types.find(x => x.type === 'SELL')?.count || 0;
    new ApexCharts(document.querySelector('#tradeTypeChart'), {
        chart: { type: 'bar', height: 120, toolbar: { show: false }, sparkline: { enabled: true } },
        series: [{ data: [buy, sell] }], colors: [green, red], plotOptions: { bar: { distributed: true, borderRadius: 5, columnWidth: '44%' } },
        dataLabels: { enabled: false }, tooltip: { theme: 'dark', x: { formatter: (_, o) => o.dataPointIndex === 0 ? 'BUY' : 'SELL' }, y: { formatter: v => v.toLocaleString('tr-TR') } }
    }).render();

    if (typeof L !== 'undefined') {
        const map = L.map('tradeMap', { zoomControl: false, attributionControl: false, scrollWheelZoom: false }).setView([28, 18], 1.45);
        L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', { maxZoom: 6 }).addTo(map);
        const max = Math.max(...data.countries.map(x => x.count), 1);
        data.countries.filter(x => x.lat || x.lng).forEach(x => {
            const radius = 6 + Math.sqrt(x.count / max) * 13;
            L.circleMarker([x.lat, x.lng], { radius: radius + 7, stroke: false, fillColor: '#168cff', fillOpacity: .14, interactive: false }).addTo(map);
            L.circleMarker([x.lat, x.lng], { radius, color: '#bce8ff', weight: 1.5, fillColor: '#087fdb', fillOpacity: .58 })
                .bindTooltip(`<strong>${x.country}</strong><br>${x.count.toLocaleString('tr-TR')} işlem<br>$${Math.round(x.volume).toLocaleString('tr-TR')}`, { direction: 'top' }).addTo(map);
        });
    }
})();
