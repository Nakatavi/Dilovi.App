namespace Domain.Enums;

/// <summary>
/// Defines what a tariff price is multiplied by.
/// </summary>
public enum ChargeBasis
{
    /// <summary>Metered consumption for the period (m³).</summary>
    MeterConsumption = 1,

    /// <summary>Apartment area (m²).</summary>
    Area = 2,

    /// <summary>Number of registered residents.</summary>
    PerResident = 3
}
