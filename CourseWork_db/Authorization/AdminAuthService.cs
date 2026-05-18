namespace CourseWork_db.Authorization;

public class AdminAuthService
{
    private const string AdminLogin = "admin";
    private const string AdminPassword = "admin123";

    public (bool Ok, string Error) Login(string login, string password)
    {
        login = (login ?? "").Trim();
        password = password ?? "";

        if (login != AdminLogin) 
            return (false, "Невірний логін адміністратора.");
        if (password != AdminPassword) 
            return (false, "Невірний пароль адміністратора.");
        return (true, "");
    }
}