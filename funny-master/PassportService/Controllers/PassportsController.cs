using Microsoft.AspNetCore.Mvc;
using PassportService.Models;
using PassportService.Services;
using ProxyUsers;
namespace PassportService.Controllers;

[ApiController]
[Route("passports")]
public class PassportsController(IUserProxy users, PassportStore store, ILogger<PassportsController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Save(PassportData passport)
    {
        try
        {
            var user = await users.GetUser(passport.UserId);
            if (user == null) return NotFound("Пользователь не найден.");
            if (user["archive"]?.GetValue<bool>() == true || user["blocked"]?.GetValue<bool>() == true)
                return StatusCode(403, "Вы не обслуживаетесь.");
            store.SaveAndPublish(user, passport);
            return Ok(new { message = "Паспорт сохранён и отправлен в очередь." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Не удалось сохранить паспорт или отправить его в очередь");
            return StatusCode(503, "Сервис пользователей или RabbitMQ недоступен. Запрос не завершён.");
        }
    }
}
