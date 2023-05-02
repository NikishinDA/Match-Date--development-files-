using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMover : MonoBehaviour
{
    public static ItemMover Instance;

   [SerializeField] private Match3 _game;
   [SerializeField] private Camera _camera;

    private ItemController _moving;

    private Vector2Int _newPos;

    private Vector3 _mouseStart;
    [SerializeField] private float _itemWidth;
    [SerializeField] private float _itemHeight;

    private bool _touchBlocked = true;
    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
        EventManager.AddListener<ItemsFallEvent>(OnItemFall);
        EventManager.AddListener<GameStartEvent>(OnGameStart);
        EventManager.AddListener<GameOverEvent>(OnGameOver);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<ItemsFallEvent>(OnItemFall);
        EventManager.RemoveListener<GameStartEvent>(OnGameStart);
        EventManager.RemoveListener<GameOverEvent>(OnGameOver);
    }

    private void OnGameOver(GameOverEvent obj)
    {
        _touchBlocked = true;
    }

    private void OnGameStart(GameStartEvent obj)
    {
        _touchBlocked = false;
    }

    private void OnItemFall(ItemsFallEvent obj)
    {
        _touchBlocked = obj.IsFalling;
    }


    // Update is called once per frame
    void Update()
    {
        if (_moving != null && !_touchBlocked)
        {
            Vector3 dir = Input.mousePosition- _mouseStart;
            Vector3 normDir = dir.normalized;
            Vector3 absDir = new Vector3(Mathf.Abs(dir.x), Mathf.Abs(dir.y), dir.z);
            _newPos = _moving.PositionOnField;
            Vector2Int add = Vector2Int.zero;
            if (dir.magnitude > _itemWidth / 2)
            {
                if (absDir.x > absDir.y)
                {
                    add = new Vector2Int((normDir.x) > 0 ? 1 : -1, 0);
                }
                else
                {
                    add = new Vector2Int(0,(normDir.y) > 0 ? 1 : -1);
                }
            }

            _newPos += add;
            Vector3 pos = _game.GetWorldPosAtPos(_moving.PositionOnField);
            if (!_newPos.Equals(_moving.PositionOnField))
            {
                pos += new Vector3(add.x, add.y, pos.z) * (_itemWidth / 4);
            }
            _moving.MovePositionTo(pos);
            
        }
    }

    public void MoveItem(ItemController item)
    {
        if (_moving != null) return;
        _moving = item;
        _mouseStart = Input.mousePosition;
    }

    public void DropItem()
    {
        if (_moving == null) return;
        if (!_newPos.Equals(_moving.PositionOnField))
        {
            _game.SwitchItems(_moving.PositionOnField, _newPos, true);
            EventManager.Broadcast(GameEventsHandler.TurnMadeEvent);
        }
        _game.ResetItem(_moving);
        _moving = null;
        
    }
}
