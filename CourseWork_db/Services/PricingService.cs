using System;
using System.Collections.Generic;

namespace CourseWork_db.Services;

public class PricingService
{
    public (float Price, string PriceInfo) Calculate(
        float segmentDistance,
        float pricePerKm,
        int fromOrder,
        int toOrder,
        int totalStations,
        List<(int FromOrder, int ToOrder)> existingTicketOrders)
        => ComputePrice(segmentDistance, pricePerKm, fromOrder, toOrder,
                        totalStations, existingTicketOrders, detailed: false);

    public (float Price, string PriceInfo) CalculateDetailed(
        float segmentDistance,
        float pricePerKm,
        int fromOrder,
        int toOrder,
        int totalStations,
        List<(int FromOrder, int ToOrder)> existingTicketOrders)
        => ComputePrice(segmentDistance, pricePerKm, fromOrder, toOrder,
                        totalStations, existingTicketOrders, detailed: true);

    private (float Price, string PriceInfo) ComputePrice(
        float segmentDistance,
        float pricePerKm,
        int fromOrder,
        int toOrder,
        int totalStations,
        List<(int FromOrder, int ToOrder)> existingTicketOrders,
        bool detailed)
    {
        var basePrice             = segmentDistance * pricePerKm;
        var requestedSegmentRatio = (toOrder - fromOrder) / (float)(totalStations - 1);
        
        if (existingTicketOrders.Count == 0)
        {
            if (requestedSegmentRatio < 0.3f)
                return (
                    basePrice * 1.3f,
                    detailed
                        ? $"Надбавка +30%. Короткий відрізок ({requestedSegmentRatio:P0}), місце вільне на всьому маршруті."
                        : "+30%"
                );

            if (requestedSegmentRatio > 0.7f)
                return (
                    basePrice * 0.9f,
                    detailed
                        ? $"Знижка -10%. Довгий відрізок ({requestedSegmentRatio:P0}), місце вільне на всьому маршруті."
                        : "-10%"
                );

            return (
                basePrice,
                detailed ? "Звичайна ціна. Місце вільне на всьому маршруті." : "звичайна"
            );
        }
        
        var ticketCount     = existingTicketOrders.Count;
        var discountPercent = Math.Min(50, ticketCount * 10);
        var finalPrice      = basePrice * (1 - discountPercent / 100f);

        return (
            finalPrice,
            detailed
                ? $"Знижка -{discountPercent}%. На цьому місці вже є {ticketCount} квиток(ів) на інших ділянках маршруту."
                : $"-{discountPercent}%"
        );
    }
}