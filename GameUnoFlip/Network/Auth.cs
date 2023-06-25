using System;

namespace Network
{
    [Serializable]
    public enum AuthMethods
    {
        login,
        login_OnUseToken,

        logout,
        register,

        login_Ok,
        logout_Ok,
        register_Ok,

        login_Error,
        logout_Error,
        register_Error,
        FastAuth
    }

    [Serializable]
    public class Auth
    {
        public AuthMethods Type { get; set; }

        public string Login { get; set; }
        public string Password { get; set; }
        public string Tokken { get; set; }
        public int Id { get; set; }

        public override string ToString()
        {
            string result = string.Empty;
            
            result += "Type: " + Type + "\n";

            if (Login != null && Login.Length > 0)
                result += "Name: " + Login + "\n";

            if (Password != null && Password.Length > 0)
                result += "Password: " + Password + "\n";

            if (Tokken != null && Tokken.Length > 0)
                result += "Token: " + Tokken + "\n";

            return result;
        }
    }
}
