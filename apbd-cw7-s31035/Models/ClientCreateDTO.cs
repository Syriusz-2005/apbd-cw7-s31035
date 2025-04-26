using System.ComponentModel.DataAnnotations;

namespace apbd_cw7_s31035.Models;

public class ClientCreateDTO
{
    [MaxLength(120)]
    public required string FirstName { get; set; }
    [MaxLength(120)]
    public required string LastName { get; set; }
    [MaxLength(120)]
    public required string Email { get; set; }
    [MaxLength(120)]
    public required string Telephone { get; set; }
    [MaxLength(120)]
    public required string Pesel { get; set; }
}