namespace Domain.Models;

public class Tariff
{
    public Guid Id { get; set; }
    public ServiceType ServiceType { get; set; }
    public ChargeBasis ChargeBasis { get; set; }

    /// <summary>Price in UAH per unit of <see cref="ChargeBasis"/>.</summary>
    public decimal PricePerUnit { get; set; }

    public DateOnly ValidFrom { get; set; }

    /// <summary>Inclusive end date; null means the tariff is valid until further notice.</summary>
    public DateOnly? ValidTo { get; set; }

    public bool Overlaps(DateOnly from, DateOnly to) => ValidFrom <= to && (ValidTo is null || ValidTo >= from);
}
