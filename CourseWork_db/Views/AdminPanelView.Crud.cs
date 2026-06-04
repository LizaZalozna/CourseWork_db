using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Interactivity;
using CourseWork_db.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork_db.Views;

public partial class AdminPanelView
{
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

    private async void OnAddCarTypeName(object? sender, RoutedEventArgs e)
    {
        CarTypeNameMsg.IsVisible = false;
        var (ok, err) = await _carTypeNamesSvc.AddAsync(CarTypeNameValueBox.Text ?? "");
        ShowMsg(CarTypeNameMsg, ok, ok ? "Назву додано." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingCarTypeNameBox);
            CarTypeNameValueBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnUpdateCarTypeName(object? sender, RoutedEventArgs e)
    {
        CarTypeNameMsg.IsVisible = false;
        var id = SelectedId(ExistingCarTypeNameBox);
        if (id is null) { ShowMsg(CarTypeNameMsg, false, "Оберіть назву."); return; }
        var (ok, err) = await _carTypeNamesSvc.UpdateAsync(id.Value, CarTypeNameValueBox.Text ?? "");
        ShowMsg(CarTypeNameMsg, ok, ok ? "Назву оновлено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingCarTypeNameBox);
            CarTypeNameValueBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnDeleteCarTypeName(object? sender, RoutedEventArgs e)
    {
        CarTypeNameMsg.IsVisible = false;
        var id = SelectedId(ExistingCarTypeNameBox);
        if (id is null) { ShowMsg(CarTypeNameMsg, false, "Оберіть назву."); return; }
        var (ok, err) = await _carTypeNamesSvc.DeleteAsync(id.Value);
        ShowMsg(CarTypeNameMsg, ok, ok ? "Назву видалено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingCarTypeNameBox);
            CarTypeNameValueBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnAddStage(object? sender, RoutedEventArgs e)
    {
        StageMsg.IsVisible = false;
        var (ok, err) = await _stagesSvc.AddAsync(StageNameBox.Text ?? "");
        ShowMsg(StageMsg, ok, ok ? "Етап додано." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingStageBox);
            StageNameBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnUpdateStage(object? sender, RoutedEventArgs e)
    {
        StageMsg.IsVisible = false;
        var id = SelectedId(ExistingStageBox);
        if (id is null) { ShowMsg(StageMsg, false, "Оберіть етап."); return; }
        var (ok, err) = await _stagesSvc.UpdateAsync(id.Value, StageNameBox.Text ?? "");
        ShowMsg(StageMsg, ok, ok ? "Етап оновлено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingStageBox);
            StageNameBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnDeleteStage(object? sender, RoutedEventArgs e)
    {
        StageMsg.IsVisible = false;
        var id = SelectedId(ExistingStageBox);
        if (id is null) { ShowMsg(StageMsg, false, "Оберіть етап."); return; }
        var (ok, err) = await _stagesSvc.DeleteAsync(id.Value);
        ShowMsg(StageMsg, ok, ok ? "Етап видалено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingStageBox);
            StageNameBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnAddCarType(object? sender, RoutedEventArgs e)
    {
        CarTypeMsg.IsVisible = false;
        var carTypeNameId = SelectedId(CarTypeNameBox);
        var modernizationStageId = SelectedId(ModernizationStageBox);
        if (carTypeNameId is null || modernizationStageId is null)
        {
            ShowMsg(CarTypeMsg, false, "Оберіть назву типу та етап модернізації.");
            return;
        }

        if (!float.TryParse(CarTypePriceBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var price) &&
            !float.TryParse(CarTypePriceBox.Text, out price))
        {
            ShowMsg(CarTypeMsg, false, "Введіть коректну ціну за км.");
            return;
        }

        if (!float.TryParse(CarTypeServicePriceBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var servicePrice) &&
            !float.TryParse(CarTypeServicePriceBox.Text, out servicePrice))
        {
            ShowMsg(CarTypeMsg, false, "Введіть коректну вартість обслуговування.");
            return;
        }

        var (ok, err) = await _carTypesSvc.AddAsync(carTypeNameId.Value, modernizationStageId.Value, price, servicePrice);
        ShowMsg(CarTypeMsg, ok, ok ? "Тип вагона додано." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingCarTypeBox);
            ClearExistingSelection(CarTypeNameBox);
            ClearExistingSelection(ModernizationStageBox);
            CarTypePriceBox.Text = "";
            CarTypeServicePriceBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnUpdateCarType(object? sender, RoutedEventArgs e)
    {
        CarTypeMsg.IsVisible = false;
        var id = SelectedId(ExistingCarTypeBox);
        var carTypeNameId = SelectedId(CarTypeNameBox);
        var modernizationStageId = SelectedId(ModernizationStageBox);
        if (id is null || carTypeNameId is null || modernizationStageId is null)
        {
            ShowMsg(CarTypeMsg, false, "Оберіть тип, назву типу та етап модернізації.");
            return;
        }

        if (!float.TryParse(CarTypePriceBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var price) &&
            !float.TryParse(CarTypePriceBox.Text, out price))
        {
            ShowMsg(CarTypeMsg, false, "Введіть коректну ціну за км.");
            return;
        }

        if (!float.TryParse(CarTypeServicePriceBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var servicePrice) &&
            !float.TryParse(CarTypeServicePriceBox.Text, out servicePrice))
        {
            ShowMsg(CarTypeMsg, false, "Введіть коректну вартість обслуговування.");
            return;
        }

        var (ok, err) = await _carTypesSvc.UpdateAsync(id.Value, carTypeNameId.Value, modernizationStageId.Value, price, servicePrice);
        ShowMsg(CarTypeMsg, ok, ok ? "Тип оновлено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingCarTypeBox);
            ClearExistingSelection(CarTypeNameBox);
            ClearExistingSelection(ModernizationStageBox);
            CarTypePriceBox.Text = "";
            CarTypeServicePriceBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnDeleteCarType(object? sender, RoutedEventArgs e)
    {
        CarTypeMsg.IsVisible = false;
        var id = SelectedId(ExistingCarTypeBox);
        if (id is null) { ShowMsg(CarTypeMsg, false, "Оберіть тип вагона."); return; }
        var (ok, err) = await _carTypesSvc.DeleteAsync(id.Value);
        ShowMsg(CarTypeMsg, ok, ok ? "Тип видалено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingCarTypeBox);
            ClearExistingSelection(CarTypeNameBox);
            ClearExistingSelection(ModernizationStageBox);
            CarTypePriceBox.Text = "";
            CarTypeServicePriceBox.Text = "";
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
        ShowMsg(CarMsg, ok, ok ? "Вагон створено." : err);
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

            if (i < _cards.Count - 1 && c.DepartureTime.HasValue && _cards[i + 1].ArrivalTime.HasValue)
            {
                var currDep = new DateTime(1, 1, 1).AddDays(c.DayOffset).Add(c.DepartureTime.Value.ToTimeSpan());
                var nextArr = new DateTime(1, 1, 1).AddDays(c.DayOffset).Add(_cards[i + 1].ArrivalTime.Value.ToTimeSpan());
                if (nextArr <= currDep) nextArr = nextArr.AddDays(1);
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
            ClearRoute();
            await ReloadLookupsAsync();
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

    private async void OnAddCharType(object? sender, RoutedEventArgs e)
    {
        CharTypeMsg.IsVisible = false;
        var (ok, err) = await _charTypesSvc.AddAsync(CharTypeNameBox.Text ?? "");
        ShowMsg(CharTypeMsg, ok, ok ? "Тип додано." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingCharTypeBox);
            CharTypeNameBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnUpdateCharType(object? sender, RoutedEventArgs e)
    {
        CharTypeMsg.IsVisible = false;
        var id = SelectedId(ExistingCharTypeBox);
        if (id is null) { ShowMsg(CharTypeMsg, false, "Оберіть тип."); return; }
        var (ok, err) = await _charTypesSvc.UpdateAsync(id.Value, CharTypeNameBox.Text ?? "");
        ShowMsg(CharTypeMsg, ok, ok ? "Тип оновлено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingCharTypeBox);
            CharTypeNameBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnDeleteCharType(object? sender, RoutedEventArgs e)
    {
        CharTypeMsg.IsVisible = false;
        var id = SelectedId(ExistingCharTypeBox);
        if (id is null) { ShowMsg(CharTypeMsg, false, "Оберіть тип."); return; }
        var (ok, err) = await _charTypesSvc.DeleteAsync(id.Value);
        ShowMsg(CharTypeMsg, ok, ok ? "Тип видалено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingCharTypeBox);
            CharTypeNameBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnAddChar(object? sender, RoutedEventArgs e)
    {
        CharMsg.IsVisible = false;
        var typeId = SelectedId(CharTypeSelectorBox);
        if (typeId is null) { ShowMsg(CharMsg, false, "Оберіть тип."); return; }
        var (ok, err) = await _charsSvc.AddAsync(typeId.Value, CharValueBox.Text ?? "");
        ShowMsg(CharMsg, ok, ok ? "Характеристику додано." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingCharBox);
            ClearExistingSelection(CharTypeSelectorBox);
            CharValueBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnUpdateChar(object? sender, RoutedEventArgs e)
    {
        CharMsg.IsVisible = false;
        var id = SelectedId(ExistingCharBox);
        var typeId = SelectedId(CharTypeSelectorBox);
        if (id is null || typeId is null) { ShowMsg(CharMsg, false, "Оберіть характеристику і тип."); return; }
        var (ok, err) = await _charsSvc.UpdateAsync(id.Value, typeId.Value, CharValueBox.Text ?? "");
        ShowMsg(CharMsg, ok, ok ? "Характеристику оновлено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingCharBox);
            ClearExistingSelection(CharTypeSelectorBox);
            CharValueBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnDeleteChar(object? sender, RoutedEventArgs e)
    {
        CharMsg.IsVisible = false;
        var id = SelectedId(ExistingCharBox);
        if (id is null) { ShowMsg(CharMsg, false, "Оберіть характеристику."); return; }
        var (ok, err) = await _charsSvc.DeleteAsync(id.Value);
        ShowMsg(CharMsg, ok, ok ? "Характеристику видалено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingCharBox);
            ClearExistingSelection(CharTypeSelectorBox);
            CharValueBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnAddPriority(object? sender, RoutedEventArgs e)
    {
        PriorityMsg.IsVisible = false;
        var (ok, err) = await _prioritiesSvc.AddAsync(PriorityNameBox.Text ?? "");
        ShowMsg(PriorityMsg, ok, ok ? "Пріоритет додано." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingPriorityBox);
            PriorityNameBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnUpdatePriority(object? sender, RoutedEventArgs e)
    {
        PriorityMsg.IsVisible = false;
        var id = SelectedId(ExistingPriorityBox);
        if (id is null) { ShowMsg(PriorityMsg, false, "Оберіть пріоритет."); return; }
        var (ok, err) = await _prioritiesSvc.UpdateAsync(id.Value, PriorityNameBox.Text ?? "");
        ShowMsg(PriorityMsg, ok, ok ? "Пріоритет оновлено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingPriorityBox);
            PriorityNameBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnDeletePriority(object? sender, RoutedEventArgs e)
    {
        PriorityMsg.IsVisible = false;
        var id = SelectedId(ExistingPriorityBox);
        if (id is null) { ShowMsg(PriorityMsg, false, "Оберіть пріоритет."); return; }
        var (ok, err) = await _prioritiesSvc.DeleteAsync(id.Value);
        ShowMsg(PriorityMsg, ok, ok ? "Пріоритет видалено." : err);
        if (ok)
        {
            ClearExistingSelection(ExistingPriorityBox);
            PriorityNameBox.Text = "";
            await ReloadLookupsAsync();
        }
    }

    private async void OnAddAllowedChar(object? sender, RoutedEventArgs e)
    {
        AllowedCharMsg.IsVisible = false;
        var carTypeId = SelectedId(AllowedCharCarTypeBox);
        var charId = SelectedId(AllowedCharBox);
        if (carTypeId is null || charId is null)
        {
            ShowMsg(AllowedCharMsg, false, "Оберіть тип вагона і характеристику.");
            return;
        }

        await using var db = new RailwayContext();

        var exists = await db.CarTypeAllowedCharacteristics.AnyAsync(
            x => x.CarTypeId == carTypeId.Value && x.SeatCharacteristicId == charId.Value);
        if (exists)
        {
            ShowMsg(AllowedCharMsg, false, "Такий зв'язок вже існує.");
            return;
        }

        db.CarTypeAllowedCharacteristics.Add(new CarTypeAllowedCharacteristic
        {
            CarTypeId = carTypeId.Value,
            SeatCharacteristicId = charId.Value
        });

        try
        {
            await db.SaveChangesAsync();
            ShowMsg(AllowedCharMsg, true, "Зв'язок додано.");
            ClearExistingSelection(ExistingAllowedCharBox);
            ClearExistingSelection(AllowedCharCarTypeBox);
            ClearExistingSelection(AllowedCharBox);
            await ReloadLookupsAsync();
        }
        catch (DbUpdateException ex)
        {
            ShowMsg(AllowedCharMsg, false, $"Помилка: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    private async void OnDeleteAllowedChar(object? sender, RoutedEventArgs e)
    {
        AllowedCharMsg.IsVisible = false;
        var id = SelectedId(ExistingAllowedCharBox);
        if (id is null) { ShowMsg(AllowedCharMsg, false, "Оберіть зв'язок."); return; }

        await using var db = new RailwayContext();
        var entity = await db.CarTypeAllowedCharacteristics
            .FirstOrDefaultAsync(x => x.Id == id.Value);

        if (entity == null)
        {
            ShowMsg(AllowedCharMsg, false, "Зв'язок не знайдено.");
            return;
        }

        db.CarTypeAllowedCharacteristics.Remove(entity);

        try
        {
            await db.SaveChangesAsync();
            ShowMsg(AllowedCharMsg, true, "Зв'язок видалено.");
            ClearExistingSelection(ExistingAllowedCharBox);
            ClearExistingSelection(AllowedCharCarTypeBox);
            ClearExistingSelection(AllowedCharBox);
            await ReloadLookupsAsync();
        }
        catch (DbUpdateException)
        {
            ShowMsg(AllowedCharMsg, false, "Не можна видалити зв'язок.");
        }
    }
}
