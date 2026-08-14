// programarSeguimiento.js
// Carga dinámicamente las fechas y horarios disponibles del doctor
// para la vista ProgramarSeguimiento. El idDoctor se lee del atributo data-doctor-id.

var idDoctor = 0;

$(document).ready(function () {
    idDoctor = parseInt($('#selectFecha').data('doctor-id'), 10);
    cargarFechasSeguimiento();
    $("#selectFecha").change(cargarHorariosSeguimiento);
});

function cargarFechasSeguimiento() {
    var $selectFecha = $("#selectFecha");
    $selectFecha.empty().append($('<option>', { value: "", text: "-- Cargando..." }));
    $("#selectHora").empty().append($('<option>', { value: "", text: "-- Seleccione fecha primero --" })).prop("disabled", true);

    $.get("/Citas/ObtenerFechasDisponibles", { idDoctor: idDoctor }, function (data) {
        $selectFecha.empty().append($('<option>', { value: "", text: "-- Seleccione una fecha --" }));
        if (!data || data.length === 0) {
            $selectFecha.append($('<option>', { value: "", text: "Sin fechas disponibles" }));
            return;
        }
        $.each(data, function (i, item) {
            $selectFecha.append($('<option>', { value: item.valor, text: item.texto }));
        });
    }).fail(function () {
        $selectFecha.empty().append($('<option>', { value: "", text: "Error al cargar fechas" }));
    });
}

function cargarHorariosSeguimiento() {
    var fecha = $("#selectFecha").val();
    var $selectHora = $("#selectHora");

    $selectHora.empty().append($('<option>', { value: "", text: "-- Cargando horarios..." })).prop("disabled", true);

    if (!fecha) {
        $selectHora.empty().append($('<option>', { value: "", text: "-- Seleccione fecha primero --" }));
        return;
    }

    $.get("/Citas/ObtenerHorariosDisponibles", { idDoctor: idDoctor, fecha: fecha }, function (data) {
        $selectHora.empty().append($('<option>', { value: "", text: "-- Seleccione un horario --" }));
        if (!data || data.length === 0) {
            $selectHora.append($('<option>', { value: "", text: "Sin horarios disponibles para esta fecha" }));
            return;
        }
        $.each(data, function (i, item) {
            $selectHora.append($('<option>', { value: item.valor, text: item.texto }));
        });
        $selectHora.prop("disabled", false);
    }).fail(function () {
        $selectHora.empty().append($('<option>', { value: "", text: "Error al cargar horarios" }));
    });
}
