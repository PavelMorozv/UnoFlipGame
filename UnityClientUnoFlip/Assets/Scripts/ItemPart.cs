using UnityEngine;
using Color = GameCore.Enums.Color;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemPart : MonoBehaviour, IPointerClickHandler
{
    public delegate void EnterColor(Color color);
    public event EnterColor OnEnterColor;

    private Sprite _sprite;
    private Color _color;


    public void Initialized(Color color)
    {
        _sprite = Resources.Load<Sprite>("Color Parts/" + color.ToString() + "Part");
        _color = color;
        GetComponentInParent<Image>().sprite = _sprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnEnterColor?.Invoke(_color);
    }
}