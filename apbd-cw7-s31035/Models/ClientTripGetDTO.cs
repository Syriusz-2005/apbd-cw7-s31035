namespace apbd_cw7_s31035.Models;

public class ClientTripGetDTO
{
    public required int IdClient { get; set; }
    public required int RegisteredAt { get; set; }
    public required int? PaymentDate { get; set; }
    public required Trip TripDetails { get; set; }
}