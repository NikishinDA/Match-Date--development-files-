using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemColor
{
    Abyss = -1,
    None = 0,
    Red,
    Yellow,
    Green,
    Blue,
    Bomb = 8,
    Firework
}
public class Node //: MonoBehaviour
{
    public ItemColor Color;
    public Vector2Int Position;
    private ItemController _item;
    public Node(ItemColor color, Vector2Int pos)
    {
        Color = color;
        Position = pos;
    }

    public bool IsIced()
    {
        if (_item != null)
        {
            return _item.Iced;
        }
        else
        {
            return false;
        }
    }
    public void SetItem(ItemController newItem)
    {
        _item = newItem;
        Color = (newItem == null) ? ItemColor.None : newItem.Color;
        if (newItem == null) return;
        newItem.SetPosition(this.Position);
    }

    public ItemController GetItem()
    {
        return _item;
    }
}
