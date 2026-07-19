$(document).ready(function () {
    $("#IndexForm").submit(function (e) {
        $(".text-danger").remove();
        $(".form-control").removeClass("is-invalid");

        var esValido = true;

        var correo = $("#Correo").val().trim();
        var contrasena = $("#Contrasena").val();
        var regexCorreo = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$/;

        if (correo === "") {
            mostrarError("#Correo", "El correo electrónico es obligatorio.");
            esValido = false;
        } else if (!regexCorreo.test(correo)) {
            mostrarError("#Correo", "Por favor, ingrese un correo electrónico válido.");
            esValido = false;
        }

        if (contrasena === "") {
            mostrarError("#Contrasena", "La contraseña es obligatoria.");
            esValido = false;
        } else if (contrasena.length < 6) {
            mostrarError("#Contrasena", "La contraseña debe tener al menos 6 caracteres.");
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
