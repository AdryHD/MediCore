// Muestra la fecha actual en el banner de bienvenida del dashboard
(function () {
    var el = document.getElementById('welcome-date');
    if (!el) return;

    var d = new Date();
    var dias = ['domingo', 'lunes', 'martes', 'miércoles', 'jueves', 'viernes', 'sábado'];
    var meses = ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'];

    el.textContent = dias[d.getDay()] + ', ' + d.getDate() + ' de ' + meses[d.getMonth()] + ' de ' + d.getFullYear();
})();

// Carga y actualiza los indicadores del dashboard
(function () {
    function estadoBadge(estado) {
        var clases = {
            'Pendiente': 'pending',
            'Confirmada': 'done',
            'Cancelada': 'canceled',
            'Completada': 'done'
        };
        var icons = {
            'Pendiente': 'bi-hourglass-split',
            'Confirmada': 'bi-check-circle',
            'Cancelada': 'bi-slash-circle',
            'Completada': 'bi-check-circle'
        };
        var cls = clases[estado] || 'pending';
        var icon = icons[estado] || 'bi-hourglass-split';
        return '<span class="badge-status ' + cls + '"><i class="bi ' + icon + '"></i>' + (estado || '—') + '</span>';
    }

    function cargarIndicadores() {
        $.getJSON('/Home/GetIndicadores', function (data) {
            if (data.error) return;

            $('#stat-pacientes').text(data.totalPacientes);
            $('#stat-doctores').text(data.totalDoctores);
            $('#stat-citas-hoy').text(data.citasHoy);
            $('#stat-citas-pendientes').text(data.citasPendientes);

            var tbody = $('#tabla-proximas-citas');
            tbody.empty();

            if (!data.proximasCitas || data.proximasCitas.length === 0) {
                tbody.html(
                    '<tr><td colspan="6" class="empty-state-cell">' +
                    '<div class="empty-state">' +
                    '<div class="empty-state-icon-wrap"><i class="bi bi-calendar3"></i></div>' +
                    '<p class="empty-state-title">Sin citas registradas</p>' +
                    '<p class="empty-state-sub">Aún no hay citas programadas en el sistema.</p>' +
                    '</div></td></tr>'
                );
                return;
            }

            $.each(data.proximasCitas, function (i, c) {
                tbody.append(
                    '<tr>' +
                    '<td>' + c.id_cita + '</td>' +
                    '<td>' + (c.paciente || '—') + '</td>' +
                    '<td>' + (c.doctor || '—') + '</td>' +
                    '<td>' + (c.especialidad || '—') + '</td>' +
                    '<td>' + c.fecha_cita + '</td>' +
                    '<td>' + estadoBadge(c.estado) + '</td>' +
                    '</tr>'
                );
            });
        });
    }

    // Carga inmediata al abrir el dashboard
    cargarIndicadores();

    // Refresca automáticamente cada 30 segundos
    setInterval(cargarIndicadores, 30000);
})();
