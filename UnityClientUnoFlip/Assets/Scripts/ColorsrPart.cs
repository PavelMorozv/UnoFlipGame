using GameCore.Enums;
using UnityEngine;
using Color = GameCore.Enums.Color;

public class ColorsrPart : MonoBehaviour
{
    public delegate void EnterColor(Color color);
    public event EnterColor OnEnterColor;

    public GameObject item;

    public void Init(Side sides)
    {
        if (sides == Side.Light)
        {
            for (int i = 0; i < 4; i++)
            {
                AddItem((Color)i);
            }
        }
        else if (sides == Side.Dark)
        {
            for (int i = 4; i < 8; i++)
            {
                AddItem((Color)i);
            }
        }
    }

    private void AddItem(Color color)
    {
        var tempObject = Instantiate(item, transform, false);
        var itemPart = tempObject.GetComponent<ItemPart>();
        itemPart.Initialized(color);
        itemPart.OnEnterColor += ItemPart_OnEnterColor;
    }

    private void ItemPart_OnEnterColor(Color color)
    {
        OnEnterColor?.Invoke(color);

        Destroy(transform.gameObject, 0.2f);
    }
}
