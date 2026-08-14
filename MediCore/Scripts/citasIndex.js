// citasIndex.js
// Lógica del modal de cancelación de citas en la vista Index.
// La URL de la acción se lee del atributo data-cancelar-url del formulario.

$(document).ready(function () {

    document.querySelectorAll('.btn-confirmar-cancelar').forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            e.preventDefault();

            var citaId = btn.getAttribute('data-cita-id');
            var form   = document.getElementById('mcCancelarForm');

            document.getElementById('mcCancelarId').value    = citaId;
            form.action                                       = form.getAttribute('data-cancelar-url');
            document.getElementById('motivoCancelacionInput').value = '';
            document.getElementById('mcCancelarMotivo').value      = '';

            var modal = new bootstrap.Modal(document.getElementById('mcCancelarModal'));

            // Sincronizar textarea → input hidden justo antes del envío
            form.addEventListener('submit', function () {
                document.getElementById('mcCancelarMotivo').value =
                    document.getElementById('motivoCancelacionInput').value;
            }, { once: true });

            modal.show();
        });
    });

});
