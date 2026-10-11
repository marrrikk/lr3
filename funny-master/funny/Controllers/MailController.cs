using funny.Logic;
using funny.Models;
using Microsoft.AspNetCore.Mvc;
namespace funny.Controllers;

[ApiController]
[Route("mail")]
public class MailController(ClientService clients, ILogger<MailController> logger) : ControllerBase
{
    [HttpPost("Send")]
    public async Task<IActionResult> Send(ConnectDialogVM request)
    {
        try
        {
            if (!await clients.Send(request)) return StatusCode(403, "Вы не обслуживаетесь.");
            logger.LogInformation("Получена обратная связь. Имя: {Name}, Телефон: {Phone}, Задачи: {Tasks}",
                request.Name, request.Phone, string.Join(", ", request.RequiredTask ?? []));
            return Ok(new { message = "Заявка принята." });
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, "Сервис пользователей или паспортов недоступен.");
        }
    }
}
