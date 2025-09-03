using Web.Services;
using YourProjectName.Services;

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    //builder.Services.AddControllers();
    builder.Services.AddControllersWithViews(); // Добавьте поддержку MVC вместо AddControllers()




    //// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddHttpClient();
    builder.Services.AddScoped<StreamingJsonProcessor>();
    builder.Services.AddControllers();

    builder.Services.AddControllersWithViews();
    builder.Services.AddHttpClient();
    builder.Services.AddScoped<IAdvertisingService, AdvertisingService>();

var app = builder.Build();



    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    // Для статических файлов (CSS, JS, изображения)
    app.UseStaticFiles();
    app.UseRouting();

    // Добавьте поддержку MVC маршрутов
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    });

    app.MapControllers();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapControllerRoute(
        name: "api",
        pattern: "api/{controller=Advertising}/{action=GetAll}/{id?}");

    app.Run();
