
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using Random = UnityEngine.Random;

struct Point
{
    public int x;
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

[System.Serializable]
public class SwitchedItems
{
    public ItemController first;
    public ItemController second;

    public SwitchedItems(ItemController f, ItemController s)
    {
        first = f;
        second = s;
    }

    public ItemController GetPairedItem(ItemController item)
    {
        if (item == first) return second;
        else if (item == second) return first;
        else
        {
            return null;
        }
    }
}

public class Match3 : MonoBehaviour
{
    public static Match3 Instance;
    [SerializeField] private int _fieldWidth;
    [SerializeField] private int _fieldHeight;
    [SerializeField] private int _emptyNodes;
    [SerializeField] private Sprite[] _redSprites;
    [SerializeField] private Sprite[] _yellowSprites;
    [SerializeField] private Sprite[] _greenSprites;
    [SerializeField] private Sprite[] _blueSprites;
    [SerializeField] private Sprite _fireworkSprite;
    [SerializeField] private Sprite _bombSprite;
    [SerializeField] private GameObject _fieldDarkTilePrefab;
    [SerializeField] private GameObject _fieldLightTilePrefab;
    [SerializeField] private GameObject _outlinePrefab;
    [SerializeField] private Sprite[] _outlineSprites;
    public Sprite[] ActiveSprites;
    private Node[,] _gameField;
    private int[] _updateHeight;
    private List<ItemController> _update;
    private bool _updatingFalling;
    private List<SwitchedItems> _switched;
    private List<ItemController> _dead;
    private int[] fills;
    [SerializeField] private Transform _fields;
    [SerializeField] private Transform _fieldObject;
    [SerializeField] private Transform _backObject;
    [SerializeField] private Transform _vfxObjectBot;
    [SerializeField] private Transform _vfxObjectTop;
    [SerializeField] private ItemController _itemPrefab;
    private Dictionary<ItemColor, List<Vector2Int>> _colorsInConnected;
    [SerializeField] private float _itemWidth;

    [SerializeField] private float _itemHeight;
    [SerializeField] private float _iceChance;

    //[SerializeField] private GameObject _explosionEffect;
    private bool _cutCorners = true;

    
    [SerializeField] private GameObject _explVFX;
    [SerializeField] private GameObject _bigExplVFX;
    [SerializeField] private GameObject _fwVFX;
    [SerializeField] private GameObject[] _matchVFX;

    [SerializeField] private float _shakeForce;
    private Vector3 _fieldInitialPos;
    private Vector3 _backInitialPos;
    [SerializeField] private GameObject _fireworkEffectObject;
    private float _additionalTime = 0;
    private void Awake()
    {
        Instance = this;

        ActiveSprites = new Sprite[4];
        ActiveSprites[0] = _redSprites[Random.Range(0, _redSprites.Length)];
        ActiveSprites[1] = _yellowSprites[Random.Range(0, _yellowSprites.Length)];
        ActiveSprites[2] = _greenSprites[Random.Range(0, _greenSprites.Length)];
        ActiveSprites[3] = _blueSprites[Random.Range(0, _blueSprites.Length)];

        _fieldInitialPos = _fieldObject.localPosition;
        _backInitialPos = _backObject.localPosition;
        
        switch (PlayerPrefs.GetInt("Level", 1))
        {
            case 1:
            {
                _fieldHeight = 5;
                _fieldWidth = 5;

                _emptyNodes = 0;
                
                _fields.localPosition += new Vector3(128, 128, 0);
                _iceChance = 0;
                _cutCorners = false;


            }
                break;
            case 2:
            {
                _fieldHeight = 6;
                _fieldWidth = 6;
                _emptyNodes = 0;
                _iceChance = 0;
                _fields.localPosition += new Vector3(64, 64, 0);
                _cutCorners = false;
            }
                break;
            case 3:
            {
                _emptyNodes = 0;
                _iceChance = 0;
                _cutCorners = true;
            }
                break;
            case 4:
            {
                _iceChance = 0;
            }
                break;
        }
    }

    void Start()
    {
        GameStart();
    }

    private void GameStart()
    {
        fills = new int[_fieldWidth];
        _gameField = new Node[_fieldWidth, _fieldHeight];
        _updateHeight = new int[_fieldWidth];
        _update = new List<ItemController>();
        _switched = new List<SwitchedItems>();
        _dead = new List<ItemController>();
        _colorsInConnected = new Dictionary<ItemColor, List<Vector2Int>>
        {
            {ItemColor.Red, new List<Vector2Int>()},
            {ItemColor.Yellow, new List<Vector2Int>()},
            {ItemColor.Green, new List<Vector2Int>()},
            {ItemColor.Blue, new List<Vector2Int>()}
        };
        GenerateField();
        VerifyField();
        InstantiateField();
        GenerateOutline();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.F))
        {
            Fall();
        }

        List<ItemController> finishedUpdating = new List<ItemController>();
        foreach (var item in _update)
        {
            bool updating = item.UpdateItem();
            if (!updating)
                finishedUpdating.Add(item);
            _updatingFalling = true;
        }

        foreach (var item in finishedUpdating)
        {
            SwitchedItems switchedItems = GetSwitched(item);
            ItemController switchedIt = null;
            int x = item.PositionOnField.x;
            fills[x] = Mathf.Clamp(fills[x] - 1, 0, _fieldWidth);
            List<Vector2Int> connected = GetConnected(item.PositionOnField, true);
            bool wasSwitched = (switchedItems != null);
            ItemController lateDestroy = null;
            ItemController switchLateDestroy = null;
            ItemColor colorForFireWork = ItemColor.None;
            bool specialItemSetOff = false;
            bool superBomb = false;
            if (wasSwitched)
            {
                switchedIt = switchedItems.GetPairedItem(item);
                if (item.Color == ItemColor.Firework && switchedIt.Color == ItemColor.Bomb
                    || switchedIt.Color == ItemColor.Firework && item.Color == ItemColor.Bomb)
                {
                    specialItemSetOff = true;
                    superBomb = true;
                    if (item.Color == ItemColor.Bomb)  
                    {
                        lateDestroy = item;
                    }
                    else 
                    {
                        lateDestroy = switchedIt;
                    }
                }
                else if (item.Color == ItemColor.Firework || item.Color == ItemColor.Bomb)  
                {
                   // DestroyItemAtPos(item.PositionOnField, true, switchedIt);
                    // StartCoroutine(MakeItemsFall());
                    lateDestroy = item;
                    switchLateDestroy = switchedIt;
                    colorForFireWork = switchedIt.Color;
                    specialItemSetOff = true;
                }
                else if (switchedIt.Color == ItemColor.Firework || switchedIt.Color == ItemColor.Bomb)
                {
                    //SetOffFirework(item.Color);
                    //DestroyItemAtPos(switchedIt.PositionOnField, true, item);
                    //StartCoroutine(MakeItemsFall());
                    lateDestroy = switchedIt;
                    switchLateDestroy = item;
                    colorForFireWork = item.Color;
                    specialItemSetOff = true;
                }
                /*else if (item.Color == ItemColor.Bomb)
                {
                    DestroyItemAtPos(item.PositionOnField);//, true, switchedIt);
                    StartCoroutine(MakeItemsFall());
                }
                else if (switchedIt.Color == ItemColor.Bomb)
                {
                    DestroyItemAtPos(switchedIt.PositionOnField);// , true, item);
                    StartCoroutine(MakeItemsFall());
                }*/
                
                AddNode(ref connected, GetConnected(switchedIt.PositionOnField, true));
            }

            if (connected.Count == 0)
            {
                if (wasSwitched && !specialItemSetOff) 
                {
                    SwitchItems(item.PositionOnField, switchedIt.PositionOnField, false);
                }
            }
            else
            {
                
                SetSpecialItems(connected);
                   
                foreach (var pos in connected)
                {
                    DestroyItemAtPos(pos);
                }
                //StartCoroutine(MakeItemsFall());
                //Fall();
            }

            if (specialItemSetOff)
            {
                if (superBomb)
                {
                    SuperBomb(lateDestroy.PositionOnField);
                }
                else if (lateDestroy.Color == ItemColor.Bomb)
                {
                    // DestroyItemAtPos(item.PositionOnField, true, switchedIt);
                    // StartCoroutine(MakeItemsFall());
                    //SetOffBomb(lateDestroy.PositionOnField);
                    DestroyItemAtPos(lateDestroy.PositionOnField);
                }
                else if (lateDestroy.Color == ItemColor.Firework)
                {
                    //SetOffFirework(switchLateDestroy.Color);
                    DestroyItemAtPos(lateDestroy.PositionOnField, colorForFireWork);
                    //StartCoroutine(MakeItemsFall());
                    
                }
            }
            if (connected.Count > 0 || specialItemSetOff)
            {
                StartCoroutine(MakeItemsFall());
            }
            _switched.Remove(switchedItems);
            _update.Remove(item);
        }

        if (_updatingFalling && finishedUpdating.Count == 0)
        {
            _updatingFalling = false;
            var evt = GameEventsHandler.ItemsFallEvent;
            evt.IsFalling = false;
            EventManager.Broadcast(evt);
        }
    }

    private void DestroyItemAtPos(Vector2Int pos, ItemColor colorForFireWork = ItemColor.None, bool anim = true)
    {
        Node node = GetNodeAtPos(pos);
        ItemController nodeItem = node.GetItem();

        if (nodeItem != null)
        {
            if (!nodeItem.Destroying && !nodeItem.JustCreated)
            {
                nodeItem.Destroying = true;
                if (nodeItem.FireworkNext)
                {
                    nodeItem.Initialize(ItemColor.Firework, pos, _fireworkSprite);
                    nodeItem.JustCreated = true;
                    node.SetItem(nodeItem);
                    nodeItem.ShowComboText();
                    nodeItem.ShowFireworkTrail();
                }
                else if (nodeItem.BombNext)
                {
                    nodeItem.Initialize(ItemColor.Bomb, pos, _bombSprite);
                    nodeItem.JustCreated = true;
                    node.SetItem(nodeItem);
                    nodeItem.ShowComboText();
                    nodeItem.ShowFireworkTrail();
                    //EventManager.Broadcast(GameEventsHandler.SpecialItemCreateEvent);
                }
                else
                {
                    if (nodeItem.Color == ItemColor.Bomb)
                    {
                        SetOffBomb(pos);
                        anim = false;
                    }
                    else if (nodeItem.Color == ItemColor.Firework)
                    {
                        if (colorForFireWork != ItemColor.None)
                        {
                            SetOffFirework(colorForFireWork, nodeItem.PositionOnField);
                        }
                        else
                        {
                            SetOffFirework((ItemColor) Random.Range(1, 5),nodeItem.PositionOnField);
                        }
                        GameObject go = Instantiate(_fwVFX, _vfxObjectTop);
                        go.transform.localPosition = GetWorldPosAtPos(pos);
                        anim = false;
                    }

                    var evt = GameEventsHandler.ItemDestoyedEvent;
                    evt.Color = nodeItem.Color;
                    EventManager.Broadcast(evt);

                    if (!nodeItem.NoEffectDestroy && anim) ItemDestroyEffect(nodeItem);
                    nodeItem.gameObject.SetActive(false);
                    _dead.Add(nodeItem);
                    node.SetItem(null);
                }
                //if(nodeItem.Iced) 
            }
        }
        else
        {
            node.SetItem(null);
        }
    }

    private void SetOffFirework(ItemColor color, Vector2Int initialPos)
    {
        GameObject itemGO = Instantiate(_fireworkEffectObject, _vfxObjectBot.transform);
        itemGO.transform.localPosition = GetWorldPosAtPos(initialPos);
        itemGO.transform.rotation = Quaternion.Euler(0,0,-45);
        for (int i = 0; i < _fieldWidth; i++)
        {
            for (int j = 0; j < _fieldHeight; j++)
            {
                if (_gameField[i, j].Color == color)
                {
                    Vector2Int pos = new Vector2Int(i, j);
                    GameObject go = Instantiate(_fireworkEffectObject, _vfxObjectTop);
                    go.transform.localPosition = GetWorldPosAtPos(initialPos);
                    go.transform.up = GetWorldPosAtPos(pos) - go.transform.localPosition;
                    _additionalTime = 0.75f;
                    StartCoroutine(MoveFirework(go.transform, pos, _additionalTime));
                    
                    //DestroyItemAtPos(pos, ItemColor.None, false);
                }
            }
        }
        StartCoroutine(FireworkLateDestroy(itemGO, _additionalTime - 0.1f));
    }

    private IEnumerator FireworkLateDestroy(GameObject go, float time)
    {
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            yield return null;
        }
        Destroy(go);
        GameObject vfxGO = Instantiate(_fwVFX, _vfxObjectTop);
        vfxGO.transform.localPosition = go.transform.localPosition;
        StartCoroutine(FieldShake(1f));
    }
    private IEnumerator MoveFirework(Transform fireworkTransform, Vector2Int target, float time)
    {
        Vector3 initPos = fireworkTransform.localPosition;
        Vector3 targetPos = GetWorldPosAtPos(target);
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            fireworkTransform.localPosition = Vector3.Lerp(initPos, targetPos, t/time);
            yield return null;
        }
        Destroy(fireworkTransform.gameObject);
        GameObject go = Instantiate(_fwVFX, _vfxObjectTop);
        go.transform.localPosition = targetPos;
        DestroyItemAtPos(target, ItemColor.None, false);
    }
    IEnumerator MakeItemsFall()
    {
        var evt = GameEventsHandler.ItemsFallEvent;
        evt.IsFalling = true;
        EventManager.Broadcast(evt);
        yield return new WaitForSeconds(0.3f + _additionalTime);
        _additionalTime = 0;
        Fall();
    }

    private void SetSpecialItems(List<Vector2Int> connected)
    {
        foreach (var key in _colorsInConnected.Keys)
        {
            _colorsInConnected[key].Clear();
        }

        foreach (var pos in connected)
        {
            ItemColor checkColor = GetNodeColorAtPos(pos);
            if (checkColor == ItemColor.Bomb || checkColor == ItemColor.Firework|| checkColor <= 0)
            {
                /*foreach (var key in _colorsInConnected.Keys)
                {
                    _colorsInConnected[key].Add(pos);//kostyl'
                }*/
            }
            else
            {
                _colorsInConnected[checkColor].Add(pos);
            }
        }

        foreach (var key in _colorsInConnected.Keys)
        {
            if (_colorsInConnected[key].Count >= 5)
            {
                Vector2Int specialItemPos = new Vector2Int();
                foreach (var position in _colorsInConnected[key])
                {
                    ItemController item = GetNodeAtPos(position).GetItem();
                    if (item.Interacted)
                    {
                        item.Interacted = false;
                        item.FireworkNext = true;
                        specialItemPos = item.PositionOnField;
                        break;
                    }
                }

                SpecialItemCreateEffect(_colorsInConnected[key], specialItemPos);
                foreach (var position in _colorsInConnected[key])
                {
                    //DestroyItemAtPos(position, false, null, false);
                    GetNodeAtPos(position).GetItem().NoEffectDestroy = true;
                }
                //GetNodeAtPos(_colorsInConnected[key][Random.Range(0, _colorsInConnected[key].Count)]).GetItem()
                //.FireworkNext = true;
            }
            else if (_colorsInConnected[key].Count == 4)
            {
                Vector2Int specialItemPos = new Vector2Int();
                foreach (var position in _colorsInConnected[key])
                {
                    ItemController item = GetNodeAtPos(position).GetItem();
                    if (item.Interacted)
                    {
                        item.Interacted = false;
                        item.BombNext = true;
                        specialItemPos = item.PositionOnField;
                        break;
                    }
                }
                SpecialItemCreateEffect(_colorsInConnected[key], specialItemPos);
                foreach (var position in _colorsInConnected[key])
                {
                    //DestroyItemAtPos(position, false, null, false);
                    GetNodeAtPos(position).GetItem().NoEffectDestroy = true;
                }
                /*oreach (var position in _colorsInConnected[key])
                {
                    ItemController item = GetNodeAtPos(position).GetItem();
                    if (item.Interacted)
                    {
                        item.Interacted = false;
                    }
                    else
                    {
                        item.MovePositionTo(GetWorldPosAtPos(collapsePos));
                    }
                }*/

                //set  bomb
                //GetNodeAtPos(_colorsInConnected[key][Random.Range(0, _colorsInConnected[key].Count)]).GetItem()
                //   .BombNext = true;
            }
        }
    }

    private void ItemDestroyEffect(ItemController item)
    {
        /*ItemController go = Instantiate(_itemPrefab, _vfxObjectBot.transform);
        switch (item.Color)
        {
            case ItemColor.Bomb:
            {
                go.GetComponent<Image>().sprite = _bombSprite;
            }
                break;
            case ItemColor.Firework:
            {
                go.GetComponent<Image>().sprite = _fireworkSprite;
            }
                break;
            default:
            {
                go.GetComponent<Image>().sprite = ActiveSprites[(int) item.Color - 1];
            }
                break;
        }
        go.transform.localPosition = item.PositionInWorld;
        go.Effect = true;
        go.GetComponent<Animator>().SetTrigger("Destroy");
        GameObject vfxgo = Instantiate(_matchVFX, _vfxObjectTop);
        vfxgo.transform.localPosition = item.PositionInWorld;
        //Vector3 pos = item.GetComponent<RectTransform>().anchoredPosition;*/
        if (item.Color != ItemColor.Bomb || item.Color != ItemColor.Firework)
        {
            GameObject vfxgo = Instantiate(_matchVFX[(int) item.Color - 1], _vfxObjectTop);
            vfxgo.transform.localPosition = item.PositionInWorld;
        }
    }
    private void SpecialItemCreateEffect(List<Vector2Int> itemsPos, Vector2Int specialItemPos)
    {
        foreach (var pos in itemsPos)
        {
            ItemController go = Instantiate(_itemPrefab, _vfxObjectBot.transform);
            go.GetComponent<Image>().sprite = ActiveSprites[(int) GetNodeColorAtPos(pos) - 1];
            go.transform.localPosition = GetWorldPosAtPos(pos);
            go.Effect = true;
            //go.MovePositionTo(GetWorldPosAtPos(specialItemPos));
            StartCoroutine(MoveItemEffect(go, GetWorldPosAtPos(specialItemPos)));
        }
    }

    private IEnumerator MoveItemEffect(ItemController item, Vector3 pos)
    {
        while (Vector3.Distance(item.transform.localPosition, pos) > 2f)
        {
            item.MovePositionToWithSpeed(pos, 24f);
            yield return null;
        }
        Destroy(item.gameObject);
    }
    private void SetOffBomb(Vector2Int pos)
    {
        Instantiate(_explVFX, _vfxObjectTop).transform.localPosition = GetWorldPosAtPos(pos);
        StartCoroutine(FieldShake(1f));
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                Vector2Int lookingDir = pos + Vector2Int.right * i + Vector2Int.up * j;
                if (lookingDir.x < 0 || lookingDir.x >= _fieldWidth || lookingDir.y < 0 ||
                    lookingDir.y >= _fieldHeight || GetNodeColorAtPos(lookingDir)==ItemColor.Abyss  /*|| lookingDir == pos*/) continue;
                else
                {
                    /*Node node = GetNodeAtPos(lookingDir);
                    ItemController nodeItem = node.GetItem();
                    if (nodeItem != null)
                    {
                        nodeItem.gameObject.SetActive(false);
                        _dead.Add(nodeItem);
                    }
   
                    node.SetItem(null);*/
                    DestroyItemAtPos(lookingDir, ItemColor.None, false);
                }
            }
        }
    }

    private IEnumerator FieldShake(float time)
    {
       // Vector3 initialPosField = _fieldObject.localPosition;
        //Vector3 initialPosBack = _backObject.localPosition;
        float shake = _shakeForce;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            Vector3 shakeDir = Random.insideUnitSphere * shake;
            _fieldObject.localPosition = shakeDir + _fieldInitialPos;
            _backObject.localPosition = shakeDir + _backInitialPos;
            shake = _shakeForce * (1 - t / time);
            yield return null;
        }

       
        _fieldObject.localPosition = _fieldInitialPos;
        _backObject.localPosition = _backInitialPos;
    }
    private void SuperBomb(Vector2Int pos)
    {
        Instantiate(_bigExplVFX, _vfxObjectTop).transform.localPosition = GetWorldPosAtPos(pos);
        StartCoroutine(FieldShake(1f));
        for (int i = -2; i <= 2; i++)
        {
            for (int j = -2; j <= 2; j++)
            {
                Vector2Int lookingDir = pos + Vector2Int.right * i + Vector2Int.up * j;
                if (lookingDir.x < 0 || lookingDir.x >= _fieldWidth || lookingDir.y < 0 ||
                    lookingDir.y >= _fieldHeight || GetNodeColorAtPos(lookingDir)==ItemColor.Abyss  /*|| lookingDir == pos*/) continue;
                else
                {
                    /*Node node = GetNodeAtPos(lookingDir);
                    ItemController nodeItem = node.GetItem();
                    if (nodeItem != null)
                    {
                        nodeItem.gameObject.SetActive(false);
                        _dead.Add(nodeItem);
                    }
   
                    node.SetItem(null);*/
                    DestroyItemAtPos(lookingDir, ItemColor.None, false);
                }
            }
        }
    }
    /*ItemController GetSwitched(ItemController item)
    {
        ItemController it = null;
        foreach (var sw in _switched)
        {
            it = sw.GetPairedItem(item);
            if (it != null) break;
        }

        return it;
    }*/

    SwitchedItems GetSwitched(ItemController item)
    {
        SwitchedItems swit = null;
        for (int i = 0; i < _switched.Count; i++)
        {
            if (_switched[i].GetPairedItem(item) != null)
            {
                swit = _switched[i];
                break;
            }
        }

        return swit;
    }

    private void Fall()
    {
        for (int i = 0; i < _fieldWidth; i++)
        {
            for (int j = 0; j < _fieldHeight; j++)
            {
                Vector2Int pos = new Vector2Int(i, j);
                Node node = GetNodeAtPos(pos);
                ItemColor color = GetNodeColorAtPos(pos);
                if (color != ItemColor.None) continue;
                for (int nj = j; nj <= _fieldHeight; nj++)
                {
                    Vector2Int next = new Vector2Int(i, nj);
                    ItemColor nextColor = GetNodeColorAtPos(next);
                    if (nextColor == ItemColor.None) continue;
                    if (nj < _fieldHeight && GetNodeAtPos(next).IsIced())
                    {
                        j = nj;
                        break;
                    }
                    if (nextColor != ItemColor.Abyss)
                    {
                        Node got = GetNodeAtPos(next);
                        ItemController item = got.GetItem();
                        node.SetItem(item);
                        _update.Add(item);
                        got.SetItem(null);
                    }
                    else
                    {
                        ItemColor newColor = (ItemColor) Random.Range(1, 5);
                        ItemController item;
                        Vector2Int fallPoint = new Vector2Int(i, _fieldHeight + fills[i]);
                        if (_dead.Count > 0)
                        {
                            ItemController revived = _dead[0];
                            revived.gameObject.SetActive(true);
                            revived.transform.localPosition = GetWorldPosAtPos(fallPoint);
                            item = revived;

                            _dead.RemoveAt(0);
                        }
                        else
                        {
                            ItemController go = Instantiate(_itemPrefab, _fieldObject);
                            go.transform.localPosition = GetWorldPosAtPos(fallPoint);
                            item = go;
                            //go.Initialize(newColor,pos, _activeSprites[(int)newColor - 1]);
                        }

                        item.Initialize(newColor, pos, ActiveSprites[(int) newColor - 1]);
                        Node hole = GetNodeAtPos(pos);
                        hole.SetItem(item);
                        ResetItem(item);
                        fills[i]++;
                    }

                    break;
                }
            }
        }

       // var evt = GameEventsHandler.ItemsFallEvent;
       // evt.IsFalling = false;
       // EventManager.Broadcast(evt);
    }

    private void VerifyField()
    {
        List<ItemColor> remove;
        for (int i = 0; i < _fieldWidth; i++)
        {
            for (int j = 0; j < _fieldHeight; j++)
            {
                Vector2Int pos = new Vector2Int(i, j);
                ItemColor color = GetNodeColorAtPos(pos);
                if (color <= 0)
                {
                    continue;
                }

                remove = new List<ItemColor>();
                while (GetConnected(pos, true, true).Count > 0)
                {
                    if (!remove.Contains(color))
                        remove.Add(color);
                    SetNodeColorAtPos(pos, GetNewColor(ref remove));
                }
            }
        }
    }

    ItemColor GetNewColor(ref List<ItemColor> remove)
    {
        List<ItemColor> available = new List<ItemColor>();
        for (int i = 0; i < ActiveSprites.Length; i++)
        {
            available.Add((ItemColor) (i + 1));
        }

        foreach (var i in remove)
        {
            available.Remove(i);
        }

        if (available.Count <= 0) return ItemColor.None;
        return available[Random.Range(0, available.Count)];
    }

    private List<Vector2Int> GetConnected(Vector2Int pos, bool main, bool init = false)
    {
        ItemColor color = GetNodeColorAtPos(pos);
        List<Vector2Int> connected = new List<Vector2Int>();
        if (color <= ItemColor.None) return connected;
        Vector2Int[] directions = {Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left};
        foreach (var dir in directions)
        {
            List<Vector2Int> line = new List<Vector2Int>();
            int matches = 0;
            for (int i = 1; i < 3; i++)
            {
                Vector2Int next = pos + (dir * i);
                ItemColor checkColor = GetNodeColorAtPos(next);
                if (checkColor == color)// || checkColor == ItemColor.Bomb || checkColor == ItemColor.Firework || 
                   // (color == ItemColor.Bomb || color == ItemColor.Firework) && checkColor > ItemColor.None)
                {
                    matches++;
                    line.Add(next);
                }
            }

            if (matches > 1)
            {
                AddNode(ref connected, line);
                if (main && !init)
                    GetNodeAtPos(pos).GetItem().Interacted = true;
            }
        }

        for (int i = 0; i < 2; i++)
        {
            List<Vector2Int> line = new List<Vector2Int>();
            int matches = 0;
            Vector2Int next1 = pos + directions[i];
            Vector2Int next2 = pos + directions[i + 2];
            ItemColor checkColor1 = GetNodeColorAtPos(next1);
            if (checkColor1 == color )//|| checkColor1 == ItemColor.Bomb || checkColor1 == ItemColor.Firework || 
                //(color == ItemColor.Bomb || color == ItemColor.Firework) && checkColor1 > ItemColor.None)
            {
                line.Add(next1);
                matches++;
            }

            ItemColor checkColor2 = GetNodeColorAtPos(next2);
            if (checkColor2 == color)// || checkColor2 == ItemColor.Bomb || checkColor2 == ItemColor.Firework || 
                //(color == ItemColor.Bomb || color == ItemColor.Firework) && checkColor2 > ItemColor.None)
            {
                line.Add(next2);
                matches++;
            }

            if (matches > 1)
            {
                AddNode(ref connected, line);
                if (main && !init)
                    GetNodeAtPos(pos).GetItem().Interacted = true;
            }
        }

        if (main)
        {
            for (int i = 0; i < connected.Count; i++)
            {
                //if (GetNodeColorAtPos(connected[i]) != ItemColor.None)
                AddNode(ref connected, GetConnected(connected[i], false, init));
            }
        }
/*
        if (connected.Count > 0)
        {
            connected.Add(pos);
        }*/

        return connected;
    }

    private void AddNode(ref List<Vector2Int> nodes, List<Vector2Int> addition)
    {
        foreach (var pos in addition)
        {
            bool add = true;
            foreach (var nodePos in nodes)
            {
                if (nodePos.Equals(pos))
                {
                    add = false;
                    break;
                }
            }

            if (add)
            {
                nodes.Add(pos);
            }
        }
    }

    public void ResetItem(ItemController item)
    {
        item.ResetPosition();
        //item.Switched = null;
        _update.Add(item);
    }

    public void SwitchItems(Vector2Int first, Vector2Int second, bool main)
    {
        if (GetNodeColorAtPos(first) <= ItemColor.None || GetNodeAtPos(first).GetItem().Iced) return;
        Node firstNode = GetNodeAtPos(first);
        ItemController firstItem = firstNode.GetItem();

        if (GetNodeColorAtPos(second) > ItemColor.None && !GetNodeAtPos(second).GetItem().Iced)
        {
            Node secondNode = GetNodeAtPos(second);
            ItemController secondItem = secondNode.GetItem();
            firstNode.SetItem(secondItem);
            secondNode.SetItem(firstItem);
            if (main)
                _switched.Add(new SwitchedItems(firstItem, secondItem));
            // firstItem.Switched = secondItem;
            //secondItem.Switched = firstItem;
            _update.Add(firstItem);
            _update.Add(secondItem);
        }
        else
        {
            ResetItem(firstItem);
        }
    }

    private ItemColor GetNodeColorAtPos(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= _fieldWidth || pos.y < 0 || pos.y >= _fieldHeight) return ItemColor.Abyss;
        return _gameField[pos.x, pos.y].Color;
    }

    public Vector3 GetWorldPosAtPos(Vector2Int pos)
    {
        return new Vector3(_itemWidth / 2 + _itemWidth * pos.x, _itemHeight / 2 + _itemHeight * pos.y,
            transform.position.z); //new Vector3(PositionOnField.x, PositionOnField.y, PositionInWorld.z);
    }

    private Node GetNodeAtPos(Vector2Int pos)
    {
        return _gameField[pos.x, pos.y];
    }

    private void SetNodeColorAtPos(Vector2Int pos, ItemColor color)
    {
        _gameField[pos.x, pos.y].Color = color;
    }

    private void GenerateField()
    {
        int[,] genField = new int[_fieldHeight + 2, _fieldWidth + 2];
        _emptyNodes = Random.Range(_emptyNodes / 2, _emptyNodes);
        for (int i = 1; i < _fieldWidth + 1; i++)
        {
            genField[0, i] = 1;
            genField[_fieldHeight + 1, i] = 1;
        }

        for (int i = 1; i < _fieldHeight + 1; i++)
        {
            genField[i, 0] = 1;
            genField[i, _fieldWidth + 1] = 1;
        }

        List<Point> availablePoints = new List<Point>
        {
            new Point(1, 1),
            new Point(_fieldHeight, _fieldWidth),
            new Point(1, _fieldWidth),
            new Point(_fieldHeight, 1)
        };
        for (int i = 0; i < _emptyNodes; i++)
        {
            int randNum = Random.Range(0, availablePoints.Count);
            Point node = availablePoints[randNum];
            availablePoints.RemoveAt(randNum);
            genField[node.x, node.y] = 1;
            Point left = new Point(node.x - 1, node.y);
            Point right = new Point(node.x + 1, node.y);
            Point up = new Point(node.x, node.y - 1);
            Point down = new Point(node.x, node.y + 1);
            if (GenerateCheckNeighbours(genField, left)) availablePoints.Add(left);
            if (GenerateCheckNeighbours(genField, right)) availablePoints.Add(right);
            if (GenerateCheckNeighbours(genField, up)) availablePoints.Add(up);
            if (GenerateCheckNeighbours(genField, down)) availablePoints.Add(down);
        }

        for (int i = 1; i < _fieldWidth + 1; i++)
        {
            for (int j = 1; j < _fieldHeight + 1; j++)
            {
                if (genField[i, j] == 1)
                {
                    _gameField[i - 1, j - 1] = new Node(ItemColor.Abyss, new Vector2Int(i - 1, j - 1));
                }
                else
                {
                    _gameField[i - 1, j - 1] = new Node((ItemColor) Random.Range(1, 5), new Vector2Int(i - 1, j - 1));
                }
            }
        }

        if (_cutCorners)
        {
            _gameField[_fieldWidth -1, _fieldHeight -1].Color = ItemColor.Abyss;
            _gameField[_fieldWidth -2, _fieldHeight -1].Color = ItemColor.Abyss;
            _gameField[_fieldWidth -1, _fieldHeight -2].Color = ItemColor.Abyss;
            _gameField[0, _fieldHeight -1].Color = ItemColor.Abyss;
            _gameField[1, _fieldHeight -1].Color = ItemColor.Abyss;
            _gameField[0, _fieldHeight -2].Color = ItemColor.Abyss;
        }
    }

    private void InstantiateField()
    {
        bool bombLevel = PlayerPrefs.GetInt("Level", 1) == 3;
        bool fireworkLevel = PlayerPrefs.GetInt("Level", 1) == 4;
        //bool iceLevel = PlayerPrefs.GetInt("Level", 1) == 5;
        bool checkered = false;
        for (int i = 0; i < _fieldWidth; i++)
        {
            for (int j = 0; j < _fieldHeight; j++)
            {
                Node node = _gameField[i, j];
                if (node.Color > ItemColor.None)
                {
                    ItemController go = Instantiate(_itemPrefab, _fieldObject);
                    go.transform.localPosition = new Vector3(_itemWidth / 2 + _itemWidth * i,
                        _itemHeight / 2 + _itemHeight * j, 0);
                    
                    go.Initialize(node.Color, new Vector2Int(i, j), ActiveSprites[(int) node.Color - 1]);
                    node.SetItem(go);
                    if (Random.value < _iceChance)
                    {
                        go.ToggleIce(true);
                        //Instantiate(_icePrefab, go.transform);
                        _updateHeight[i] = j;
                    }

                    GameObject tileGO;
                    if (checkered)
                        tileGO = Instantiate(_fieldDarkTilePrefab, _backObject);
                    else
                        tileGO = Instantiate(_fieldLightTilePrefab, _backObject);
                    tileGO.transform.localPosition = 
                        new Vector3(_itemWidth/2 + _itemWidth * i,_itemHeight/2 + _itemHeight*j, 0);
                    
                }
                checkered = !checkered;
            }
        }
        if (bombLevel)
        {
            ItemController item = _gameField[3,3].GetItem();
            item.Initialize(ItemColor.Bomb, new Vector2Int(3, 3), _bombSprite);
            _gameField[3,3].SetItem(item);
        }
        else if(fireworkLevel)
        {
            ItemController item = _gameField[3,3].GetItem();
            item.Initialize(ItemColor.Firework, new Vector2Int(3, 3), _fireworkSprite);
            _gameField[3,3].SetItem(item);
        }
    }

    bool GenerateCheckNeighbours(int[,] field, Point node)
    {
        if (field[node.x, node.y] == 1) return false;
        else
        {
            int lim = 0;
            if (field[node.x - 1, node.y] == 1) lim++;
            if (field[node.x + 1, node.y] == 1) lim++;
            if (field[node.x, node.y - 1] == 1) lim++;
            if (field[node.x, node.y + 1] == 1) lim++;
            return lim >= 2;
        }
    }
    void GenerateOutline()
    {
        int row = 0;
        int col = 0;
        for (row = 0; row < _fieldHeight; row++)
        { //down
            SetOutline(new Vector2Int(col,row), 0);//left
        }
        row = _fieldHeight - 1;
        for (col = 0; col < _fieldWidth; col++)
        { //right
            SetOutline(new Vector2Int(col,row), 90);
        }
        col = _fieldWidth - 1;
        for (row = _fieldHeight - 1; row >= 0; row--)
        { //up
            SetOutline(new Vector2Int(col,row), 180);
        }
        row = 0;
        for (col = _fieldWidth - 1; col >= 0; col--)
        { //left
            SetOutline(new Vector2Int(col,row), 270);
        }
        col = 0;
        for (row = 1; row < _fieldHeight - 1; row++)
        {
            for (col = 1; col < _fieldWidth - 1; col++)
            {
                //  if (GetSquare(col, row).type == SquareTypes.NONE)
                SetOutline(new Vector2Int(col,row), 0);
            }
        }
    }
    void SetOutline(Vector2Int pos, int zRot)
    {
        int row = pos.y;
        int col = pos.x;
        int maxCols = _fieldWidth;
        int maxRows = _fieldHeight;
        ItemColor nodeColor = GetNodeColorAtPos(pos);
        if (nodeColor != ItemColor.Abyss)
        {
            if (pos.x == 0 || pos.y == 0 || pos.x == _fieldWidth - 1 || pos.y == _fieldHeight - 1)
            {
                GameObject outline =  Instantiate(_outlinePrefab, _backObject);
                outline.name = "outline " + row.ToString() + col.ToString();
                //SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                outline.transform.localRotation = Quaternion.Euler(0, 0, zRot);
                if (zRot == 0)
                    outline.transform.localPosition = GetWorldPosAtPos(pos) + Vector3.left * 63.5f;//* 0.425f;
                if (zRot == 90)
                    outline.transform.localPosition = GetWorldPosAtPos(pos) + Vector3.up * 63.5f;//* 0.425f;
                if (zRot == 180)
                    outline.transform.localPosition = GetWorldPosAtPos(pos) + Vector3.right * 63.5f;//* 0.425f;
                if (zRot == 270)
                    outline.transform.localPosition = GetWorldPosAtPos(pos) + Vector3.down * 63.5f;//* 0.425f;
                if (pos.x == 0 && pos.y == 0)
                {   //top left//bottom left
                    Image img = outline.GetComponent<Image>();
                    img.sprite = _outlineSprites[2];
                    img.SetNativeSize();
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 270);//180
                    outline.transform.localPosition = GetWorldPosAtPos(pos) + Vector3.left * 1.5f + Vector3.down * 1.5f;
                }
                if (row == 0 && col == maxCols - 1)
                {   //bottom right
                    Image img = outline.GetComponent<Image>();
                    img.sprite = _outlineSprites[2];
                    img.SetNativeSize();
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 0);//90
                    outline.transform.localPosition = GetWorldPosAtPos(pos) + Vector3.right * 1.5f + Vector3.down * 1.5f;
                }
                if (row == maxRows - 1 && col == 0)
                {   //top left
                    Image img = outline.GetComponent<Image>();
                    img.sprite = _outlineSprites[2];
                    img.SetNativeSize();
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 180);//-90
                    outline.transform.localPosition = GetWorldPosAtPos(pos) + Vector3.left * 1.5f + Vector3.up * 1.5f;
                }
                if (row == maxRows - 1 && col == maxCols - 1)
                {   //top right
                    Image img = outline.GetComponent<Image>();
                    img.sprite = _outlineSprites[2];
                    img.SetNativeSize();
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 90);//0
                    outline.transform.localPosition = GetWorldPosAtPos(pos) + Vector3.right * 1.5f + Vector3.up * 1.5f;
                }
            }
            else
            {
                //top left//bottom
                if (GetNodeColorAtPos(new Vector2Int(col - 1, row - 1)) == ItemColor.Abyss 
                    && GetNodeColorAtPos(new Vector2Int(col, row - 1)) == ItemColor.Abyss 
                    && GetNodeColorAtPos(new Vector2Int(col - 1, row)) == ItemColor.Abyss)
                {
                    //GameObject outline = CreateOutline(square);
                    GameObject outline =  Instantiate(_outlinePrefab, _backObject);
                    outline.name = "outline " + row.ToString() + col.ToString();
                    Image img = outline.GetComponent<Image>();
                    img.sprite = _outlineSprites[2];
                    img.SetNativeSize();
                    outline.transform.localPosition = GetWorldPosAtPos(pos) + Vector3.left * 1.5f + Vector3.down * 1.5f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 270);//180
                }
                //top right//bottom
                if (GetNodeColorAtPos(new Vector2Int(col + 1, row - 1)) == ItemColor.Abyss 
                     && GetNodeColorAtPos(new Vector2Int(col, row - 1)) == ItemColor.Abyss 
                     && GetNodeColorAtPos(new Vector2Int(col + 1, row)) == ItemColor.Abyss)
                {
                    GameObject outline =  Instantiate(_outlinePrefab, _backObject);
                    outline.name = "outline " + row.ToString() + col.ToString();
                    Image img = outline.GetComponent<Image>();
                    img.sprite = _outlineSprites[2];
                    img.SetNativeSize();
                    outline.transform.localPosition = GetWorldPosAtPos(pos) + Vector3.right * 1.5f + Vector3.down * 1.5f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 0);//90
                }
                //bottom left//top
                if (GetNodeColorAtPos(new Vector2Int(col - 1, row + 1)) == ItemColor.Abyss 
                    && GetNodeColorAtPos(new Vector2Int(col, row + 1)) == ItemColor.Abyss 
                    && GetNodeColorAtPos(new Vector2Int(col - 1, row)) == ItemColor.Abyss)
                {
                    GameObject outline =  Instantiate(_outlinePrefab, _backObject);
                    outline.name = "outline " + row.ToString() + col.ToString();
                    Image img = outline.GetComponent<Image>();
                    img.sprite = _outlineSprites[2];
                    img.SetNativeSize();
                    outline.transform.localPosition = GetWorldPosAtPos(pos) + Vector3.left * 1.5f + Vector3.up * 1.5f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 180);//270
                }
                //bottom right//top
                if (GetNodeColorAtPos(new Vector2Int(col + 1, row + 1)) == ItemColor.Abyss 
                    && GetNodeColorAtPos(new Vector2Int(col, row + 1)) == ItemColor.Abyss 
                    && GetNodeColorAtPos(new Vector2Int(col + 1, row)) == ItemColor.Abyss)
                {
                    GameObject outline =  Instantiate(_outlinePrefab, _backObject);
                    outline.name = "outline " + row.ToString() + col.ToString();
                    Image img = outline.GetComponent<Image>();
                    img.sprite = _outlineSprites[2];
                    img.SetNativeSize();
                    outline.transform.localPosition = GetWorldPosAtPos(pos) + Vector3.right * 1.5f + Vector3.up * 1.5f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 90);//0
                }


            }
        }
        else
        {
            bool corner = false;
            if (GetNodeColorAtPos(new Vector2Int(col - 1, row)) != ItemColor.Abyss 
                && GetNodeColorAtPos(new Vector2Int(col, row-1)) != ItemColor.Abyss)
            {
                GameObject outline =  Instantiate(_outlinePrefab, _backObject);
                outline.name = "outline " + row.ToString() + col.ToString();
                Image img = outline.GetComponent<Image>();
                img.sprite = _outlineSprites[1];
                img.SetNativeSize();
                outline.transform.localPosition = GetWorldPosAtPos(pos);
                outline.transform.localRotation = Quaternion.Euler(0, 0, 90);//0
                corner = true;
            }
            if (GetNodeColorAtPos(new Vector2Int(col + 1, row)) != ItemColor.Abyss 
                && GetNodeColorAtPos(new Vector2Int(col, row+1)) != ItemColor.Abyss)
            {
                GameObject outline =  Instantiate(_outlinePrefab, _backObject);
                outline.name = "outline " + row.ToString() + col.ToString();
                Image img = outline.GetComponent<Image>();
                img.sprite = _outlineSprites[1];
                img.SetNativeSize();
                outline.transform.localPosition = GetWorldPosAtPos(pos);
                outline.transform.localRotation = Quaternion.Euler(0, 0, 270);//180
                corner = true;
            }
            if (GetNodeColorAtPos(new Vector2Int(col + 1, row)) != ItemColor.Abyss 
                && GetNodeColorAtPos(new Vector2Int(col, row-1)) != ItemColor.Abyss)
            {
                GameObject outline =  Instantiate(_outlinePrefab, _backObject);
                outline.name = "outline " + row.ToString() + col.ToString();
                Image img = outline.GetComponent<Image>();
                img.sprite = _outlineSprites[1];
                img.SetNativeSize();
                outline.transform.localPosition = GetWorldPosAtPos(pos);
                outline.transform.localRotation = Quaternion.Euler(0, 0, 180);//270
                corner = true;
            }
            if (GetNodeColorAtPos(new Vector2Int(col - 1, row)) != ItemColor.Abyss 
                && GetNodeColorAtPos(new Vector2Int(col, row+1)) != ItemColor.Abyss)
            {
                GameObject outline =  Instantiate(_outlinePrefab, _backObject);
                outline.name = "outline " + row.ToString() + col.ToString();
                Image img = outline.GetComponent<Image>();
                img.sprite = _outlineSprites[1];
                img.SetNativeSize();
                outline.transform.localPosition = GetWorldPosAtPos(pos);
                outline.transform.localRotation = Quaternion.Euler(0, 0, 0);//90
                corner = true;
            }


            if (!corner)
            {
                if (GetNodeColorAtPos(new Vector2Int(col, row-1)) != ItemColor.Abyss)
                {
                    GameObject outline =  Instantiate(_outlinePrefab, _backObject);
                    outline.name = "outline " + row.ToString() + col.ToString();
                    outline.transform.localPosition = GetWorldPosAtPos(pos) + Vector3.down * 63.5f;// 0.395f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 90);
                }
                if (GetNodeColorAtPos(new Vector2Int(col, row+1)) != ItemColor.Abyss)
                {
                    GameObject outline =  Instantiate(_outlinePrefab, _backObject);
                    outline.name = "outline " + row.ToString() + col.ToString();
                    outline.transform.localPosition = GetWorldPosAtPos(pos) + Vector3.up * 63.5f;//0.395f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 90);
                }
                if (GetNodeColorAtPos(new Vector2Int(col - 1, row)) != ItemColor.Abyss)
                {
                    GameObject outline =  Instantiate(_outlinePrefab, _backObject);
                    outline.name = "outline " + row.ToString() + col.ToString();
                    outline.transform.localPosition = GetWorldPosAtPos(pos) + Vector3.left * 63.5f;//0.395f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
                if (GetNodeColorAtPos(new Vector2Int(col + 1, row)) != ItemColor.Abyss)
                {
                    GameObject outline =  Instantiate(_outlinePrefab, _backObject);
                    outline.name = "outline " + row.ToString() + col.ToString();
                    outline.transform.localPosition = GetWorldPosAtPos(pos) + Vector3.right * 63.5f;//0.395f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
            }
        }

    }
}