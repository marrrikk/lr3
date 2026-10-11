using System.ComponentModel.DataAnnotations;
namespace funny.Models;

public class ClientRequest
{
    [Required] public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? MiddleName { get; set; }
    [EmailAddress] public string? Email { get; set; }
    [Required] public string? Phone { get; set; }
    [Required, RegularExpression(@"^\d{4}$")] public string? SerialNumber { get; set; }
    [Required, RegularExpression(@"^\d{6}$")] public string? Number { get; set; }
}
