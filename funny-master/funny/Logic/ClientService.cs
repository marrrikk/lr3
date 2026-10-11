using System.Net.Http.Json;
using System.Text.Json.Nodes;
using funny.Models;
using ProxyUsers;
namespace funny.Logic;

// Один экземпляр: счётчик общий для колы, пиццы и обратной связи.
public class ClientService(IUserProxy users, HttpClient passports)
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly Dictionary<string, (DateTime Last, int Count)> attempts = new();

    public async Task<bool> Send(ClientRequest request)
    {
        var id = request.Phone!.Trim();
        await gate.WaitAsync();
        try
        {
            var user = await users.GetUser(id);
            if (user != null && (user["archive"]?.GetValue<bool>() == true || user["blocked"]?.GetValue<bool>() == true))
                return false;
            if (user == null)
                user = await users.SaveUser(new JsonObject
                {
                    ["id"] = id, ["newId"] = id, ["firstName"] = request.Name,
                    ["surname"] = request.Surname ?? "Не указано",
                    ["middleName"] = request.MiddleName ?? "Не указано",
                    ["email"] = request.Email ?? id + "@example.test",
                    ["department"] = "Клиенты", ["organization"] = "Клиенты",
                    ["phoneNumber"] = id, ["password"] = "Aa1!" + Guid.NewGuid().ToString("N"),
                    ["roles"] = new JsonArray("Client"), ["archive"] = false, ["blocked"] = false
                }, true);

            var now = DateTime.UtcNow;
            var count = attempts.TryGetValue(id, out var previous) && now - previous.Last < TimeSpan.FromMinutes(1)
                ? previous.Count + 1 : 1;
            attempts[id] = (now, count);
            if (count > 3)
            {
                // Сервис требует эти поля даже при обновлении флагов.
                user["newId"] = id;
                user["password"] = "Aa1!" + Guid.NewGuid().ToString("N");
                user["department"] = user["department"]?.GetValue<string>() ?? "Клиенты";
                user["organization"] = user["organization"]?.GetValue<string>() ?? "Клиенты";
                user["archive"] = true;
                user["blocked"] = true;
                await users.SaveUser(user, false);
                return false;
            }
            using var response = await passports.PostAsJsonAsync("passports", new
            {
                UserId = id, request.SerialNumber, request.Number
            });
            response.EnsureSuccessStatusCode();
            return true;
        }
        finally { gate.Release(); }
    }
}
