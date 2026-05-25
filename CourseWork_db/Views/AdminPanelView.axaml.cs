using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using CourseWork_db.Controls;
using CourseWork_db.Helpers;
using CourseWork_db.Models;
using CourseWork_db.Services;

namespace CourseWork_db.Views;

public partial class AdminPanelView : UserControl
{
    internal readonly TrainService _trainsSvc = new();
    internal readonly CarTypeService _carTypesSvc = new();
    internal readonly RouteService _routesSvc = new();
    internal readonly StationService _stationsSvc = new();
    internal readonly CarService _carsSvc = new();
    internal readonly TripService _tripsSvc = new();
    internal readonly TariffService _tariffsSvc = new();

    internal List<Train> _trains = new();
    internal List<CarType> _carTypes = new();
    internal List<Route> _routes = new();
    internal List<Station> _stations = new();
    internal List<Car> _cars = new();
    internal List<Trip> _trips = new();
    internal List<Tariff> _tariffs = new();

    internal readonly List<RouteStationCard> _cards = new();
    internal RouteStationCard? _selectedCard;

    private readonly SemaphoreSlim _reloadGate = new(1, 1);
    private bool _suppressSelectionHandlers;
    private bool _isReloading;
    private bool _isFillingFromExistingTrip;

    public event Action? ExitRequested;

    public AdminPanelView()
    {
        InitializeComponent();
        
        AddTrainButton.Click += OnAddTrain;
        UpdateTrainButton.Click += OnUpdateTrain;
        DeleteTrainButton.Click += OnDeleteTrain;
        AddCarTypeButton.Click += OnAddCarType;
        UpdateCarTypeButton.Click += OnUpdateCarType;
        DeleteCarTypeButton.Click += OnDeleteCarType;
        UpdateRouteButton.Click += OnUpdateRoute;
        DeleteRouteButton.Click += OnDeleteRoute;
        LoadRouteButton.Click += OnLoadRoute;
        AddStationButton.Click += OnAddStation;
        UpdateStationButton.Click += OnUpdateStation;
        DeleteStationButton.Click += OnDeleteStation;
        AddCarButton.Click += OnAddCar;
        UpdateCarButton.Click += OnUpdateCar;
        DeleteCarButton.Click += OnDeleteCar;
        AddTariffButton.Click += OnAddTariff;
        UpdateTariffButton.Click += OnUpdateTariff;
        DeleteTariffButton.Click += OnDeleteTariff;
        AddTripButton.Click += OnAddTrip;
        UpdateTripButton.Click += OnUpdateTrip;
        DeleteTripButton.Click += OnDeleteTrip;

        SaveRouteBtn.Click += OnSaveRoute;
        ClearRouteBtn.Click += OnClearRoute;
        AddStationCardBtn.Click += OnAddStationCard;
        RemoveStationCardBtn.Click += OnRemoveStationCard;
        MoveUpBtn.Click += OnMoveCardUp;
        MoveDownBtn.Click += OnMoveCardDown;

        ExistingTrainBox.SelectionChanged += (_, _) => FillTrainFromSelected();
        ExistingCarTypeBox.SelectionChanged += (_, _) => FillCarTypeFromSelected();
        ExistingStationBox.SelectionChanged += (_, _) => FillStationFromSelected();
        ExistingCarBox.SelectionChanged += (_, _) => FillCarFromSelected();
        ExistingTripBox.SelectionChanged += (_, _) =>
        {
            _isFillingFromExistingTrip = true;
            FillTripFromSelected();
            _isFillingFromExistingTrip = false;
        };
        ExistingTariffBox.SelectionChanged += (_, _) => FillTariffFromSelected();

        DepartureDatePicker.SelectedDate = DateTimeOffset.Now;
        DepartureDatePicker.MinYear = DateTimeOffset.Now;
        ArrivalDatePicker.SelectedDate = DateTimeOffset.Now;
        ArrivalDatePicker.MinYear = DateTimeOffset.Now;

        TripRouteBox.SelectionChanged += async (_, _) => await UpdateArrivalDateAsync();
        DepartureDatePicker.SelectedDateChanged += async (_, _) => await UpdateArrivalDateAsync();
        

        Loaded += async (_, _) => await ReloadLookupsAsync();
    }
    internal static void ShowMsg(TextBlock block, bool ok, string text)
    {
        block.Text = text;
        block.Foreground = ok
            ? new SolidColorBrush(Color.Parse("#66BB6A"))
            : new SolidColorBrush(Color.Parse("#EF5350"));
        block.IsVisible = true;
    }

    internal async Task ReloadLookupsAsync()
    {
        if (_isReloading) return;

        await _reloadGate.WaitAsync();
        _isReloading = true;
        _suppressSelectionHandlers = true;

        try
        {
            _trains = await _trainsSvc.GetAllAsync();
            _carTypes = await _carTypesSvc.GetAllAsync();
            _routes = await _routesSvc.GetAllAsync();
            _stations = await _stationsSvc.GetAllAsync();
            _cars = await _carsSvc.GetAllAsync();
            _trips = await _tripsSvc.GetAllAsync();
            _tariffs = await _tariffsSvc.GetAllAsync();

            var trainsItems = _trains.Select(t => $"{t.Id}: {t.Name}").ToList();
            CarTrainBox.ItemsSource = trainsItems;
            TripTrainBox.ItemsSource = trainsItems;
            ExistingTrainBox.ItemsSource = trainsItems;

            var carTypeItems = _carTypes.Select(t => $"{t.Id}: {t.Name}").ToList();
            CarTypeBox.ItemsSource = carTypeItems;
            TariffCarTypeBox.ItemsSource = carTypeItems;
            ExistingCarTypeBox.ItemsSource = carTypeItems;

            var routeItems = _routes.Select(r => $"{r.Id}: {r.Name}").ToList();
            TripRouteBox.ItemsSource = routeItems;
            ExistingRouteBox.ItemsSource = routeItems;

            var stationItems = _stations.Select(s => $"{s.Id}: {s.Name} ({s.City})").ToList();
            ExistingStationBox.ItemsSource = stationItems;

            ExistingCarBox.ItemsSource = _cars
                .Select(c => $"{c.Id}: поїзд #{c.TrainId} / тип #{c.CarTypeId} / {c.SeatsCount} місць")
                .ToList();

            ExistingTripBox.ItemsSource = _trips
                .Select(t => $"{t.Id}: {t.Route?.Name} / {t.Train?.Name}")
                .ToList();

            ExistingTariffBox.ItemsSource = _tariffs
                .Select(t => $"{t.Id}: {t.CarType?.Name} = {t.PricePerKm}")
                .ToList();

            foreach (var card in _cards)
                card.SetStations(_stations);
        }
        catch
        {
        }
        finally
        {
            _suppressSelectionHandlers = false;
            _isReloading = false;
            _reloadGate.Release();
        }
    }

    internal static int? SelectedId(AutoCompleteBox box)
    {
        if (box.SelectedItem is not string s) return null;
        var idx = s.IndexOf(':');
        if (idx <= 0) return null;
        return int.TryParse(s[..idx], out var id) ? id : null;
    }

    internal static void ClearExistingSelection(AutoCompleteBox box)
    {
        box.SelectedItem = null;
        box.Text = "";
    }

    internal static void SelectById(AutoCompleteBox box, int id)
    {
        if (box.ItemsSource is not IEnumerable<string> items) return;
        box.SelectedItem = items.FirstOrDefault(x => x.StartsWith($"{id}:"));
    }

    internal void FillTrainFromSelected()
    {
        if (_suppressSelectionHandlers) return;

        var id = SelectedId(ExistingTrainBox);
        var e = _trains.FirstOrDefault(x => x.Id == id);
        if (e is null) return;
        TrainNameBox.Text = e.Name;
    }

    internal void FillCarTypeFromSelected()
    {
        if (_suppressSelectionHandlers) return;

        var id = SelectedId(ExistingCarTypeBox);
        var e = _carTypes.FirstOrDefault(x => x.Id == id);
        if (e is null) return;
        CarTypeNameBox.Text = e.Name;
    }

    internal void FillStationFromSelected()
    {
        if (_suppressSelectionHandlers) return;

        var id = SelectedId(ExistingStationBox);
        var e = _stations.FirstOrDefault(x => x.Id == id);
        if (e is null) return;
        StationNameBox.Text = e.Name;
        StationCityBox.Text = e.City;
        StationCountryBox.Text = e.Country;
    }

    internal void FillCarFromSelected()
    {
        if (_suppressSelectionHandlers) return;

        var id = SelectedId(ExistingCarBox);
        var e = _cars.FirstOrDefault(x => x.Id == id);
        if (e is null) return;
        SeatsCountBox.Text = e.SeatsCount.ToString();
        SelectById(CarTrainBox, e.TrainId);
        SelectById(CarTypeBox, e.CarTypeId);
    }

    internal void FillTripFromSelected()
    {
        if (_suppressSelectionHandlers) return;

        var id = SelectedId(ExistingTripBox);
        var e = _trips.FirstOrDefault(x => x.Id == id);
        if (e is null) return;
        SelectById(TripRouteBox, e.RouteId);
        SelectById(TripTrainBox, e.TrainId);
        DepartureDatePicker.SelectedDate = e.DepartureDate.ToDateTime(TimeOnly.MinValue);
        ArrivalDatePicker.SelectedDate = e.ArrivalDate.ToDateTime(TimeOnly.MinValue);
    }

    internal void FillTariffFromSelected()
    {
        if (_suppressSelectionHandlers) return;

        var id = SelectedId(ExistingTariffBox);
        var e = _tariffs.FirstOrDefault(x => x.Id == id);
        if (e is null) return;
        SelectById(TariffCarTypeBox, e.CarTypeId);
        TariffPriceBox.Text = e.PricePerKm.ToString();
    }

    private async Task UpdateArrivalDateAsync()
    {
        if (_suppressSelectionHandlers || _isFillingFromExistingTrip) return;

        var routeId = SelectedId(TripRouteBox);
        var depDate = DepartureDatePicker.SelectedDate?.DateTime;

        if (routeId == null || depDate == null) return;

        try
        {
            var stations = await _routesSvc.GetStationsForRouteAsync(routeId.Value);
            var maxDayOffset = stations.Count > 0 ? stations.Max(s => s.DayOffset) : 0;

            var depDateOnly = DateOnly.FromDateTime(depDate.Value);
            var arrivalDateOnly = depDateOnly.AddDays(maxDayOffset);
            ArrivalDatePicker.SelectedDate = arrivalDateOnly.ToDateTime(TimeOnly.MinValue);
        }
        catch
        {
            // ignore errors during auto-calculation
        }
    }

    internal void AddCardToPanel(RouteStationCard card)
    {
        card.SetStations(_stations);
        card.Changed += RecalculateTotals;

        var idx = card == _selectedCard ? _cards.IndexOf(_selectedCard!) + 1 : _cards.Count;
        _cards.Insert(idx, card);
        RebuildPanel();

        card.SelectRequested += OnCardSelectRequested;
        SelectCard(card);
    }

    private void OnCardSelectRequested(RouteStationCard card) => SelectCard(card);

    internal void SelectCard(RouteStationCard card)
    {
        if (_selectedCard != null) _selectedCard.SelectBorder(false);
        _selectedCard = card;
        _selectedCard.SelectBorder(true);
    }

    internal void RebuildPanel()
    {
        RouteStationsCardsPanel.Children.Clear();
        for (var i = 0; i < _cards.Count; i++)
        {
            _cards[i].UpdateNumber(i + 1);
            _cards[i].SetMode(i == 0, i == _cards.Count - 1);
            RouteStationsCardsPanel.Children.Add(_cards[i]);
        }
        RecalculateTotals();
    }

    internal void RecalculateTotals()
    {
        var totalDist = _cards.Sum(c => c.Distance);

        int? firstDepDay = null;
        TimeOnly? firstDep = null;
        int? lastArrDay = null;
        TimeOnly? lastArr = null;

        foreach (var card in _cards)
        {
            if (card.DepartureTime.HasValue && firstDep is null)
            {
                firstDep = card.DepartureTime;
                firstDepDay = card.DayOffset;
            }
            if (card.ArrivalTime.HasValue)
            {
                lastArr = card.ArrivalTime;
                lastArrDay = card.DayOffset;
            }
        }

        var duration = "0 год 0 хв";
        if (firstDep.HasValue && lastArr.HasValue && firstDepDay.HasValue && lastArrDay.HasValue)
        {
            var start = new DateTime(1, 1, 1).AddDays(firstDepDay.Value).Add(firstDep.Value.ToTimeSpan());
            var end   = new DateTime(1, 1, 1).AddDays(lastArrDay.Value).Add(lastArr.Value.ToTimeSpan());
            var span = end - start;

            if (span.TotalSeconds < 0)
                span = TimeSpan.Zero;

            duration = span.Days > 0
                ? $"{span.Days} дн {span.Hours} год {span.Minutes} хв"
                : $"{(int)span.TotalHours} год {span.Minutes} хв";
        }

        TotalDistanceText.Text = $"{totalDist:0.0} км";
        TotalDurationText.Text = duration;
    }

    private void OnAddStationCard(object? sender, RoutedEventArgs e)
    {
        RouteDetailMsg.IsVisible = false;
        AddCardToPanel(new RouteStationCard());
    }

    private void OnRemoveStationCard(object? sender, RoutedEventArgs e)
    {
        RouteDetailMsg.IsVisible = false;
        if (_selectedCard is null || _cards.Count == 0)
        {
            ShowMsg(RouteDetailMsg, false, "Оберіть станцію для видалення.");
            return;
        }

        _selectedCard.Changed -= RecalculateTotals;
        _selectedCard.SelectRequested -= OnCardSelectRequested;
        _cards.Remove(_selectedCard);
        _selectedCard = _cards.Count > 0 ? _cards[^1] : null;
        RebuildPanel();
    }

    private void OnMoveCardUp(object? sender, RoutedEventArgs e)
    {
        if (_selectedCard is null) return;
        var idx = _cards.IndexOf(_selectedCard);
        if (idx <= 0) return;

        _cards.RemoveAt(idx);
        _cards.Insert(idx - 1, _selectedCard);
        RebuildPanel();
        SelectCard(_selectedCard);
    }

    private void OnMoveCardDown(object? sender, RoutedEventArgs e)
    {
        if (_selectedCard is null) return;
        var idx = _cards.IndexOf(_selectedCard);
        if (idx >= _cards.Count - 1) return;

        _cards.RemoveAt(idx);
        _cards.Insert(idx + 1, _selectedCard);
        RebuildPanel();
        SelectCard(_selectedCard);
    }

    private async void OnSaveRoute(object? sender, RoutedEventArgs e) => await SaveRouteAsync();

    private void OnClearRoute(object? sender, RoutedEventArgs e) => ClearRoute();

    internal void ClearRoute()
    {
        RouteDetailMsg.IsVisible = false;
        foreach (var card in _cards)
        {
            card.Changed -= RecalculateTotals;
            card.SelectRequested -= OnCardSelectRequested;
        }
        _cards.Clear();
        _selectedCard = null;
        RouteStationsCardsPanel.Children.Clear();
        RouteNameBox.Text = "";
        TotalDistanceText.Text = "0 км";
        TotalDurationText.Text = "0 год 0 хв";
    }

    private void OnLoadRoute(object? sender, RoutedEventArgs e)
    {
        RouteMsg.IsVisible = false;
        var id = SelectedId(ExistingRouteBox);
        if (id is null)
        {
            ShowMsg(RouteMsg, false, "Оберіть маршрут.");
            return;
        }

        var route = _routes.FirstOrDefault(r => r.Id == id);
        if (route is null)
        {
            ShowMsg(RouteMsg, false, "Маршрут не знайдено.");
            return;
        }

        _ = LoadRouteToCardsAsync(id.Value, route.Name);
    }

    internal async System.Threading.Tasks.Task LoadRouteToCardsAsync(int routeId, string routeName)
    {
        ClearRoute();

        try
        {
            var stations = await _routesSvc.GetStationsForRouteAsync(routeId);
            var segments = await _routesSvc.GetSegmentsForRouteAsync(routeId);
            var segmentMap = segments.ToDictionary(s => s.FromStationId, s => s.Distance);

            RouteNameBox.Text = routeName;

            foreach (var s in stations.OrderBy(x => x.StopOrder))
            {
                var card = new RouteStationCard();
                card.SetStations(_stations);
                card.LoadStation(s.StationId, s.Station?.Name ?? "", s.Station?.City ?? "");
                card.Changed += RecalculateTotals;
                card.SetDayOffset(s.DayOffset);
                if (s.StopOrder > 1) card.SetArrival(s.ArrivalTime);
                if (s.StopOrder < stations.Count) card.SetDeparture(s.DepartureTime);
                if (s.StopOrder < stations.Count && segmentMap.TryGetValue(s.StationId, out var dist))
                    card.SetDistance(dist);
                card.SelectRequested += OnCardSelectRequested;
                _cards.Add(card);
            }

            RebuildPanel();
            if (_cards.Count > 0) SelectCard(_cards[0]);
        }
        catch
        {
        }
    }

    internal async System.Threading.Tasks.Task SaveRouteAsync()
    {
        RouteDetailMsg.IsVisible = false;

        var routeName = (RouteNameBox.Text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(routeName))
        {
            ShowMsg(RouteDetailMsg, false, "Введіть назву маршруту.");
            return;
        }

        if (_cards.Count < 2)
        {
            ShowMsg(RouteDetailMsg, false, "Додайте мінімум 2 станції.");
            return;
        }

        for (var i = 0; i < _cards.Count; i++)
        {
            var c = _cards[i];
            if (c.StationId is null)
            {
                ShowMsg(RouteDetailMsg, false, $"Станція #{i + 1}: оберіть станцію.");
                return;
            }

            if (i > 0 && !c.ArrivalTime.HasValue)
            {
                ShowMsg(RouteDetailMsg, false, $"Станція #{i + 1}: введіть час прибуття.");
                return;
            }

            if (i < _cards.Count - 1 && !c.DepartureTime.HasValue)
            {
                ShowMsg(RouteDetailMsg, false, $"Станція #{i + 1}: введіть час відправлення.");
                return;
            }

            if (i < _cards.Count - 1 && c.Distance <= 0)
            {
                ShowMsg(RouteDetailMsg, false, $"Станція #{i + 1}: введіть відстань до наступної.");
                return;
            }

            if (i < _cards.Count - 1 && c.DepartureTime.HasValue && _cards[i + 1].ArrivalTime.HasValue)
            {
                var currDep = new DateTime(1, 1, 1).AddDays(c.DayOffset).Add(c.DepartureTime.Value.ToTimeSpan());
                var nextArr = new DateTime(1, 1, 1).AddDays(_cards[i + 1].DayOffset).Add(_cards[i + 1].ArrivalTime.Value.ToTimeSpan());
                if (nextArr <= currDep)
                {
                    ShowMsg(RouteDetailMsg, false, $"Станція #{i + 1}: прибуття на наступну станцію має бути пізніше за відправлення.");
                    return;
                }
            }
        }

        var (ok, err, routeId) = await _routesSvc.AddAsync(routeName);
        if (!ok)
        {
            ShowMsg(RouteDetailMsg, false, err);
            return;
        }

        for (var i = 0; i < _cards.Count; i++)
        {
            var c = _cards[i];
            var arr = c.ArrivalTime ?? (i == 0 ? c.DepartureTime ?? TimeOnly.MinValue : TimeOnly.MinValue);
            var dep = c.DepartureTime ?? (i == _cards.Count - 1 ? c.ArrivalTime ?? TimeOnly.MinValue : TimeOnly.MinValue);

            var (sOk, sErr) = await _routesSvc.AddStationAsync(routeId, c.StationId!.Value, i + 1, arr, dep, c.DayOffset);
            if (!sOk)
            {
                ShowMsg(RouteDetailMsg, false, $"Помилка зупинки {i + 1}: {sErr}");
                await _routesSvc.DeleteAsync(routeId);
                return;
            }
        }

        for (var i = 1; i < _cards.Count; i++)
        {
            var prev = _cards[i - 1];
            var curr = _cards[i];
            var (segOk, segErr) = await _routesSvc.AddSegmentAsync(
                routeId, prev.StationId!.Value, curr.StationId!.Value, prev.Distance);
            if (!segOk)
            {
                ShowMsg(RouteDetailMsg, false, $"Помилка сегмента: {segErr}");
                await _routesSvc.DeleteAsync(routeId);
                return;
            }
        }

        ShowMsg(RouteDetailMsg, true,
            $"Маршрут '{routeName}' збережено ({_cards.Count} станцій, {TotalDistanceText.Text}, {TotalDurationText.Text}).");
        ClearRoute();
        ClearExistingSelection(ExistingRouteBox);
        await ReloadLookupsAsync();
    }
}
