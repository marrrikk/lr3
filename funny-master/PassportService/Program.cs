using ProxyUsers;
using PassportService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<IUserProxy, UserProxy>(client =>
    client.BaseAddress = new Uri(builder.Configuration["UserServiceUrl"] ?? "http://localhost:8000/"));
builder.Services.AddSingleton<PassportStore>();
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
