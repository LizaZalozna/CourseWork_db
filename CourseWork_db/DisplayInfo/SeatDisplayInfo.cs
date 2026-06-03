namespace CourseWork_db.DisplayInfo;

public class SeatDisplayInfo
{
    public int    SeatId         { get; set; }
    public int    SeatNumber     { get; set; }
    public int    CarNumber      { get; set; }
    public string CarTypeName    { get; set; } = "";
    public float  Price          { get; set; }
    public string PriceInfo      { get; set; } = "";
    public string PriorityName   { get; set; } = "";
    public string Characteristics { get; set; } = "";

    public override string ToString()
    {
        var line1 = $"Вагон {CarNumber} | {CarTypeName} | М #{SeatNumber} | {PriorityName} | {Price:F2} грн ({PriceInfo})";
        if (string.IsNullOrEmpty(Characteristics))
            return line1;
        return $"{line1}\n{Characteristics}";
    }
}