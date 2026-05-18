using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using CourseWork_db.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork_db.Authorization;

public class AuthService
{
    public async Task<(bool Ok, string Error)> RegisterAsync(
        string login,
        string name,
        string surname,
        string email,
        string password,
        CancellationToken ct = default)
    {
        login = (login ?? "").Trim();
        name = (name ?? "").Trim();
        surname = (surname ?? "").Trim();
        email = (email ?? "").Trim();

        if (string.IsNullOrWhiteSpace(login)) 
            return (false, "Логін обовʼязковий.");
        if (string.IsNullOrWhiteSpace(name)) 
            return (false, "Імʼя обовʼязкове.");
        if (string.IsNullOrWhiteSpace(surname)) 
            return (false, "Прізвище обовʼязкове.");
        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email обовʼязковий.");
        if (password.Length < 6)
            return (false, "Пароль має містити мінімум 6 символів.");

        try
        {
            _ = new MailAddress(email);
        }
        catch
        {
            return (false, "Некоректний email.");
        }

        await using var db = new RailwayContext();

        var loginExists = await db.Users.AnyAsync(u => u.Login == login, ct);
        if (loginExists) 
            return (false, "Такий логін вже зайнятий.");

        var emailExists = await db.Users.AnyAsync(u => u.Email == email, ct);
        if (emailExists) 
            return (false, "Такий email вже зареєстрований.");

        var user = new User
        {
            Login = login,
            Name = name,
            Surname = surname,
            Email = email,
            Password = PasswordHasher.Hash(password),
        };

        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync(ct);
            return (true, "");
        }
        catch (DbUpdateException)
        {
            return (false, "Помилка збереження користувача.");
        }
    }

    public async Task<(bool Ok, string Error, User? User)> LoginAsync(
        string loginOrEmail,
        string password,
        CancellationToken ct = default)
    {
        loginOrEmail = (loginOrEmail ?? "").Trim();

        if (string.IsNullOrWhiteSpace(loginOrEmail)) 
            return (false, "Введіть логін або email.", null);
        if (string.IsNullOrWhiteSpace(password)) 
            return (false, "Введіть пароль.", null);

        await using var db = new RailwayContext();
        
        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Login == loginOrEmail ||
                     u.Email == loginOrEmail,
                ct);
        if (user is null)
            return (false, "Користувача не знайдено.", null);

        var passwordCorrect =
            PasswordHasher.Verify(password, user.Password);

        if (!passwordCorrect)
            return (false, "Невірний пароль.", null);

        return (true, "", user);
    }
}