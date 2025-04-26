namespace apbd_cw7_s31035.Models;

public class Trip
{
    public required int IdTrip { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required DateTime DateFrom { get; set; }
    public required DateTime DateTo { get; set; }
    public required int MaxPeople { get; set; }
}