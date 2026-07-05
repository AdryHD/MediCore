$(document).ready(function () {
    $("#RegistroForm").submit(function (e) {
        $(".text-danger").remove();
        $(".form-control").removeClass("is-invalid");

        var esValido = true;

        var nombre = $("#Nombre").val().trim();
        var cedula = $("#Cedula").val().trim();
        var fechaNacimiento = $("#FechaNacimiento").val();
        var telefono = $("#Telefono").val().trim();
        var correo = $("#Correo").val().trim();
        var contrasenna = $("#Contrasenna").val();
        var confirmarContrasenna = $("#ConfirmarContrasenna").val();

        var regexCorreo = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$/;


        if (nombre === "") {
            mostrarError("#Nombre", "El nombre completo es obligatorio.");
            esValido = false;
        }

        if (cedula === "") {
            mostrarError("#Cedula", "El documento de identidad es obligatorio.");
            esValido = false;
        } else if (cedula.length < 9) {
            mostrarError("#Cedula", "La cédula debe tener al menos 9 dígitos.");
            esValido = false;
        }

        if (fechaNacimiento === "") {
            mostrarError("#FechaNacimiento", "La fecha de nacimiento es obligatoria.");
            esValido = false;
        }

  
        if (telefono === "") {
            mostrarError("#Telefono", "El número de teléfono es obligatorio.");
            esValido = false;
        } else if (telefono.length < 8) {
            mostrarError("#Telefono", "El teléfono debe tener al menos 8 dígitos.");
            esValido = false;
        }


        if (correo === "") {
            mostrarError("#Correo", "El correo electrónico es obligatorio.");
            esValido = false;
        } else if (!regexCorreo.test(correo)) {
            mostrarError("#Correo", "Por favor, ingrese un correo electrónico válido.");
            esValido = false;
        }


        if (contrasenna === "") {
            mostrarError("#Contrasenna", "La contraseña es obligatoria.");
            esValido = false;
        } else if (contrasenna.length < 8) {
            mostrarError("#Contrasenna", "La contraseña debe tener al menos 8 caracteres.");
            esValido = false;
        }

     
        if (confirmarContrasenna === "") {
            mostrarError("#ConfirmarContrasenna", "Debe confirmar su contraseña.");
            esValido = false;
        } else if (contrasenna !== confirmarContrasenna) {
            mostrarError("#ConfirmarContrasenna", "Las contraseñas no coinciden.");
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