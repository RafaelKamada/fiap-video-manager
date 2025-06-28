namespace VideoManager.Application.Common.Reponse;

public class LoginResponse
{
    public bool Success { get; set; } = false;
    public string Token { get; set; } = string.Empty;
    public string MessageError { get; set; } = string.Empty;
}
