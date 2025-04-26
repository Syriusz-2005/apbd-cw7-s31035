namespace apbd_cw7_s31035.Models;

public class ClientTrip
{
    public required int IdClient { get; set; }
    public required int IdTrip { get; set; }
    public required int RegisteredAt { get; set; }
    public required int? PaymentDate { get; set; }
}