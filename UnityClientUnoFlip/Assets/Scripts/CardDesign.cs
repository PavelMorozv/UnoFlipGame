using GameCore.Classes;
using GameCore.Enums;
using UnityEngine;
using UnityEngine.UI;

public class CardDesign : MonoBehaviour
{
    private CardMovement cardMovement;
    private MyGameManager gameManager;

    Sprite spriteLight;
    Sprite spriteDark;
    Text[] texts;

    Image image;

    string symLight, symDark;

    public Card Info { get; private set; }

    public void Initialized(MyGameManager game, Card card)
    {
        gameManager = game;
        Info = card;

        spriteLight = GetImage(Info.GetSide(Side.Light), Side.Light);
        spriteDark = GetImage(Info.GetSide(Side.Dark), Side.Dark);

        symLight = GetSymbol(Info.GetSide(Side.Light));
        symDark = GetSymbol(Info.GetSide(Side.Dark));
    }

    private Sprite GetImage(CardSide side, Side CurrentSide)
    {
            switch (side.Action)
            {
                case Action.Wild:
                case Action.WildGive:
                case Action.WildGiveForNow:
                        return Resources.Load<Sprite>(CurrentSide.ToString());
            }

        return Resources.Load<Sprite>(side.Color.ToString());
    }

    private string GetSymbol(CardSide side)
    {
        switch (side.Action)
        {
            case Action.Number:
                return side.Value.ToString();
            case Action.Give:
                return "+" + side.Value.ToString();
            case Action.ChangeDirection:
                return "⇄";
            case Action.Wild:
                return "";
            case Action.WildGive:
                return "+" + side.Value.ToString();
            case Action.WildGiveForNow:
                return "+1..n";
            case Action.SkipMove:
                return "∅";
            case Action.SkipMoveAll:
                return "↺";
            case Action.Flip:
                return "<>";
        }

        return "Error";
    }


    private void Start()
    {
        cardMovement = GetComponent<CardMovement>();

        image = transform.GetComponentInChildren<Image>();
        texts = transform.GetComponentsInChildren<Text>();
    }

    private void Update()
    {
        if (transform.parent.name == "MainDeck" || transform.parent.name == "DeckEnemy")
        {
            image.sprite = gameManager.gameState.Side == Side.Light ? spriteDark : spriteLight;

            foreach (var text in texts)
            {
                text.text = gameManager.gameState.Side == Side.Light ? symDark : symLight;
            }
        }
        else
        {
            image.sprite = gameManager.gameState.Side == Side.Light ? spriteLight : spriteDark;

            foreach (var text in texts)
            {
                text.text = gameManager.gameState.Side == Side.Light ? symLight : symDark;
            }
        }

        if (cardMovement.isMove) image.transform.localScale = new Vector3(1f, 1f);
        else image.transform.localScale = new Vector3(0.8f, 0.8f);

        // Все пропускают ↺
        // Пропускает 1 игрок ∅
        // Смена направления ⇄
    }
}
