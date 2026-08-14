var doctorSeleccionado = '';
var horaSeleccionada = '';

$(document).ready(function () {

    var initData = $('#citas-init-data');
    doctorSeleccionado = initData.data('doctor') || '';
    horaSeleccionada   = initData.data('hora')   || '';

    $("#IdEspecialidad").change(cargarDoctores);

    $("#IdDoctor").change(cargarFechas);

    $("#Fecha").change(cargarHorarios);

});

function cargarDoctores() {

    let idEspecialidad = $("#IdEspecialidad").val();

    $("#IdDoctor").empty();
    $("#Fecha").empty().append($('<option>', { value: "", text: "-- Seleccione un doctor primero --" }));
    $("#Hora").empty().append($('<option>', { value: "", text: "-- Seleccione una hora --" }));
    $("#Duracion").val("0");

    if (idEspecialidad === "") {

        $("#IdDoctor").append(
            $('<option>', {
                value: "",
                text: "-- Seleccione un doctor --"
            })
        );

        return;
    }

    $.get("/Citas/ObtenerDoctores",
        {
            idEspecialidad: idEspecialidad
        },
        function (data) {

            $("#IdDoctor").append(
                $('<option>', {
                    value: "",
                    text: "-- Seleccione un doctor --"
                })
            );

            $.each(data, function (i, doctor) {

                var option = $('<option>', {
                    value: doctor.id,
                    text: doctor.nombre
                });

                if (doctorSeleccionado != "" &&
                    doctor.id == doctorSeleccionado) {

                    option.prop("selected", true);

                }

                $("#IdDoctor").append(option);

            });

            if (doctorSeleccionado != "")
                cargarFechas();

        });

}

function cargarFechas() {

    let idDoctor = $("#IdDoctor").val();

    $("#Fecha").empty();
    $("#Hora").empty().append($('<option>', { value: "", text: "-- Seleccione una hora --" }));
    $("#Duracion").val("0");

    if (idDoctor === "") {
        $("#Fecha").append($('<option>', { value: "", text: "-- Seleccione un doctor primero --" }));
        return;
    }

    $("#Fecha").append($('<option>', { value: "", text: "Cargando fechas..." }));

    $.get("/Citas/ObtenerFechasDisponibles",
        { idDoctor: idDoctor },
        function (data) {

            $("#Fecha").empty();

            if (data.length === 0) {
                $("#Fecha").append($('<option>', { value: "", text: "-- Sin fechas disponibles --" }));
                return;
            }

            $("#Fecha").append($('<option>', { value: "", text: "-- Seleccione una fecha --" }));

            $.each(data, function (i, fecha) {
                var option = $('<option>', {
                    value: fecha.valor,
                    text: fecha.texto
                });
                $("#Fecha").append(option);
            });

        });

}

function cargarHorarios() {

    let idDoctor = $("#IdDoctor").val();
    let fecha = $("#Fecha").val();

    $("#Hora").empty();

    if (idDoctor === "" || fecha === "")
        return;

    $.get("/Citas/ObtenerHorariosDisponibles",
        {
            idDoctor: idDoctor,
            fecha: fecha
        },
        function (data) {

            $("#Hora").append(
                $('<option>', {
                    value: "",
                    text: "-- Seleccione una hora --"
                })
            );

            $.each(data, function (i, hora) {

                var option = $('<option>', {
                    value: hora.valor,
                    text: hora.texto
                });

                if (horaSeleccionada != "" &&
                    hora.valor == horaSeleccionada) {

                    option.prop("selected", true);

                }

                $("#Hora").append(option);

            });

        });

}