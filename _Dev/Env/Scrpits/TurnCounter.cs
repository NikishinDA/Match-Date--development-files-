using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnCounter : MonoBehaviour
{
    [SerializeField] private Text _counterText;
    [SerializeField] private int _numOfTurns;
    [SerializeField] private Image _back;

    private int _stage = 0;
    private void Awake()
    {
        EventManager.AddListener<TurnMadeEvent>(OnTurnMade);
        EventManager.AddListener<StageChangeEvent>(OnStageChange);
        EventManager.AddListener<DebugEvent>(OnDebugCall);
    }
    private void OnDestroy()
    {
        EventManager.RemoveListener<TurnMadeEvent>(OnTurnMade);
        EventManager.RemoveListener<StageChangeEvent>(OnStageChange);
        EventManager.RemoveListener<DebugEvent>(OnDebugCall);
    }
    private void OnDebugCall(DebugEvent obj)
    {
        _numOfTurns = obj.MaxTurns;
    }
    private void OnStageChange(StageChangeEvent obj)
    {
        _stage = obj.Stage;
    }

    private void OnTurnMade(TurnMadeEvent obj)
    {
        _numOfTurns--;
        _counterText.text = _numOfTurns.ToString();
        if (_numOfTurns <= 0)
        {
            var evt = GameEventsHandler.GameOverEvent;
            if (_stage == 0)
            {
                
                evt.IsWin = false;
            }
            else
            {
                evt.IsWin = true;
            }
            EventManager.Broadcast(evt);
        }

        if (_numOfTurns == 5)
        {
            _back.color = Color.red;
            _counterText.color = Color.red;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _counterText.text = _numOfTurns.ToString();
    }
}
