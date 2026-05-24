// PATTERN: SINGLETON
namespace ByteMe.Services.Patterns;

public class UserSessionManager
{
    private static UserSessionManager? _instance;
    private static readonly object _lock = new();

    public int UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public string UserRole { get; private set; } = string.Empty;
    public bool IsLoggedIn { get; private set; }

    private UserSessionManager() { }

    public static UserSessionManager GetInstance()
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                _instance ??= new UserSessionManager();
            }
        }
        return _instance;
    }

    public void SetUser(int id, string name, string role)
    {
        UserId = id;
        UserName = name;
        UserRole = role;
        IsLoggedIn = true;
    }

    public void ClearUser()
    {
        UserId = 0;
        UserName = string.Empty;
        UserRole = string.Empty;
        IsLoggedIn = false;
    }

    public void SyncFromSession(ISession session)
    {
        if (session.GetInt32("UserId") is int userId && userId > 0)
        {
            SetUser(
                userId,
                session.GetString("UserName") ?? string.Empty,
                session.GetString("UserRole") ?? "Customer");
        }
        else
        {
            ClearUser();
        }
    }

    public void SyncToSession(ISession session)
    {
        if (IsLoggedIn)
        {
            session.SetInt32("UserId", UserId);
            session.SetString("UserName", UserName);
            session.SetString("UserRole", UserRole);
        }
        else
        {
            session.Remove("UserId");
            session.Remove("UserName");
            session.Remove("UserRole");
        }
    }
}
