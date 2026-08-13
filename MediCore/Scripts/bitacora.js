// bitacora.js — Lógica del modal de detalle de registro de bitácora

(function () {
    'use strict';

    function setText(id, val) {
        var el = document.getElementById(id);
        if (el) el.textContent = val || '—';
    }

    function setHtml(id, html) {
        var el = document.getElementById(id);
        if (el) el.innerHTML = html;
    }

    // Poblar el modal con todos los datos del registro seleccionado
    document.addEventListener('click', function (e) {
        var boton = e.target.closest('.btn-detalle-bitacora');
        if (!boton) return;

        var nivel   = boton.getAttribute('data-nivel') || '';
        var stack   = boton.getAttribute('data-stack') || '';

        setText('detalleFecha',   boton.getAttribute('data-fecha'));
        setText('detalleUsuario', boton.getAttribute('data-usuario'));
        setText('detalleModulo',  boton.getAttribute('data-modulo'));
        setText('detalleAccion',  boton.getAttribute('data-accion'));
        setText('detalleIp',      boton.getAttribute('data-ip'));
        setText('detalleMensaje', boton.getAttribute('data-mensaje'));
        setText('detalleStack',   stack || '—');

        // Badge de nivel
        var nivelCls = nivel === 'ERROR' ? 'badge-nivel badge-nivel-error' : 'badge-nivel badge-nivel-info';
        setHtml('detalleNivel', '<span class="' + nivelCls + '">' + (nivel || '—') + '</span>');

        // Mostrar/ocultar sección stack trace
        var stackWrap = document.getElementById('detalleStackWrap');
        if (stackWrap) stackWrap.style.display = stack ? '' : 'none';
    });
})();
