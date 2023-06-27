using Network;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardMovement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Canvas canvas;
    private MyGameManager gameManager;

    public Transform defaultParent;
    public bool isDrop = false;
    public bool isMove = false;
    private int curentPos;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        gameManager = GameObject.Find("GameController").GetComponent<MyGameManager>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isDrop) return;

        if (!isMove) return;

        GetComponent<CanvasGroup>().blocksRaycasts = false;
        curentPos = transform.GetSiblingIndex();
        defaultParent = transform.parent;
        transform.SetParent(defaultParent.parent, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDrop) return;

        if (!isMove) return;

        GetComponent<RectTransform>().anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        if (!isMove) return;

        GetComponent<CanvasGroup>().blocksRaycasts = true;
        transform.SetParent(defaultParent, false);
        if (isDrop)
        {
            transform.localPosition = new Vector3(0, 0);
        }
        else
        {
            transform.SetSiblingIndex(curentPos);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (transform.parent.name == "MainDeck" && gameManager.gameState.CurrentPlayer == gameManager.auth.Id)
        {
            if (gameManager.player.IsGive) return;

            var packet = new Packet()
                                .Add(Property.Type, PacketType.Response)
                                .Add(Property.TargetModule, "GamesModule")
                                .Add(Property.Method, "giveCard");
            gameManager.client.Send(packet);

            Debug.Log(packet);
        }
    }

}
