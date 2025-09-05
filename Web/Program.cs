using Web.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);  // Создание билдера приложения с предустановленной конфигурацией





    // Add services to the container.


    // Добавление сервисов в контейнер зависимостей

    // Альтернативная регистрация контроллеров - используем MVC вместо простых контроллеров
    // для поддержки представлений (Views)
    //builder.Services.AddControllers();
    builder.Services.AddControllersWithViews(); // Добавьте поддержку MVC вместо AddControllers()




    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    
    builder.Services.AddEndpointsApiExplorer();// Добавление сервиса для исследования API endpoints
    builder.Services.AddSwaggerGen();// Добавление генератора Swagger для документации API

    builder.Services.AddHttpClient(); // Регистрация HttpClient для выполнения HTTP-запросов
    builder.Services.AddScoped<StreamingJsonProcessor>();   // Регистрация кастомного сервиса для обработки JSON в потоковом режиме
    
   
    builder.Services.AddScoped<IAdvertisingService, AdvertisingService>(); // Регистрация сервиса с его реализацией

// Настройка Swagger с кастомным css и js
    builder.Services.AddSwaggerGen(c =>
    {   
            // Конфигурация документа Swagger
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Advertising API",
                Version = "v1",
                Description = "API для управления рекламными площадками"
            });

    });


    var app = builder.Build();// Построение приложения на основе конфигурации




    // Настройка конвейера обработки HTTP-запросов
    // Включение Swagger только в режиме разработки
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(); // Подключение middleware для генерации Swagger JSON
            
            // Подключение Swagger UI интерфейса
            app.UseSwaggerUI(c =>
            {
                
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Advertising API v1");    // Указание endpoint'а для Swagger JSON
                c.RoutePrefix = "swagger";                                              // Установка префикса маршрута для Swagger UI
                c.IndexStream = () => File.OpenRead("wwwroot/swagger/index.html");      // Указание кастомного HTML-файла для Swagger UI

                //old
                //c.SwaggerEndpoint("/swagger/v1/swagger.json", "Advertising API v1");
                //c.RoutePrefix = "swagger"; // Доступ по /swagger
                //c.DocumentTitle = "Advertising API Documentation";

                //// ПРАВИЛЬНОЕ место для InjectStylesheet и InjectJavascript
                //c.SwaggerEndpoint("/swagger/v1/swagger.json", "Advertising API v1");
                //c.RoutePrefix = "swagger"; // Доступ по /swagger
                //c.DocumentTitle = "Advertising API Documentation";
            });
    }


    app.UseHttpsRedirection(); // Перенаправление HTTP запросов на HTTPS

    app.UseAuthorization(); // Подключение middleware для авторизации

    
    app.UseStaticFiles();   // Для статических файлов (css, js, изображения)
    app.UseRouting(); // Подключение маршрутизации

    // Настройка endpoints с поддержкой MVC маршрутов
    app.UseEndpoints(endpoints =>
    {
        // Регистрация default маршрута для MVC
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}"); // Шаблон маршрута
    });

    app.MapControllers(); // Cопоставления входящих HTTP-запросов с конкретными контроллерами

    // Регистрация API маршрута с конкретным контроллером и действием по умолчанию
    app.MapControllerRoute(
        name: "api",
        pattern: "api/{controller=Advertising}/{action=GetAll}/{id?}");

    app.Run();
