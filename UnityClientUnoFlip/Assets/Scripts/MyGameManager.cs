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
    public GameObject deckPlayer;
    public GameObject deckEnemy;
    public GameObject deckDrop;
    public GameObject deckMain;
    public GameObject card;

    public GameObject Win;
    public GameObject Lost;
    public GameObject devInfo;

    public Client client;
    public PlayerState player, enemy;
    public GameState gameState = new GameState()
    {
        CurrentPlayer = -1
    };

    public event Action<Packet>? OnReceive;



    void Start()
    {
        player = enemy = new PlayerState() { Id = -1, IsGive = false, IsUno = false, Cards = new List<Card>() };

        client = new Client("109.195.67.94", 9999);
        //client.Send(new Packet().Add(Property.Type, PacketType.Response)
        //    .Add(Property.TargetModule, "RoomsModule")
        //    .Add(Property.Method, "join")
        //    .Add(Property.Data, 0));
    }

    void Update()
    {
        if (client.Connected && client.Available())
        {
            var pkg = client.Read();
            if (pkg.Get<PacketType>(Property.Type) != PacketType.Test) ReciveProcess(pkg);
            OnReceive?.Invoke(pkg);
        }

        //if (isReady)
        //{
        //    Destroy(mainMenus);
        //}

        if (gameState.CurrentPlayer == player.Id && client.Connected)
        {
            CheckMove();
        }
        infoUpdate();
        //if (client.Connected)
        //{
        //    Destroy(mainMenus);
        //    if (gameState.CurrentPlayer == player.Id && client.Connected)
        //    {
        //        CheckMove();
        //    }
        //}
    }

    void ReciveProcess(Packet packet)
    {
        var obj = GameObject.Find("MainMenu");
        Debug.Log(packet);
        PlayerState ps;
        switch (packet.Get<string>(Property.Method))
        {
            case "GameInit":
                gameState = packet.Get<GameState>(Property.Data);

                //gameState = Converter.GetObject<GameState>(gp.Data);
                //Debug.Log("Game Init: " + gameState);
                //enemy = new Player(gameState.CountPlayer - player.Id);
                break;

            case "GameState":

                //gameState = Converter.GetObject<GameState>(gp.Data);
                gameState = packet.Get<GameState>(Property.Data);
                DrawPlayerCards();
                DrawEnemyCards();
                DrawDropOrMain();
                if (gameState.Status == GameStatus.EndGame)
                {
                    if (gameState.CurrentPlayer == player.Id)
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
                break;

            case "AddCard":
                var card = packet.Get<Card>(Property.Data);
                player.Cards.Add(card);
                break;

            case "ChangeUno":
                break;

            case "AddCards":
                var cards = packet.Get<List<Card>>(Property.Data);
                player.Cards.AddRange(cards);
                break;

            case "RemoveCard":
                var rescard = packet.Get<Card>(Property.Data);
                var removeCard = player.Cards.First(c => c.Id == rescard.Id);
                player.Cards.Remove(removeCard);
                break;

            case "playerState":
                ps = packet.Get<PlayerState>(Property.Data);
                player = ps;
                break;
            case "join":
            case "create":
                if (packet.Get<bool>(Property.Data)) GameObject.Find("MenuController").GetComponent<MenuManager>().GmaePole();
                break;
        }
    }

    public void OnClickButtonPlay()
    {
        //if (!client.IsReady)
        //{
        //    client.SetReady();
        //    mainMenus.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = "Не готов";
        //}
        //else
        //{
        //    client.ResetReady();
        //    mainMenus.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = "Готов";
        //}
    }

    public void DrawPlayerCards()
    {
        deckPlayer.GetComponentsInChildren<CardDesign>().ToList().ForEach(c => Destroy(c.gameObject));

        foreach (var card in player.Cards)
        {
            AddCard(deckPlayer, card);
        }
    }

    public void onClick()
    {
        gameState.Side = gameState.Side == Side.Light ? Side.Dark : Side.Light;
    }

    public void DrawEnemyCards()
    {
        //deckEnemy.GetComponentsInChildren<CardDesign>().ToList().ForEach(c => Destroy(c.gameObject));

        //foreach (var card in enemy.Cards)
        //{
        //    AddCard(deckEnemy, card);
         //}
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
        var tempObject = Instantiate(card, target.transform, false);
        var tempDesign = tempObject.GetComponent<CardDesign>();
        tempDesign.Initialized(this, gameCard);
    }

    public void CheckMove()
    {
        foreach (var cardDesign in deckPlayer.GetComponentsInChildren<CardDesign>())
        {
            if (Game.IsMovePosible(gameState.LastCardPlayed, cardDesign.Info, gameState.Side))
            {
                cardDesign.gameObject.GetComponent<CardMovement>().isMove = true;
            }
            else
            {
                cardDesign.gameObject.GetComponent<CardMovement>().isMove = false;
            }
        }
    }

    private void infoUpdate()
    {
        Text info = devInfo.GetComponent<Text>();

        info.text = "";

        if (client?.ConnectedID != -1) info.text += client.ConnectedID + "\n";
        info.text += gameState + "\n";

        info.text += player + "\n";

    }
}