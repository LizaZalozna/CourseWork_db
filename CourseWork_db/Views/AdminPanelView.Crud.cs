using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Interactivity;
using CourseWork_db.Models;

namespace CourseWork_db.Views;

public partial class AdminPanelView
{
    private async void OnAddTrain(object? sender, RoutedEventArgs e)
    {
        TrainMsg.IsVisible = false;
        var (ok, err) = await _trainsSvc.AddAsync(TrainNameBox.Text ?? "");
        ShowMsg(TrainMsg, ok, ok ? "Поїзд додано." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingTrainBox);
            TrainNameBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnUpdateTrain(object? sender, RoutedEventArgs e)
    {
        TrainMsg.IsVisible = false;
        var id = SelectedId(ExistingTrainBox);
        if (id is null) { ShowMsg(TrainMsg, false, "Оберіть поїзд."); return; }
        var (ok, err) = await _trainsSvc.UpdateAsync(id.Value, TrainNameBox.Text ?? "");
        ShowMsg(TrainMsg, ok, ok ? "Поїзд оновлено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingTrainBox);
            TrainNameBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnDeleteTrain(object? sender, RoutedEventArgs e)
    {
        TrainMsg.IsVisible = false;
        var id = SelectedId(ExistingTrainBox);
        if (id is null) { ShowMsg(TrainMsg, false, "Оберіть поїзд."); return; }
        var (ok, err) = await _trainsSvc.DeleteAsync(id.Value);
        ShowMsg(TrainMsg, ok, ok ? "Поїзд видалено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingTrainBox);
            TrainNameBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnAddCarType(object? sender, RoutedEventArgs e)
    {
        CarMsg.IsVisible = false;
        var (ok, err) = await _carTypesSvc.AddAsync(CarTypeNameBox.Text ?? "");
        ShowMsg(CarMsg, ok, ok ? "Тип вагона додано." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingCarTypeBox);
            CarTypeNameBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnUpdateCarType(object? sender, RoutedEventArgs e)
    {
        CarMsg.IsVisible = false;
        var id = SelectedId(ExistingCarTypeBox);
        if (id is null) { ShowMsg(CarMsg, false, "Оберіть тип вагона."); return; }
        var (ok, err) = await _carTypesSvc.UpdateAsync(id.Value, CarTypeNameBox.Text ?? "");
        ShowMsg(CarMsg, ok, ok ? "Тип оновлено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingCarTypeBox);
            CarTypeNameBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnDeleteCarType(object? sender, RoutedEventArgs e)
    {
        CarMsg.IsVisible = false;
        var id = SelectedId(ExistingCarTypeBox);
        if (id is null) { ShowMsg(CarMsg, false, "Оберіть тип вагона."); return; }
        var (ok, err) = await _carTypesSvc.DeleteAsync(id.Value);
        ShowMsg(CarMsg, ok, ok ? "Тип видалено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingCarTypeBox);
            CarTypeNameBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnAddStation(object? sender, RoutedEventArgs e)
    {
        StationMsg.IsVisible = false;
        var (ok, err) = await _stationsSvc.AddAsync(
            StationNameBox.Text ?? "", StationCityBox.Text ?? "", StationCountryBox.Text ?? "");
        ShowMsg(StationMsg, ok, ok ? "Станцію додано." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingStationBox);
            StationNameBox.Text = "";
            StationCityBox.Text = "";
            StationCountryBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnUpdateStation(object? sender, RoutedEventArgs e)
    {
        StationMsg.IsVisible = false;
        var id = SelectedId(ExistingStationBox);
        if (id is null) { ShowMsg(StationMsg, false, "Оберіть станцію."); return; }
        var (ok, err) = await _stationsSvc.UpdateAsync(
            id.Value, StationNameBox.Text ?? "", StationCityBox.Text ?? "", StationCountryBox.Text ?? "");
        ShowMsg(StationMsg, ok, ok ? "Станцію оновлено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingStationBox);
            StationNameBox.Text = "";
            StationCityBox.Text = "";
            StationCountryBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnDeleteStation(object? sender, RoutedEventArgs e)
    {
        StationMsg.IsVisible = false;
        var id = SelectedId(ExistingStationBox);
        if (id is null) { ShowMsg(StationMsg, false, "Оберіть станцію."); return; }
        var (ok, err) = await _stationsSvc.DeleteAsync(id.Value);
        ShowMsg(StationMsg, ok, ok ? "Станцію видалено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingStationBox);
            StationNameBox.Text = "";
            StationCityBox.Text = "";
            StationCountryBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnUpdateRoute(object? sender, RoutedEventArgs e)
    {
        RouteMsg.IsVisible = false;
        RouteDetailMsg.IsVisible = false;

        var id = SelectedId(ExistingRouteBox);
        if (id is null) { ShowMsg(RouteMsg, false, "Оберіть маршрут."); return; }

        var routeName = (RouteNameBox.Text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(routeName))
        {
            ShowMsg(RouteMsg, false, "Введіть назву маршруту.");
            return;
        }

        if (_cards.Count < 2)
        {
            ShowMsg(RouteMsg, false, "Додайте мінімум 2 станції.");
            return;
        }

        for (var i = 0; i < _cards.Count; i++)
        {
            var c = _cards[i];
            if (c.StationId is null)
            {
                ShowMsg(RouteMsg, false, $"Станція #{i + 1}: оберіть станцію.");
                return;
            }

            if (i > 0 && !c.ArrivalTime.HasValue)
            {
                ShowMsg(RouteMsg, false, $"Станція #{i + 1}: введіть час прибуття.");
                return;
            }

            if (i < _cards.Count - 1 && !c.DepartureTime.HasValue)
            {
                ShowMsg(RouteMsg, false, $"Станція #{i + 1}: введіть час відправлення.");
                return;
            }

            if (i < _cards.Count - 1 && c.Distance <= 0)
            {
                ShowMsg(RouteMsg, false, $"Станція #{i + 1}: введіть відстань до наступної.");
                return;
            }

            if (i < _cards.Count - 1 && c.DepartureTime.HasValue && _cards[i + 1].ArrivalTime.HasValue)
            {
                var currDep = new DateTime(1, 1, 1).AddDays(c.DayOffset).Add(c.DepartureTime.Value.ToTimeSpan());
                var nextArr = new DateTime(1, 1, 1).AddDays(_cards[i + 1].DayOffset).Add(_cards[i + 1].ArrivalTime.Value.ToTimeSpan());
                if (nextArr <= currDep)
                {
                    ShowMsg(RouteMsg, false, $"Станція #{i + 1}: прибуття на наступну станцію має бути пізніше за відправлення.");
                    return;
                }
            }
        }

        var stations = new List<(int StationId, int StopOrder, int DayOffset, TimeOnly ArrivalTime, TimeOnly DepartureTime, float Distance)>(_cards.Count);

        for (var i = 0; i < _cards.Count; i++)
        {
            var c = _cards[i];
            var arr = c.ArrivalTime ?? (i == 0 ? c.DepartureTime ?? TimeOnly.MinValue : TimeOnly.MinValue);
            var dep = c.DepartureTime ?? (i == _cards.Count - 1 ? c.ArrivalTime ?? TimeOnly.MinValue : TimeOnly.MinValue);
            stations.Add((c.StationId!.Value, i + 1, c.DayOffset, arr, dep, c.Distance));
        }

        var (ok, err) = await _routesSvc.UpdateRouteFullAsync(id.Value, routeName, stations);
        ShowMsg(RouteMsg, ok, ok ? "Маршрут оновлено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingRouteBox);
            await ReloadLookupsAsync();
            ClearRoute();
        }
    }

    private async void OnDeleteRoute(object? sender, RoutedEventArgs e)
    {
        RouteMsg.IsVisible = false;
        var id = SelectedId(ExistingRouteBox);
        if (id is null) { ShowMsg(RouteMsg, false, "Оберіть маршрут."); return; }
        var (ok, err) = await _routesSvc.DeleteAsync(id.Value);
        ShowMsg(RouteMsg, ok, ok ? "Маршрут видалено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingRouteBox);
            ClearRoute();
            await ReloadLookupsAsync();
        }
    }

    private async void OnAddCar(object? sender, RoutedEventArgs e)
    {
        CarMsg.IsVisible = false;
        var trainId = SelectedId(CarTrainBox);
        var carTypeId = SelectedId(CarTypeBox);
        if (trainId is null || carTypeId is null)
        {
            ShowMsg(CarMsg, false, "Оберіть поїзд і тип вагона.");
            return;
        }

        if (!int.TryParse(SeatsCountBox.Text, out var seats) || seats <= 0)
        {
            ShowMsg(CarMsg, false, "Введіть кількість місць.");
            return;
        }

        var (ok, err) = await _carsSvc.AddAsync(seats, trainId.Value, carTypeId.Value);
        ShowMsg(CarMsg, ok, ok ? "Вагон і місця створено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingCarBox);
            ClearExistingSelection(CarTrainBox);
            ClearExistingSelection(CarTypeBox);
            SeatsCountBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnUpdateCar(object? sender, RoutedEventArgs e)
    {
        CarMsg.IsVisible = false;
        var id = SelectedId(ExistingCarBox);
        var trainId = SelectedId(CarTrainBox);
        var carTypeId = SelectedId(CarTypeBox);
        if (id is null || trainId is null || carTypeId is null)
        {
            ShowMsg(CarMsg, false, "Оберіть вагон, поїзд і тип.");
            return;
        }

        if (!int.TryParse(SeatsCountBox.Text, out var seats) || seats <= 0)
        {
            ShowMsg(CarMsg, false, "Введіть кількість місць.");
            return;
        }

        var (ok, err) = await _carsSvc.UpdateAsync(id.Value, seats, trainId.Value, carTypeId.Value);
        ShowMsg(CarMsg, ok, ok ? "Вагон оновлено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingCarBox);
            ClearExistingSelection(CarTrainBox);
            ClearExistingSelection(CarTypeBox);
            SeatsCountBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnDeleteCar(object? sender, RoutedEventArgs e)
    {
        CarMsg.IsVisible = false;
        var id = SelectedId(ExistingCarBox);
        if (id is null) { ShowMsg(CarMsg, false, "Оберіть вагон."); return; }
        var (ok, err) = await _carsSvc.DeleteAsync(id.Value);
        ShowMsg(CarMsg, ok, ok ? "Вагон видалено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingCarBox);
            ClearExistingSelection(CarTrainBox);
            ClearExistingSelection(CarTypeBox);
            SeatsCountBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnAddTrip(object? sender, RoutedEventArgs e)
    {
        TripMsg.IsVisible = false;
        var routeId = SelectedId(TripRouteBox);
        var trainId = SelectedId(TripTrainBox);
        if (routeId is null || trainId is null)
        {
            ShowMsg(TripMsg, false, "Оберіть маршрут і поїзд.");
            return;
        }

        var dep = DateOnly.FromDateTime(DepartureDatePicker.SelectedDate?.DateTime ?? DateTime.Today);
        var arr = DateOnly.FromDateTime(ArrivalDatePicker.SelectedDate?.DateTime ?? DateTime.Today);

        var (ok, err) = await _tripsSvc.AddAsync(routeId.Value, trainId.Value, dep, arr);
        ShowMsg(TripMsg, ok, ok ? "Рейс додано." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingTripBox);
            ClearExistingSelection(TripRouteBox);
            ClearExistingSelection(TripTrainBox);
            DepartureDatePicker.SelectedDate = DateTimeOffset.Now;
            ArrivalDatePicker.SelectedDate = DateTimeOffset.Now;
            await ReloadLookupsAsync();
        }
    }

    private async void OnUpdateTrip(object? sender, RoutedEventArgs e)
    {
        TripMsg.IsVisible = false;
        var id = SelectedId(ExistingTripBox);
        var routeId = SelectedId(TripRouteBox);
        var trainId = SelectedId(TripTrainBox);
        if (id is null || routeId is null || trainId is null)
        {
            ShowMsg(TripMsg, false, "Оберіть рейс, маршрут і поїзд.");
            return;
        }

        var dep = DateOnly.FromDateTime(DepartureDatePicker.SelectedDate?.DateTime ?? DateTime.Today);
        var arr = DateOnly.FromDateTime(ArrivalDatePicker.SelectedDate?.DateTime ?? DateTime.Today);

        var (ok, err) = await _tripsSvc.UpdateAsync(id.Value, routeId.Value, trainId.Value, dep, arr);
        ShowMsg(TripMsg, ok, ok ? "Рейс оновлено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingTripBox);
            ClearExistingSelection(TripRouteBox);
            ClearExistingSelection(TripTrainBox);
            DepartureDatePicker.SelectedDate = DateTimeOffset.Now;
            ArrivalDatePicker.SelectedDate = DateTimeOffset.Now;
            await ReloadLookupsAsync();
        }
    }

    private async void OnDeleteTrip(object? sender, RoutedEventArgs e)
    {
        TripMsg.IsVisible = false;
        var id = SelectedId(ExistingTripBox);
        if (id is null) { ShowMsg(TripMsg, false, "Оберіть рейс."); return; }
        var (ok, err) = await _tripsSvc.DeleteAsync(id.Value);
        ShowMsg(TripMsg, ok, ok ? "Рейс видалено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingTripBox);
            ClearExistingSelection(TripRouteBox);
            ClearExistingSelection(TripTrainBox);
            DepartureDatePicker.SelectedDate = DateTimeOffset.Now;
            ArrivalDatePicker.SelectedDate = DateTimeOffset.Now;
            await ReloadLookupsAsync();
        }
    }

    private async void OnAddTariff(object? sender, RoutedEventArgs e)
    {
        TariffMsg.IsVisible = false;
        var carTypeId = SelectedId(TariffCarTypeBox);
        if (carTypeId is null) { ShowMsg(TariffMsg, false, "Оберіть тип вагона."); return; }

        if (!float.TryParse(TariffPriceBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var price) &&
            !float.TryParse(TariffPriceBox.Text, out price))
        {
            ShowMsg(TariffMsg, false, "Введіть коректну ціну.");
            return;
        }

        var (ok, err) = await _tariffsSvc.AddAsync(carTypeId.Value, price);
        ShowMsg(TariffMsg, ok, ok ? "Тариф додано." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingTariffBox);
            ClearExistingSelection(TariffCarTypeBox);
            TariffPriceBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnUpdateTariff(object? sender, RoutedEventArgs e)
    {
        TariffMsg.IsVisible = false;
        var id = SelectedId(ExistingTariffBox);
        var carTypeId = SelectedId(TariffCarTypeBox);
        if (id is null || carTypeId is null)
        {
            ShowMsg(TariffMsg, false, "Оберіть тариф і тип вагона.");
            return;
        }

        if (!float.TryParse(TariffPriceBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var price) &&
            !float.TryParse(TariffPriceBox.Text, out price))
        {
            ShowMsg(TariffMsg, false, "Введіть коректну ціну.");
            return;
        }

        var (ok, err) = await _tariffsSvc.UpdateAsync(id.Value, carTypeId.Value, price);
        ShowMsg(TariffMsg, ok, ok ? "Тариф оновлено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingTariffBox);
            ClearExistingSelection(TariffCarTypeBox);
            TariffPriceBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnDeleteTariff(object? sender, RoutedEventArgs e)
    {
        TariffMsg.IsVisible = false;
        var id = SelectedId(ExistingTariffBox);
        if (id is null) { ShowMsg(TariffMsg, false, "Оберіть тариф."); return; }
        var (ok, err) = await _tariffsSvc.DeleteAsync(id.Value);
        ShowMsg(TariffMsg, ok, ok ? "Тариф видалено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingTariffBox);
            ClearExistingSelection(TariffCarTypeBox);
            TariffPriceBox.Text = "";
            await ReloadLookupsAsync();
        }
    }
}
