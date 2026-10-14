namespace Domain.Models;

public class Apartment
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;

    /// <summary>Total area in square meters.</summary>
    public decimal Area { get; set; }

    public int ResidentsCount { get; set; }
}
