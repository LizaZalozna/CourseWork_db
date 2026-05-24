using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork_db.Authorization;
using CourseWork_db.Helpers;
using CourseWork_db.Models;

namespace CourseWork_db.Views;

public partial class LoginView : UserControl
{
    private readonly AuthService _auth = new();

    public event Action<User>? LoginSucceeded;
    public event Action? RegisterRequested;
    public event Action? AdminLoginRequested;

    public LoginView()
    {
        InitializeComponent();
    }

    private async void OnLoginClick(object? sender, RoutedEventArgs e)
    {
        var (ok, error, user) = await _auth.LoginAsync(LoginInput.Text ?? "", LoginPassword.Text ?? "");
        if (!ok)
        {
            ViewStatusHelper.Set(StatusText, error, true);
            return;
        }

        ViewStatusHelper.Set(StatusText, "");
        LoginSucceeded?.Invoke(user!);
    }

    private void OnGoRegisterClick(object? sender, RoutedEventArgs e) =>
        RegisterRequested?.Invoke();

    private void OnGoAdminClick(object? sender, RoutedEventArgs e) =>
        AdminLoginRequested?.Invoke();
}
