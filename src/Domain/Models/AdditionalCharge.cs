namespace Domain.Models;

/// <summary>
/// One-off charge (or discount, when negative) for an apartment in a specific month.
/// </summary>
public class AdditionalCharge
{
    public Guid Id { get; set; }
    public Guid ApartmentId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
}
