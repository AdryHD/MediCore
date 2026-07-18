$(document).ready(function () {
    $("#DoctorForm").submit(function (e) {
        $(".text-danger").remove();
        $(".form-control,.form-select").removeClass("is-invalid");

        var esValido = true;

        var nombreCompleto = $("#NombreCompleto").val().trim();
        var cedula = $("#Cedula").val().trim();
        var codigoColegiado = $("#CodigoColegiado").val().trim();
        var correo = $("#Correo").val().trim();
        var telefono = $("#Telefono").val().trim();
        var idEspecialidad = $("#IdEspecialidad").val();

        if (nombreCompleto === "") {
            mostrarError("#NombreCompleto", "El nombre completo es obligatorio.");
            esValido = false;
        } else if (nombreCompleto.length < 3) {
            mostrarError("#NombreCompleto", "El nombre debe tener al menos 3 caracteres.");
            esValido = false;
        }

        if (cedula === "") {
            mostrarError("#Cedula", "La cédula profesional es obligatoria.");
            esValido = false;
        } else if (cedula.length < 5) {
            mostrarError("#Cedula", "La cédula debe tener al menos 5 caracteres.");
            esValido = false;
        }

        if (codigoColegiado === "") {
            mostrarError("#CodigoColegiado", "El código colegiado es obligatorio.");
            esValido = false;
        }

        if (correo === "") {
            mostrarError("#Correo", "El correo electrónico es obligatorio.");
            esValido = false;
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(correo)) {
            mostrarError("#Correo", "El formato del correo no es válido.");
            esValido = false;
        }

        if (telefono.length > 20) {
            mostrarError("#Telefono", "El teléfono no puede exceder los 20 caracteres.");
            esValido = false;
        }

        if (!idEspecialidad) {
            mostrarError("#IdEspecialidad", "Debe seleccionar una especialidad.");
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

    // Modal de confirmación para desactivar/activar un doctor (Index)
    var modalElement = document.getElementById("mcConfirmModal");
    if (modalElement && typeof bootstrap !== "undefined") {
        var mcModal = new bootstrap.Modal(modalElement);
        var formIdPendiente = null;

        $(".btn-confirmar-estado").on("click", function () {
            formIdPendiente = $(this).data("form-id");
            var nombre = $(this).data("nombre");
            $("#mcConfirmText").text('El doctor "' + nombre + '" dejará de estar disponible para programar nuevas citas.');
            mcModal.show();
        });

        $("#mcConfirmBtn").on("click", function () {
            if (formIdPendiente) {
                document.getElementById(formIdPendiente).submit();
            }
        });
    }
});
