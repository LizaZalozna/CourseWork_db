using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CourseWork_db.Helpers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using CourseWork_db.Models;

namespace CourseWork_db.Controls;

public partial class RouteStationCard : UserControl
{
    public event Action? Changed;
    public event Action<RouteStationCard>? SelectRequested;

    public int? StationId => (StationCombo.SelectedItem as StationOption)?.Station.Id;
    public float Distance => float.TryParse(DistanceBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)
        ? d
        : (float.TryParse(DistanceBox.Text, out d) ? d : 0);

    public int DayOffset => int.TryParse(DayOffsetBox.Text, out var d) ? Math.Max(d, 0) : 0;
    public TimeOnly? ArrivalTime => ArrivalTimePicker.SelectedTime.HasValue
        ? TimeOnly.FromTimeSpan(ArrivalTimePicker.SelectedTime.Value)
        : null;
    public TimeOnly? DepartureTime => DepartureTimePicker.SelectedTime.HasValue
        ? TimeOnly.FromTimeSpan(DepartureTimePicker.SelectedTime.Value)
        : null;

    public RouteStationCard()
    {
        InitializeComponent();

        NumberText.PointerPressed += OnSelectPointerPressed;
        CardBorder.PointerPressed += OnSelectPointerPressed;

        StationCombo.SelectionChanged += (_, _) => Changed?.Invoke();
        ArrivalTimePicker.SelectedTimeChanged += (_, _) => Changed?.Invoke();
        DepartureTimePicker.SelectedTimeChanged += (_, _) => Changed?.Invoke();
        DayOffsetBox.TextChanged += (_, _) => Changed?.Invoke();
        DistanceBox.TextChanged += (_, _) => Changed?.Invoke();
    }

    private void OnSelectPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (IsFromInputControl(e.Source)) return;
        SelectRequested?.Invoke(this);
    }

    private static bool IsFromInputControl(object? source)
    {
        if (source is TextBox or ComboBox or TimePicker) return true;

        if (source is Visual visual)
        {
            foreach (var parent in visual.GetVisualAncestors())
            {
                if (parent is TextBox or ComboBox or TimePicker) return true;
            }
        }

        return false;
    }

    public void SetStations(IEnumerable<Station> stations)
    {
        var currentId = StationId;
        var options = stations.Select(s => new StationOption(s)).ToList();
        StationCombo.ItemsSource = options;
        StationCombo.ItemTemplate = null;

        if (currentId is null) return;

        var match = options.FirstOrDefault(o => o.Station.Id == currentId);
        if (match != null) StationCombo.SelectedItem = match;
    }

    public void UpdateNumber(int n) => NumberText.Text = $"Станція #{n}";

    public void SetMode(bool isFirst, bool isLast)
    {
        ArrivalTimePicker.IsEnabled = !isFirst;
        DepartureTimePicker.IsEnabled = !isLast;
        DistanceBox.IsEnabled = !isLast;
    }

    public void SelectBorder(bool selected)
    {
        CardBorder.BorderBrush = selected
            ? new SolidColorBrush(Color.Parse("#E91E8C"))
            : new SolidColorBrush(Color.Parse("#F8BBD9"));
    }

    public void LoadStation(int stationId, string name, string city)
    {
        if (StationCombo.ItemsSource is IEnumerable<StationOption> list)
        {
            foreach (var o in list)
            {
                if (o.Station.Id != stationId) continue;
                StationCombo.SelectedItem = o;
                break;
            }
        }
    }

    public void SetArrival(TimeOnly time) => ArrivalTimePicker.SelectedTime = time.ToTimeSpan();
    public void SetDeparture(TimeOnly time) => DepartureTimePicker.SelectedTime = time.ToTimeSpan();
    public void SetDistance(float dist) => DistanceBox.Text = dist.ToString(CultureInfo.InvariantCulture);
    public void SetDayOffset(int day) => DayOffsetBox.Text = day.ToString();
}
