using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public ItemColor Color;
    public Vector2Int PositionOnField;
    [HideInInspector] public Vector3 PositionInWorld;
    //[HideInInspector] public ItemController Switched;

    [SerializeField] private float _itemWidth;
    [SerializeField] private float _itemHeight;
    [SerializeField] private float _speed;
    [SerializeField] private GameObject _ice;
    private Sprite _sprite;

    private bool _updating;

    [HideInInspector] public bool BombNext = false;
    [HideInInspector] public bool FireworkNext = false;
    [HideInInspector] public bool Interacted = false;
    [HideInInspector] public bool Destroying = false;
    [HideInInspector] public bool JustCreated = false;
    [HideInInspector] public bool Effect = false;
    [HideInInspector] public bool NoEffectDestroy = false;
    private bool _iced =false;
    
    [SerializeField] private Text _comboText;
    [SerializeField] private string[] _strings;
    [SerializeField] private GameObject _fuseFX;
    public bool Iced => _iced;

    private void Awake()
    {
        EventManager.AddListener<ItemsFallEvent>(OnItemsFall);
    }

    

    private void OnDestroy()
    {
        EventManager.RemoveListener<ItemsFallEvent>(OnItemsFall);
    }
    private void OnItemsFall(ItemsFallEvent obj)
    {
        if (!obj.IsFalling)
        {
            JustCreated = false;
        }
    }
    public void Initialize(ItemColor color, Vector2Int pos, Sprite itemSprite)
    {
        GetComponent<Image>().sprite = itemSprite;
        Color = color;
        SetPosition(pos);
        BombNext = false;
        FireworkNext = false;
        Interacted = false;
        Destroying = false;
        NoEffectDestroy = false;
        ToggleIce(false);
        _fuseFX.SetActive(false);
        //Iced = false;
        //Switched = null;
    }

    public void ToggleIce(bool iced)
    {
        _iced = iced;
        _ice.SetActive(iced);
    }

    public void SetPosition(Vector2Int pos)
    {
        PositionOnField = pos;
        ResetPosition();
        ChangeName();
    }
    public void ResetPosition()
    {
        PositionInWorld = new Vector3(_itemWidth/2 + _itemWidth * PositionOnField.x,_itemHeight/2 + _itemHeight*PositionOnField.y, transform.position.z);//new Vector3(PositionOnField.x, PositionOnField.y, PositionInWorld.z);
    }

    public void MovePosition(Vector3 move)
    {
        transform.position += move * (Time.deltaTime * 16f);
    }
    public void MovePositionTo(Vector3 move)
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition,move,Time.deltaTime * _speed);
    }
    public void MovePositionToWithSpeed(Vector3 move, float speed)
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition,move,Time.deltaTime * speed);
    }
    private void ChangeName()
    {
        transform.name = "Node " + PositionOnField.x + ", " + PositionOnField.y;
    }

    public bool UpdateItem()
    {
        if (Vector3.Distance(transform.localPosition, PositionInWorld) > 1)
        {
            MovePositionTo(PositionInWorld);
            _updating = true;
            return true;
        }
        else
        {
            transform.localPosition = PositionInWorld;
            _updating = false;
            return false;
        }
    }

    public void ShowComboText()
    {
        _comboText.gameObject.SetActive(true);
        _comboText.text = _strings[Random.Range(0, _strings.Length)];
    }

    public void ShowFireworkTrail()
    {
        _fuseFX.SetActive(true);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_updating || Effect) return;
        ItemMover.Instance.MoveItem(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ItemMover.Instance.DropItem();
    }
}
