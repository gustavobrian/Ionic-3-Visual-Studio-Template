namespace Sammaron.Server.ViewModels
{
    public class LoginModel
    {
        public string UserIdentifier { get; set; }
        public string Password { get; set; }

    }


    public enum GrantType
    {
        password,
        refresh_token,
        totp
    }
}