// Добавляем кнопку перехода на главную в Swagger UI
window.onload = function() {
    // Ждем загрузки Swagger UI
    setTimeout(function() {
        // Ищем верхнюю панель
        const topbar = document.querySelector('.topbar-wrapper');
        
        if (topbar) {
            // Создаем кнопку
            const homeButton = document.createElement('a');
            homeButton.href = '/';
            homeButton.className = 'home-button';
            homeButton.textContent = 'На главную';
            homeButton.title = 'Перейти на главную страницу приложения';
            
            // Вставляем кнопку перед логотипом
            topbar.insertBefore(homeButton, topbar.firstChild);
            
            // Добавляем кнопку также в мобильное меню
            const mobileMenu = document.querySelector('.download-url-wrapper');
            if (mobileMenu) {
                const mobileHomeButton = homeButton.cloneNode(true);
                mobileHomeButton.style.marginTop = '10px';
                mobileHomeButton.style.display = 'block';
                mobileHomeButton.style.textAlign = 'center';
                mobileMenu.appendChild(mobileHomeButton);
            }
        }
        
        // Альтернативное размещение - если не нашли топбар
        const alternativeContainer = document.querySelector('.information-container');
        if (alternativeContainer && !document.querySelector('.home-button')) {
            const homeButton = document.createElement('a');
            homeButton.href = '/';
            homeButton.className = 'home-button';
            homeButton.textContent = 'На главную';
            homeButton.style.marginLeft = '20px';
            alternativeContainer.appendChild(homeButton);
        }
    }, 1000); // Задержка для полной загрузки Swagger UI
};

// Добавляем обработчик для всех ссылок Swagger
document.addEventListener('click', function(e) {
    if (e.target.classList.contains('home-button')) {
        e.preventDefault();
        window.location.href = e.target.href;
    }
});