using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CourseWork_db.Models;

namespace CourseWork_db.Helpers;

public static class ComboTemplates
{
    public static FuncDataTemplate<Station> Station { get; } =
        new((s, _) => new TextBlock { Text = $"{s.Name} ({s.City}, {s.Country})" });

    public static FuncDataTemplate<Route> Route { get; } =
        new((r, _) => new TextBlock { Text = r.Name });

    public static FuncDataTemplate<Train> Train { get; } =
        new((t, _) => new TextBlock { Text = t.Name ?? "" });

    public static FuncDataTemplate<CarType> CarType { get; } =
        new((c, _) => new TextBlock { Text = c.Name });
}
