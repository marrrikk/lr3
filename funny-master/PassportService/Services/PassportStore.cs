using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using PassportService.Models;
using RabbitMQ.Client;
namespace PassportService.Services;

public class PassportStore(IConfiguration configuration)
{
    public void SaveAndPublish(JsonObject user, PassportData passport)
    {
        var directory = configuration["PassportDirectory"] ?? "data/passports";
        Directory.CreateDirectory(directory);
        // Имя файла не зависит от пользовательского ввода.
        var file = Path.Combine(directory, Guid.NewGuid() + ".json");
        var message = JsonSerializer.Serialize(new { User = user, Passport = passport });
        File.WriteAllText(file, message);

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "localhost",
            Port = configuration.GetValue("RabbitMq:Port", 5672),
            UserName = configuration["RabbitMq:Username"] ?? "guest",
            Password = configuration["RabbitMq:Password"] ?? "guest"
        };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        var queue = configuration["RabbitMq:Queue"] ?? "user-passports";
        channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        channel.ConfirmSelect();
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        channel.BasicPublish("", queue, properties, Encoding.UTF8.GetBytes(message));
        channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));
    }
}
