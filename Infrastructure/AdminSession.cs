namespace StadiumSystem.Infrastructure;

/// <summary>
/// GRASP - Information Expert: conoce las credenciales y el estado de
/// autenticación del administrador del sistema.
/// </summary>
public class AdminSession
{
    private string _username;
    private bool _isAuthenticated;

    /// <summary>Intenta autenticar al usuario con las credenciales dadas.</summary>
    public bool Login(string user, string pass) { return false; }

    /// <summary>Cierra la sesión activa.</summary>
    public void Logout() { }

    /// <summary>Indica si hay una sesión autenticada activa.</summary>
    public bool IsAuthenticated() { return _isAuthenticated; }
}
