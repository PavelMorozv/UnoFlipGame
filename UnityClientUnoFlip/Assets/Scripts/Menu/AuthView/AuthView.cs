using Network;
using UnityEngine;
using UnityEngine.UI;

public class AuthView : MonoBehaviour
{
    public GameObject gameController;
    public GameObject menuController;
    public GameObject objLoginInput;
    public GameObject objPasswordInput;
    public GameObject objectRegisterButton;
    public GameObject objectLoginButton;


    private MyGameManager gameManager;
    private MenuManager menuManager;
    private Button loginButton;
    private Button registerButton;
    private InputField loginInput;
    private InputField passwordInput;


    private void Start()
    {
        gameManager = gameController.GetComponent<MyGameManager>();
        menuManager = menuController.GetComponent<MenuManager>();
        loginButton = objectLoginButton.GetComponent<Button>();
        registerButton = objectRegisterButton.GetComponent<Button>();
        loginInput = objLoginInput.GetComponent<InputField>();
        passwordInput = objPasswordInput.GetComponent<InputField>();

        loginButton.onClick.AddListener(() => onClick_Login());
        registerButton.onClick.AddListener(() => onClick_Register());

        gameManager.OnReceive += GameManager_OnReceive;
    }

    private void GameManager_OnReceive(Packet packet)
    {
        if (packet.Get<string>(Property.TargetModule) != "AuthorizationModule") return;

        if (packet.Get<AuthMethods>(Property.Method) == AuthMethods.login_Ok
            && packet.Get<AuthMethods>(Property.Method) == AuthMethods.register_Ok)
        {
            Auth auth = packet.Get<Auth>(Property.Data);

            gameManager.auth = auth;

            PlayerPrefs.SetString("Login", auth.Login);
            PlayerPrefs.SetString("Tokken", auth.Tokken);
            PlayerPrefs.Save();
            menuManager.MainMenu();
        }
    }

    private void onClick_Login()
    {
        Auth auth = new Auth()
        {
            Type = AuthMethods.login,
            Login = loginInput.text,
            Password = passwordInput.text
        };

        if (auth.Login == "" && auth.Password == ""
            && gameManager.auth.Login == auth.Login)
        {
            if (auth.Login == "") loginInput.text = "Логин не может быть пустым";
            if (auth.Login == "") loginInput.text = "Пароль не может быть пустым";
            return;
        }

        gameManager.client.Send(new Packet()
            .Add(Property.Type, PacketType.Request)
            .Add(Property.TargetModule, "AuthorizationModule")
            .Add(Property.Method, AuthMethods.login)
            .Add(Property.Data, auth));
    }

    private void onClick_Register()
    {
        Auth auth = new Auth()
        {
            Type = AuthMethods.register,
            Login = loginInput.text,
            Password = passwordInput.text
        };

        if (auth.Login == "" && auth.Password == ""
            && gameManager.auth.Login == auth.Login)
        {
            if (auth.Login == "") loginInput.text = "Логин не может быть пустым";
            if (auth.Login == "") loginInput.text = "Пароль не может быть пустым";
            return;
        }

        gameManager.client.Send(new Packet()
            .Add(Property.Type, PacketType.Request)
            .Add(Property.TargetModule, "AuthorizationModule")
            .Add(Property.Method, AuthMethods.register)
            .Add(Property.Data, auth));
    }
}