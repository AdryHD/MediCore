$(document).ready(function () {


    $.validator.addMethod("maxSize", function (value, element, param) {
        if (element.files && element.files[0]) {
            return element.files[0].size <= param;
        }
        return true;
    }, "El archivo no debe superar los 5 MB.");


    $.validator.addMethod("formatoPermitido", function (value, element) {
        if (element.files && element.files[0]) {
            var file = element.files[0];


            var mimesValidos = ["application/pdf", "image/jpeg", "image/png"];
            var mimeValido = mimesValidos.indexOf(file.type) !== -1;

            var extValida = /\.(pdf|jpg|jpeg|png)$/i.test(file.name);

            return mimeValido || extValida; 
        }
        return true;
    }, "Solo se permiten formatos de imagen (JPG, PNG) y documentos PDF.");


    var validator = $("#formArchivo").validate({
        rules: {
            Id_Expediente: {
                required: true
            },
            Nombre: {
                required: true,
                maxlength: 100
            },
            Estado: {
                required: true
            },
            archivoSubido: {
                required: true,
                maxSize: 5 * 1024 * 1024,    
                formatoPermitido: true
            }
        },
        messages: {
            Id_Expediente: {
                required: "Debe seleccionar un expediente de paciente."
            },
            Nombre: {
                required: "El nombre del archivo es obligatorio.",
                maxlength: "El nombre no puede exceder los 100 caracteres."
            },
            Estado: {
                required: "Debe seleccionar un estado para el registro."
            },
            archivoSubido: {
                required: "Debe seleccionar un archivo para cargar."
            }
        },
        errorElement: 'span',
        errorClass: 'text-danger small d-block mt-1',
        highlight: function (element) {
            $(element).addClass('is-invalid').removeClass('is-valid');
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid').addClass('is-valid');
        },
        errorPlacement: function (error, element) {
            error.insertAfter(element);
        }
    });

    $('#archivoSubido').on('change', function () {
        $(this).valid(); 
    });

});