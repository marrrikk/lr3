using System.ComponentModel.DataAnnotations;
namespace PassportService.Models;

public class PassportData
{
    [Required] public string UserId { get; set; } = "";
    [Required, RegularExpression(@"^\d{4}$")] public string SerialNumber { get; set; } = "";
    [Required, RegularExpression(@"^\d{6}$")] public string Number { get; set; } = "";
}
