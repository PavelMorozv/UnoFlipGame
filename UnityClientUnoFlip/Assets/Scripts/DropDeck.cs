using GameCore.Classes;
using GameCore.Enums;
using Color = GameCore.Enums.Color;
using Network;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropDeck : MonoBehaviour, IDropHandler
{
    private MyGameManager gm;
    private Card card;
    public GameObject ColorSelectionPanel;

    private void Start()
    {
        gm = GameObject.Find("GameController").GetComponent<MyGameManager>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (gm.gameState.CurrentPlayer != gm.auth.Id) return;

        CardMovement cardMovement = eventData.pointerDrag.GetComponent<CardMovement>();
        if (!cardMovement.isMove) return;

        cardMovement.defaultParent = transform;
        cardMovement.transform.localScale = new Vector3(0.7f, 0.7f);
        cardMovement.isDrop = true;


        card = cardMovement.GetComponentInParent<CardDesign>().Info;

        if (card.Action(gm.gameState.Side).ToString().Contains("Wild"))
        {
            ShowColorSelection(card, gm.gameState.Side);
            return;
        }
        if(card.Action(gm.gameState.Side) == Action.Flip
            && card.Action(gm.gameState.Side==Side.Light ? Side.Dark : Side.Light).ToString().Contains("Wild"))
        {
            if (gm.gameState.Side == Side.Light) gm.gameState.Side = Side.Dark; 
            else gm.gameState.Side = Side.Light;

            ShowColorSelection(card, gm.gameState.Side);
        }
        else
        {
            SendCardToServer(card);
            return;
        }
    }

    private void ShowColorSelection(Card card, Side sides)
    {
        var tempObject = Instantiate(ColorSelectionPanel, GameObject.Find("UICanvas").transform, false);
        var colorsrPart = tempObject.GetComponent<ColorsrPart>();
        colorsrPart.Init(sides);
        colorsrPart.OnEnterColor += ColorsrPart_OnEnterColor;
    }

    private void ColorsrPart_OnEnterColor(Color color)
    {
        card.SetColor(gm.gameState.Side, color);
        SendCardToServer(card);
    }

    private void SendCardToServer(Card card)
    {
        var packet = new Packet()
                            .Add(Property.Type, PacketType.Response)
                            .Add(Property.TargetModule, "GamesModule")
                            .Add(Property.Method, "move")
                            .Add(Property.Data, card);
        gm.client.Send(packet);

        Debug.Log(packet);
    }
}
