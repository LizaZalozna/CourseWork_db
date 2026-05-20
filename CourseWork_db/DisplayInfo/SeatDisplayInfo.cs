namespace CourseWork_db.DisplayInfo;

public class SeatDisplayInfo
{
    public int    SeatId      { get; set; }
    public int    SeatNumber  { get; set; }
    public int    CarId       { get; set; }
    public int    CarNumber   { get; set; }
    public string CarTypeName { get; set; } = "";
    public bool   IsWindow    { get; set; }
    public bool   IsUpper     { get; set; }
    public float  Price       { get; set; }
    public string PriceInfo   { get; set; } = "";

    public override string ToString() =>
        $"Вагон {CarNumber} | {CarTypeName} | М #{SeatNumber} | {(IsUpper ? "Верх" : "Низ")} | {(IsWindow ? "Вік" : "Прх")} | {Price:F2} грн ({PriceInfo})";
}