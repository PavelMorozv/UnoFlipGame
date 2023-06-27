using GameCore.Classes;
using GameCore.Enums;
using GameCore.Structs;
using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MyGameManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public GameObject deckDrop;
    public GameObject deckMain;
    public GameObject playersManagerObject;

    public GameObject Win;
    public GameObject Lost;
    public GameObject devInfo;
    private Text info;


    public PlayersManager Manager;
    public Client client;
    public Auth auth = new Auth()
    {
        Id = -1,
        Login = "No name",
        Password = "Password",
        Tokken = ""
    };
    public PlayerState player, enemy;
    public GameState gameState = new GameState()
    {
        CurrentPlayer = -1
    };

    public event Action<Packet> OnReceive;



    void Start()
    {
        player = enemy = new PlayerState() { Id = -1, IsGive = false, IsUno = false, Cards = new List<Card>() };

        client = new Client("109.195.67.94", 9999);

        client.Send(new Packet().Add(Property.Type, PacketType.Connect)
            .Add(Property.Data, (PlayerPrefs.HasKey("Tokken") ? PlayerPrefs.GetString("Tokken") : "")));
        info = devInfo.GetComponent<Text>();

        Manager = playersManagerObject.GetComponent<PlayersManager>();
    }

    void Update()
    {
        if (client.Connected && client.Available())
        {
            var pkg = client.Read();
            if (pkg.Get<PacketType>(Property.Type) != PacketType.Test) ReciveProcess(pkg);
            OnReceive?.Invoke(pkg);
        }

        //if (gameState.CurrentPlayer == player.Id && client.Connected)
        //{
        //    CheckMove();
        //}

        infoUpdate();
    }

    void ReciveProcess(Packet packet)
    {
        var obj = GameObject.Find("MainMenu");
        Debug.Log(packet);
        PlayerState ps;
        switch (packet.Get<string>(Property.Method))
        {
            #region Game Methods

            case "GetPlayersIds":
                {
                    var ids = packet.Get<int[]>(Property.Data);
                    Manager.Initialized(auth.Id, ids);
                }
                break;

            case "GameInit":
                gameState = packet.Get<GameState>(Property.Data);
                DrawDropOrMain();
                break;

            case "GameState":
                {
                    gameState = packet.Get<GameState>(Property.Data);
                    DrawDropOrMain();
                    Manager.ResetAvailableCards();
                    if (gameState.Status == GameStatus.EndGame)
                    {
                        if (gameState.CurrentPlayer == auth.Id)
                        {
                            obj = Instantiate(Win, GameObject.Find("UICanvas").transform, false);
                        }
                        else
                        {
                            obj = Instantiate(Lost, GameObject.Find("UICanvas").transform, false);
                        }
                        Destroy(obj, 3f);
                        GameObject.Find("MenuController").GetComponent<MenuManager>().MainMenu();
                    }

                    if (gameState.CurrentPlayer == auth.Id)
                    {
                        Manager.AvailableCards();
                    }
                }
                break;

            #endregion


            case "AddCard":
                ps = packet.Get<PlayerState>(Property.Data);
                Manager.AddCardToPlayer(ps.Id, ps.Cards[0]);
                break;

            case "ChangeUno":
                break;

            case "AddCards":
                ps = packet.Get<PlayerState>(Property.Data);
                Manager.AddCardsToPlayer(ps.Id, ps.Cards);
                break;

            case "RemoveCard":
                ps = packet.Get<PlayerState>(Property.Data);
                if (ps.Id == auth.Id) break;
                else Manager.RemoveCardFromPlayer(ps.Id, ps.Cards[0]);
                break;

            case "playerState":
                ps = packet.Get<PlayerState>(Property.Data);
                player = ps;
                break;

            case "join":
            case "create":
                if (packet.Get<bool>(Property.Data)) GameObject.Find("MenuController").GetComponent<MenuManager>().GmaePole();
                break;

            case "FastAuth":

                var resFastAuth = packet.Get<Auth>(Property.Data);
                auth.Id = resFastAuth.Id;
                auth.Login = resFastAuth.Login;
                auth.Tokken = resFastAuth.Tokken;

                PlayerPrefs.SetString("Login", auth.Login);
                PlayerPrefs.SetString("Tokken", auth.Tokken);

                PlayerPrefs.Save();

                break;

            case "FastReg":

                var resFastReg = packet.Get<Auth>(Property.Data);
                auth.Id = resFastReg.Id;
                auth.Login = resFastReg.Login;
                auth.Tokken = resFastReg.Tokken;

                PlayerPrefs.SetString("Login", auth.Login);
                PlayerPrefs.SetString("Tokken", auth.Tokken);

                PlayerPrefs.Save();
                break;

        }
    }

    public void DrawDropOrMain()
    {
        var delcard = new List<CardDesign>();
        delcard.AddRange(deckDrop.GetComponentsInChildren<CardDesign>());
        delcard.AddRange(deckMain.GetComponentsInChildren<CardDesign>());

        foreach (var card in delcard)
            Destroy(card.gameObject);

        AddCard(deckDrop, gameState.LastCardPlayed);
        AddCard(deckMain, gameState.NextCardInDeck);
    }

    private void AddCard(GameObject target, Card gameCard)
    {
        var tempObject = Instantiate(cardPrefab, target.transform, false);
        var tempDesign = tempObject.GetComponent<CardDesign>();
        tempDesign.Initialized(this, gameCard);
    }

    private void infoUpdate()
    {
        info.text = "";

        if (client?.ConnectedID != -1) info.text += client.ConnectedID + "\n";

        info.text += $"AUTH DATA {{ ID: {auth.Id}, Login: {auth.Login}, Tokken: {auth.Tokken} }}\n";

        info.text += gameState + "\n";

        info.text += player + "\n";

    }
}