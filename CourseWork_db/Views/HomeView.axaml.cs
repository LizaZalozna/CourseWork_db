using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork_db.DisplayInfo;
using CourseWork_db.Helpers;
using CourseWork_db.Models;
using CourseWork_db.Services;


namespace CourseWork_db.Views;

public partial class HomeView : UserControl
{
    private readonly TicketService _ticketService = new();

    private User? _user;
    private TripDisplayInfo? _selectedTrip;
    private TicketDisplayInfo? _selectedTicket;
    private List<StationOption> _stationOptions = new();
    private List<SeatDisplayInfo> _allSeats = new();
    private bool _suppressTripSelection;

    public HomeView()
    {
        InitializeComponent();
        TripDatePicker.SelectedDate = DateTimeOffset.Now;
        TripDatePicker.MinYear = DateTimeOffset.Now;
    }
    
    public async Task InitializeForUserAsync(User user)
    {
        _user = user;

        try
        {
            var stations = await _ticketService.GetStationsAsync();
            var tickets = await _ticketService.GetMyTicketsAsync(user.Id);
            _stationOptions = stations.Select(s => new StationOption(s)).ToList();

            await UiDispatcher.RunAsync(() =>
            {
                FromStationCombo.ItemsSource = _stationOptions;
                ToStationCombo.ItemsSource = _stationOptions;

                MyTicketsList.ItemsSource = tickets;
                MyTicketsStatus.Text = tickets.Count == 0
                    ? "У вас ще немає квитків"
                    : $"Всього квитків: {tickets.Count}";

                TripSearchStatus.Text = stations.Count == 0
                    ? "Немає станцій. Зверніться до адміністратора."
                    : "";
            });
        }
        catch (Exception ex)
        {
            await UiDispatcher.RunAsync(() =>
                ViewStatusHelper.Set(TripSearchStatus, $"Помилка завантаження: {ex.Message}", true));
        }
    }

    private static Station? GetSelectedStation(AutoCompleteBox combo) =>
        (combo.SelectedItem as StationOption)?.Station;

    private async void OnSearchTripsClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var from = GetSelectedStation(FromStationCombo);
            var to = GetSelectedStation(ToStationCombo);
            var date = TripDatePicker.SelectedDate?.DateTime ?? DateTime.Today;

            if (from == null || to == null)
            {
                ViewStatusHelper.Set(TripSearchStatus, "Оберіть станції відправлення та прибуття", true);
                return;
            }

            if (from.Id == to.Id)
            {
                ViewStatusHelper.Set(TripSearchStatus, "Станції мають відрізнятися", true);
                return;
            }

            var trips = await _ticketService.FindTripsAsync(from.Id, to.Id, date);

            await UiDispatcher.RunAsync(() =>
            {
                _suppressTripSelection = true;
                TripsList.ItemsSource = trips;
                TripsList.SelectedItem = null;
                _suppressTripSelection = false;

                SeatsList.ItemsSource = null;
                _allSeats.Clear();
                BuyTicketBtn.IsEnabled = false;
                SelectedPriceText.Text = "";
                ResetFilters();
                ViewStatusHelper.Set(TripSearchStatus,
                    trips.Count == 0 ? "Рейсів не знайдено на обрану дату" : $"Знайдено рейсів: {trips.Count}");
            });
        }
        catch (Exception ex)
        {
            await UiDispatcher.RunAsync(() =>
                ViewStatusHelper.Set(TripSearchStatus, ex.Message, true));
        }
    }

    private async void OnTripSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (_suppressTripSelection) return;
        if (e.AddedItems.Count == 0) return;

        _selectedTrip = TripsList.SelectedItem as TripDisplayInfo;
        BuyTicketBtn.IsEnabled = false;
        SelectedPriceText.Text = "";

        if (_selectedTrip == null)
        {
            SeatsList.ItemsSource = null;
            _allSeats.Clear();
            return;
        }

        try
        {
            var from = GetSelectedStation(FromStationCombo);
            var to = GetSelectedStation(ToStationCombo);
            if (from == null || to == null) return;

            var (seats, debug) = await _ticketService.GetFreeSeatsAsync(
                _selectedTrip.TripId,
                from.Id,
                to.Id);

            _allSeats = seats;

            await UiDispatcher.RunAsync(() =>
            {
                PopulateCarTypeFilter();
                ApplyFilters();
                ViewStatusHelper.Set(TripSearchStatus, debug, seats.Count == 0);
            });
        }
        catch (Exception ex)
        {
            await UiDispatcher.RunAsync(() =>
                ViewStatusHelper.Set(TripSearchStatus, ex.Message, true));
        }
    }

    private void OnSeatSelected(object? sender, SelectionChangedEventArgs e)
    {
        var selected = SeatsList.SelectedItems
            .OfType<SeatDisplayInfo>()
            .ToList();

        if (selected.Count == 0)
        {
            BuyTicketBtn.IsEnabled = false;
            SelectedPriceText.Text = "Оберіть місця";
            return;
        }

        var total = selected.Sum(s => s.Price);
        SelectedPriceText.Text = selected.Count == 1
            ? $"Ціна: {total:F2} грн"
            : $"Обрано {selected.Count} місць на {total:F2} грн";
        BuyTicketBtn.IsEnabled = true;
    }

    private async void OnBuyTicketClick(object? sender, RoutedEventArgs e)
    {
        if (_user == null || _selectedTrip == null) return;

        var selected = SeatsList.SelectedItems
            .OfType<SeatDisplayInfo>()
            .ToList();

        if (selected.Count == 0) return;

        var bought = 0;
        var errors = new List<string>();

        var from = GetSelectedStation(FromStationCombo);
        var to = GetSelectedStation(ToStationCombo);
        if (from == null || to == null) return;

        foreach (var seat in selected)
        {
            var (ok, error) = await _ticketService.BuyTicketAsync(
                _user.Id,
                _selectedTrip.TripId,
                seat.SeatId,
                from.Id,
                to.Id);

            if (ok)
                bought++;
            else
                errors.Add($"Місце №{seat.SeatNumber} (вагон {seat.CarNumber}): {error}");
        }

        await UiDispatcher.RunAsync(() =>
        {
            var msg = bought > 0
                ? $"Придбано квитків: {bought}"
                : "Не вдалося придбати квитки";
            if (errors.Count > 0)
                msg += "\n" + string.Join("\n", errors);
            ViewStatusHelper.Set(BuyStatus, msg, errors.Count > 0 && bought == 0);
        });

        var (retrySeats, retryDebug) = await _ticketService.GetFreeSeatsAsync(
            _selectedTrip.TripId, from.Id, to.Id);
        _allSeats = retrySeats;

        await UiDispatcher.RunAsync(() =>
        {
            PopulateCarTypeFilter();
            ApplyFilters();
            SeatsList.SelectedItems.Clear();
            BuyTicketBtn.IsEnabled = false;
            SelectedPriceText.Text = "Оберіть місця";
            ViewStatusHelper.Set(TripSearchStatus, retryDebug, retrySeats.Count == 0);
        });

        await LoadMyTicketsAsync();
    }

    private async void OnRefreshTicketsClick(object? sender, RoutedEventArgs e) =>
        await LoadMyTicketsAsync();

    private async Task LoadMyTicketsAsync()
    {
        if (_user == null) return;

        try
        {
            var tickets = await _ticketService.GetMyTicketsAsync(_user.Id);

            await UiDispatcher.RunAsync(() =>
            {
                MyTicketsList.ItemsSource = tickets;
                MyTicketsStatus.Text = tickets.Count == 0
                    ? "У вас ще немає квитків"
                    : $"Всього квитків: {tickets.Count}";
                RouteInfoPanel.IsVisible = false;
            });
        }
        catch (Exception ex)
        {
            await UiDispatcher.RunAsync(() =>
                MyTicketsStatus.Text = $"Помилка: {ex.Message}");
        }
    }

    private async void OnMyTicketSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0) return;

        _selectedTicket = MyTicketsList.SelectedItem as TicketDisplayInfo;
        if (_selectedTicket == null)
        {
            RouteInfoPanel.IsVisible = false;
            return;
        }

        try
        {
            var routeStations = await _ticketService.GetRouteStationsAsync(
                _selectedTicket.TripId);

            await UiDispatcher.RunAsync(() =>
            {
                RouteInfoHeader.Text = $"{_selectedTicket.RouteName} | {_selectedTicket.TrainName} | {_selectedTicket.DepartureDate:dd.MM.yyyy}";
                RouteStationsList.ItemsSource = routeStations.Count > 0 ? routeStations : null;
                RouteStationsStatus.IsVisible = routeStations.Count == 0;
                RouteStationsStatus.Text = routeStations.Count == 0 ? "Немає даних про маршрут" : "";
                RouteInfoPanel.IsVisible = routeStations.Count > 0;
            });
        }
        catch (Exception ex)
        {
            await UiDispatcher.RunAsync(() =>
            {
                MyTicketsStatus.Text = $"Помилка завантаження маршруту: {ex.Message}";
                RouteInfoPanel.IsVisible = false;
            });
        }
    }

    private void PopulateCarTypeFilter()
    {
        var types = _allSeats
            .Select(s => s.CarTypeName)
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        var items = new List<string> { "Всі типи" };
        items.AddRange(types);

        SeatCarTypeFilter.ItemsSource = items;
        SeatCarTypeFilter.SelectedItem = "Всі типи";
    }

    private void ResetFilters()
    {
        SeatCarTypeFilter.ItemsSource = null;
        SeatCarTypeFilter.Text = "";
        CharFilterBox.Text = "";
        PriceFromBox.Text = "";
        PriceToBox.Text = "";
    }

    private void OnSeatFilterChanged(object? sender, RoutedEventArgs e)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allSeats.AsEnumerable();

        var carType = SeatCarTypeFilter.SelectedItem as string;
        if (!string.IsNullOrEmpty(carType) && carType != "Всі типи")
            filtered = filtered.Where(s => s.CarTypeName == carType);

        var charText = CharFilterBox.Text?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(charText))
            filtered = filtered.Where(s =>
                s.Characteristics.Contains(charText, StringComparison.OrdinalIgnoreCase));

        if (float.TryParse(PriceFromBox.Text, out var priceFrom))
            filtered = filtered.Where(s => s.Price >= priceFrom);

        if (float.TryParse(PriceToBox.Text, out var priceTo))
            filtered = filtered.Where(s => s.Price <= priceTo);

        SeatsList.ItemsSource = filtered.OrderBy(s => s.Price).ToList();
    }

    private void OnResetFiltersClick(object? sender, RoutedEventArgs e)
    {
        SeatCarTypeFilter.SelectedItem = "Всі типи";
        CharFilterBox.Text = "";
        PriceFromBox.Text = "";
        PriceToBox.Text = "";
    }

    public void ClearForm()
    {
        _selectedTrip = null;
        _allSeats.Clear();
        FromStationCombo.SelectedItem = null;
        FromStationCombo.Text = "";
        ToStationCombo.SelectedItem = null;
        ToStationCombo.Text = "";
        TripDatePicker.SelectedDate = DateTimeOffset.Now;
        TripsList.ItemsSource = null;
        SeatsList.ItemsSource = null;
        SeatCarTypeFilter.ItemsSource = null;
        SeatCarTypeFilter.Text = "";
        BuyTicketBtn.IsEnabled = false;
        SelectedPriceText.Text = "Оберіть місця";
        TripSearchStatus.Text = "";
        BuyStatus.Text = "";
        MyTicketsList.ItemsSource = null;
        MyTicketsStatus.Text = "";
        RouteInfoPanel.IsVisible = false;
        ResetFilters();
    }
}
