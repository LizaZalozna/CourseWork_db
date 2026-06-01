namespace CourseWork_db.Services;

public class PricingService
{
    public (float Price, string PriceInfo) Calculate(
        float segmentDistance,
        float pricePerKm,
        float servicePrice,
        string? priorityName)
    {
        var basePrice = segmentDistance * pricePerKm + servicePrice;

        return priorityName switch
        {
            "Низький" => (basePrice * 1.15f, "+15%"),
            "Високий" => (basePrice * 0.85f, "-15%"),
            _ => (basePrice, "звичайна"),
        };
    }
}
