using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork_db.Authorization;
using CourseWork_db.Helpers;

namespace CourseWork_db.Views;

public partial class RegisterView : UserControl
{
    private readonly AuthService _auth = new();

    public event Action? BackRequested;
    public event Action? Registered;

    public RegisterView()
    {
        InitializeComponent();
    }

    private async void OnRegisterClick(object? sender, RoutedEventArgs e)
    {
        var (ok, error) = await _auth.RegisterAsync(
            RegLogin.Text ?? "",
            RegName.Text ?? "",
            RegSurname.Text ?? "",
            RegEmail.Text ?? "",
            RegPassword.Text ?? "");

        ViewStatusHelper.Set(StatusText, ok ? "Реєстрація успішна! Увійдіть." : error, !ok);
        if (ok) Registered?.Invoke();
    }

    private void OnBackClick(object? sender, RoutedEventArgs e) =>
        BackRequested?.Invoke();
}
