$(document).ready(function () {
    $("#HorarioMedicoForm").submit(function (e) {
        $(".text-danger").remove();
        $(".form-control,.form-select").removeClass("is-invalid");

        var esValido = true;

        var idDoctor = $("#IdDoctor").val();
        var diaSemana = $("#DiaSemana").val();
        var horaInicio = $("#HoraInicio").val();
        var horaFin = $("#HoraFin").val();
        var duracionCita = parseInt($("#DuracionCita").val(), 10);

        if (!idDoctor) {
            mostrarError("#IdDoctor", "Debe seleccionar un doctor.");
            esValido = false;
        }

        if (!diaSemana) {
            mostrarError("#DiaSemana", "Debe seleccionar el día de la semana.");
            esValido = false;
        }

        if (!horaInicio) {
            mostrarError("#HoraInicio", "La hora de inicio es obligatoria.");
            esValido = false;
        }

        if (!horaFin) {
            mostrarError("#HoraFin", "La hora de fin es obligatoria.");
            esValido = false;
        }

        if (horaInicio && horaFin && horaFin <= horaInicio) {
            mostrarError("#HoraFin", "La hora de fin debe ser mayor que la hora de inicio.");
            esValido = false;
        }

        if (!duracionCita || duracionCita < 15 || duracionCita > 480) {
            mostrarError("#DuracionCita", "La duración de la cita debe estar entre 15 y 480 minutos.");
            esValido = false;
        }

        if (!esValido) {
            e.preventDefault();
        }
    });

    function mostrarError(selector, mensaje) {
        $(selector).addClass("is-invalid");
        $(selector).closest(".form-group").append(
            '<small class="text-danger d-block mt-1 fw-bold" style="font-size: 0.85rem;">' + mensaje + '</small>'
        );
    }
});
