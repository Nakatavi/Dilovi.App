namespace Domain.Models;

public class MeterReading
{
    public Guid Id { get; set; }
    public Guid ApartmentId { get; set; }
    public MeterType MeterType { get; set; }

    /// <summary>Meter value in cubic meters.</summary>
    public decimal Value { get; set; }

    public DateOnly ReadingDate { get; set; }

    /// <summary>
    /// True for the initial reading of a newly installed meter (meter replacement).
    /// Consumption is not calculated between the old meter's last reading and this one.
    /// </summary>
    public bool IsNewMeter { get; set; }
}
