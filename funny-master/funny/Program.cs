using Microsoft.EntityFrameworkCore;
using funny.Data;
using funny.Logic;
using ProxyUsers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("FunnyDb"));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IOldSexLogic, OldSexLogic>();

builder.Services.AddHttpClient<IUserProxy, UserProxy>(client =>
    client.BaseAddress = new Uri(builder.Configuration["UserServiceUrl"] ?? "http://localhost:8000/"));
builder.Services.AddSingleton<ClientService>(provider => new ClientService(
    provider.GetRequiredService<IUserProxy>(),
    new HttpClient { BaseAddress = new Uri(builder.Configuration["PassportServiceUrl"] ?? "http://localhost:8008/") }));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Funny API V1");
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();



using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        context.Database.EnsureCreated();

        if (!context.DigitalServices.Any())
        {
            var defaultServices = new[]
            {
                new funny.Models.DigitalService { Name = "Разработка Landing Page", Description = "Рекламная страница для компании", Price = 15000 },
                new funny.Models.DigitalService { Name = "Разработка сайта", Description = "Сайт для вашего бизнеса", Price = 45000 },
                new funny.Models.DigitalService { Name = "Настройка 1С", Description = "Установка и настройка 1С", Price = 10000 },
                new funny.Models.DigitalService { Name = "SEO продвижение", Description = "Продвижение сайта в поиске", Price = 20000 },
                new funny.Models.DigitalService { Name = "Техническая поддержка", Description = "Помощь пользователям", Price = 8000 }
            };

            context.DigitalServices.AddRange(defaultServices);
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при заполнении списка услуг.");
    }
}

app.Run();
