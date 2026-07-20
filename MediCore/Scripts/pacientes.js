$(document).ready(function () {
    $("#PacienteForm").submit(function (e) {
        $(".text-danger").remove();
        $(".form-control,.form-select").removeClass("is-invalid");

        var esValido = true;

        var nombreCompleto = $("#NombreCompleto").val().trim();
        var cedula = $("#Cedula").val().trim();
        var fechaNacimiento = $("#FechaNacimiento").val();
        var sexo = $("#Sexo").val();
        var correo = $("#CorreoElectronico").val().trim();
        var telefono = $("#Telefono").val().trim();

        if (nombreCompleto === "") {
            mostrarError("#NombreCompleto", "El nombre completo es obligatorio.");
            esValido = false;
        } else if (nombreCompleto.length < 3) {
            mostrarError("#NombreCompleto", "El nombre debe tener al menos 3 caracteres.");
            esValido = false;
        }

        if (cedula === "") {
            mostrarError("#Cedula", "La cédula es obligatoria.");
            esValido = false;
        } else if (cedula.length < 5) {
            mostrarError("#Cedula", "La cédula debe tener al menos 5 caracteres.");
            esValido = false;
        }

        if (!fechaNacimiento) {
            mostrarError("#FechaNacimiento", "La fecha de nacimiento es obligatoria.");
            esValido = false;
        } else {
            var hoy = new Date();
            hoy.setHours(0, 0, 0, 0);
            var fechaSeleccionada = new Date(fechaNacimiento);
            if (fechaSeleccionada > hoy) {
                mostrarError("#FechaNacimiento", "La fecha de nacimiento no puede ser una fecha futura.");
                esValido = false;
            }
        }

        if (!sexo) {
            mostrarError("#Sexo", "Debe seleccionar el sexo.");
            esValido = false;
        }

        if (correo === "") {
            mostrarError("#CorreoElectronico", "El correo electrónico es obligatorio.");
            esValido = false;
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(correo)) {
            mostrarError("#CorreoElectronico", "El formato del correo no es válido.");
            esValido = false;
        }

        if (telefono.length > 20) {
            mostrarError("#Telefono", "El teléfono no puede exceder los 20 caracteres.");
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
