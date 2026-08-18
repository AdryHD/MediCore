(function () {
    var el = document.getElementById('welcome-date');
    if (!el) return;
    var d = new Date();
    var dias = ['domingo', 'lunes', 'martes', 'miércoles', 'jueves', 'viernes', 'sábado'];
    var meses = ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'];
    el.textContent = dias[d.getDay()] + ', ' + d.getDate() + ' de ' + meses[d.getMonth()] + ' de ' + d.getFullYear();
})();

(function () {
    var calendarInstance = null;

    function parseFecha(str) {
        var p = str.split(' ');
        var d = p[0].split('/');
        return d[2] + '-' + d[1] + '-' + d[0] + 'T' + (p[1] || '00:00');
    }

    function iniciarCalendario(citas) {
        var el = document.getElementById('calendario-citas');
        if (!el || typeof FullCalendar === 'undefined') return;

        var eventos = citas.map(function (c) {
            return {
                id: c.id_cita,
                title: c.paciente,
                start: parseFecha(c.fecha_cita),
                extendedProps: { especialidad: c.especialidad, estado: c.estado, id_cita: c.id_cita }
            };
        });

        if (calendarInstance) {
            calendarInstance.removeAllEvents();
            calendarInstance.addEventSource(eventos);
            return;
        }

        calendarInstance = new FullCalendar.Calendar(el, {
            initialView: 'dayGridMonth',
            locale: 'es',
            height: 'auto',
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth,timeGridWeek,listMonth'
            },
            buttonText: { today: 'Hoy', month: 'Mes', week: 'Semana', list: 'Lista' },
            events: eventos,
            eventColor: '#01C0BA',
            eventTextColor: '#ffffff',
            eventClick: function (info) {
                window.location.href = '/Citas/Details/' + info.event.extendedProps.id_cita;
            },
            eventDidMount: function (info) {
                info.el.title = info.event.title + '\n' + info.event.extendedProps.especialidad;
            }
        });
        calendarInstance.render();
    }

    function estadoBadge(estado) {
        var clases = { 'PENDIENTE': 'pending', 'CONFIRMADA': 'confirmed', 'CANCELADA': 'canceled', 'ATENDIDA': 'done', 'COMPLETADA': 'done', 'PROGRAMADA': 'programmed' };
        var icons  = { 'PENDIENTE': 'bi-hourglass-split', 'CONFIRMADA': 'bi-check2-circle', 'CANCELADA': 'bi-slash-circle', 'ATENDIDA': 'bi-check-circle-fill', 'COMPLETADA': 'bi-check-circle-fill', 'PROGRAMADA': 'bi-calendar-check' };
        var key = (estado || '').toUpperCase();
        return '<span class="badge-status ' + (clases[key] || 'pending') + '"><i class="bi ' + (icons[key] || 'bi-hourglass-split') + '"></i>' + (estado || '—') + '</span>';
    }

    function cargarIndicadores() {
        $.getJSON('/Home/GetIndicadores', function (data) {
            if (data.error) return;

            $('#stat-citas-hoy').text(data.citasHoy);
            $('#stat-citas-pendientes').text(data.citasPendientes);

            if (!data.esDoctor) {
                $('#stat-pacientes').text(data.totalPacientes);
                $('#stat-doctores').text(data.totalDoctores);
            }

            iniciarCalendario(data.proximasCitas || []);
        });
    }

    cargarIndicadores();
    setInterval(cargarIndicadores, 30000);
})();
