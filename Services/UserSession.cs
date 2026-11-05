namespace RepoStudyPP.Services;

public sealed class UserSession
{
    public User? CurrentUser { get; private set; }

    public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
    public bool IsVolunteer => CurrentUser?.Role == UserRole.Volunteer;
    public bool IsClient => CurrentUser?.Role == UserRole.Client;
    public bool IsVeterinarian => CurrentUser?.Role == UserRole.Veterinarian;

    public event Action? Changed;

    public bool TryLogin(string login, string password)
    {
        var user = UserStore.Users.FirstOrDefault(item =>
            item.Login.Equals(login.Trim(), StringComparison.OrdinalIgnoreCase) &&
            item.Password == password);

        if (user is null)
            return false;

        CurrentUser = user;
        Changed?.Invoke();
        return true;
    }

    public void Logout()
    {
        CurrentUser = null;
        Changed?.Invoke();
    }
}
