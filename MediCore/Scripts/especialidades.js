$(document).ready(function () {
    $("#EspecialidadForm").submit(function (e) {
        $(".text-danger").remove();
        $(".form-control").removeClass("is-invalid");

        var esValido = true;

        var nombre = $("#Nombre").val().trim();
        var descripcion = $("#Descripcion").val().trim();

        if (nombre === "") {
            mostrarError("#Nombre", "El nombre de la especialidad es obligatorio.");
            esValido = false;
        } else if (nombre.length < 3) {
            mostrarError("#Nombre", "El nombre debe tener al menos 3 caracteres.");
            esValido = false;
        } else if (nombre.length > 80) {
            mostrarError("#Nombre", "El nombre no puede exceder los 80 caracteres.");
            esValido = false;
        }

        if (descripcion.length > 255) {
            mostrarError("#Descripcion", "La descripción no puede exceder los 255 caracteres.");
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

    // Modal de confirmación para desactivar una especialidad (Index)
    var modalElement = document.getElementById("mcConfirmModal");
    if (modalElement && typeof bootstrap !== "undefined") {
        var mcModal = new bootstrap.Modal(modalElement);
        var formIdPendiente = null;

        $(".btn-confirmar-estado").on("click", function () {
            formIdPendiente = $(this).data("form-id");
            var nombre = $(this).data("nombre");
            $("#mcConfirmText").text('La especialidad "' + nombre + '" dejará de estar disponible para programar nuevas citas.');
            mcModal.show();
        });

        $("#mcConfirmBtn").on("click", function () {
            if (formIdPendiente) {
                document.getElementById(formIdPendiente).submit();
            }
        });
    }
});
