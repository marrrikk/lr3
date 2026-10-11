using System.Text.Json.Nodes;
namespace ProxyUsers;

public interface IUserProxy
{
    Task<JsonObject?> GetUser(string id);
    Task<JsonObject> SaveUser(JsonObject user, bool isNewUser);
}
