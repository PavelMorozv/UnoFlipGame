using Network;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoom : MonoBehaviour
{
    public GameObject createButtonObject;
    public GameObject textBoxObject;
    public GameObject gameController;
    public GameObject menuController;

    private Button createButton;
    private InputField inputField;
    private MyGameManager gameManager;
    private MenuManager menuManager;


    void Start()
    {
        createButton = createButtonObject.GetComponent<Button>();
        inputField = textBoxObject.GetComponent<InputField>();
        gameManager = gameController.GetComponent<MyGameManager>();
        menuManager = menuController.GetComponent<MenuManager>();

        createButton.onClick.AddListener(() => { onClick(); });
    }

    private void onClick()
    {
        gameManager.client.Send( new Packet()
            .Add(Property.Type, PacketType.Request)
            .Add(Property.TargetModule, "RoomsModule")
            .Add(Property.Method, "create")
            .Add(Property.Data, inputField.text));
        menuManager.MainMenu();
    }
}