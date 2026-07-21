$(document).ready(function () {

    $("#IdEspecialidad").change(cargarDoctores);

    $("#IdDoctor").change(cargarHorarios);

    $("#Fecha").change(cargarHorarios);

});

function cargarDoctores() {

    let idEspecialidad = $("#IdEspecialidad").val();

    $("#IdDoctor").empty();
    $("#Hora").empty();

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
                cargarHorarios();

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