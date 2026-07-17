// Manejo del botón de colapsar/expandir el sidebar en el layout principal
document.addEventListener('DOMContentLoaded', function () {
    var sidebar = document.getElementById('sidebar');
    var main = document.getElementById('main');
    var burgerBtn = document.querySelector('.burger-btn');

    if (burgerBtn && sidebar) {
        burgerBtn.addEventListener('click', function (e) {
            e.preventDefault();
            sidebar.classList.toggle('hide');
            if (main) main.classList.toggle('expanded');
        });
    }
});
