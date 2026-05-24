using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork_db.Authorization;
using CourseWork_db.Helpers;

namespace CourseWork_db.Views;

public partial class AdminLoginView : UserControl
{
    private readonly AdminAuthService _adminAuth = new();

    public event Action? AdminLoggedIn;
    public event Action? BackRequested;

    public AdminLoginView()
    {
        InitializeComponent();
    }

    private void OnLoginClick(object? sender, RoutedEventArgs e)
    {
        var (ok, error) = _adminAuth.Login(AdminLoginInput.Text ?? "", AdminPasswordInput.Text ?? "");
        if (!ok)
        {
            ViewStatusHelper.Set(StatusText, error, true);
            return;
        }

        ViewStatusHelper.Set(StatusText, "");
        AdminLoggedIn?.Invoke();
    }

    private void OnBackClick(object? sender, RoutedEventArgs e) =>
        BackRequested?.Invoke();
}
