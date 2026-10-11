using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
namespace ProxyUsers;

// Общая библиотека: используется и основным API, и сервисом паспортов.
public class UserProxy(HttpClient client) : IUserProxy
{
    public async Task<JsonObject?> GetUser(string id)
    {
        using var response = await client.GetAsync("users/" + Uri.EscapeDataString(id));
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonObject>();
    }

    public async Task<JsonObject> SaveUser(JsonObject user, bool isNewUser)
    {
        using var response = await client.PostAsJsonAsync("users/" + isNewUser.ToString().ToLower(), user);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonObject>()
            ?? throw new InvalidOperationException("Сервис пользователей вернул пустой ответ.");
    }
}
