using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWork_db.Models;
using CourseWork_db.Views;
using System.Threading.Tasks;

namespace CourseWork_db;

public partial class MainWindow : Window
{
    private readonly LoginView _loginView = new();
    private readonly RegisterView _registerView = new();
    private readonly AdminLoginView _adminLoginView = new();
    private readonly AdminPanelView _adminPanelView = new();

    private User? _currentUser;
    private bool _isAdmin;

    public MainWindow()
    {
        InitializeComponent();
        WireAuthViews();
        ShowLogin();
    }

    private void WireAuthViews()
    {
        _loginView.LoginSucceeded += OnUserLoggedIn;
        _loginView.RegisterRequested += ShowRegister;
        _loginView.AdminLoginRequested += ShowAdminLogin;

        _registerView.BackRequested += ShowLogin;
        _registerView.Registered += ShowLogin;

        _adminLoginView.BackRequested += ShowLogin;
        _adminLoginView.AdminLoggedIn += OnAdminLoggedIn;

        _adminPanelView.ExitRequested += OnLogout;
    }

    private void ShowView(Control view)
    {
        ContentHost.Content = view;
    }

    private void ShowLogin()
    {
        ClearAllForms();
        UserGreeting.IsVisible = false;
        LogoutBtn.IsVisible = false;
        HeaderSubtitle.Text = "Система бронювання квитків";
        ShowView(_loginView);
    }

    private void ShowRegister()
    {
        ClearAllForms();
        ShowView(_registerView);
    }

    private void ShowAdminLogin()
    {
        ClearAllForms();
        ShowView(_adminLoginView);
    }

    private async void OnUserLoggedIn(User user)
    {
        try
        {
            _currentUser = user;
            _isAdmin = false;
            ShowMain();
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            await Helpers.UiDispatcher.RunAsync(() =>
            {
                var dialog = new Window
                {
                    Title = "Помилка входу",
                    Width = 400,
                    Height = 180,
                    Content = new TextBlock
                    {
                        Text = msg,
                        Margin = new Avalonia.Thickness(16),
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    }
                };
                dialog.ShowDialog(this);
                OnLogout();
            });
        }
    }

    private void OnAdminLoggedIn()
    {
        _currentUser = null;
        _isAdmin = true;
        ShowMain();
    }

    private void ClearAllForms()
    {
        _loginView.ClearForm();
        _registerView.ClearForm();
        _adminLoginView.ClearForm();
    }

    private void ShowMain()
    {
        ClearAllForms();
        UserGreeting.IsVisible = true;
        LogoutBtn.IsVisible = true;

        if (_isAdmin)
        {
            UserGreeting.Text = "Адміністратор";
            HeaderSubtitle.Text = "Панель керування";
            ShowView(_adminPanelView);
        }
        else
        {
            UserGreeting.Text = $"{_currentUser?.Name} {_currentUser?.Surname}";
            HeaderSubtitle.Text = "Приємної подорожі!";
        }
    }

    private void OnLogoutClick(object? sender, RoutedEventArgs e) => OnLogout();

    private void OnLogout()
    {
        _currentUser = null;
        _isAdmin = false;
        ShowLogin();
    }
}
