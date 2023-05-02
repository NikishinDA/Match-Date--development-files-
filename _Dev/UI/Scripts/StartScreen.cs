using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
    [SerializeField] Button _startButton;

    [SerializeField] private InputField _maxTurnsInput;
    [SerializeField] private InputField _targetInput;

    private int _maxTurns = 20;
    private int _target = 35;
    private void Awake()
    {
        _startButton.onClick.AddListener(OnStartButtonPress);
        _maxTurnsInput.onValueChanged.AddListener(OnInputValuesChange);
        _targetInput.onValueChanged.AddListener(OnInputValuesChange);
    }
    private void OnStartButtonPress()
    {
        EventManager.Broadcast(GameEventsHandler.GameStartEvent);
        gameObject.SetActive(false);

        var evt = GameEventsHandler.DebugEvent;
        evt.Target = _target;
        evt.MaxTurns = _maxTurns;
        EventManager.Broadcast(evt);
    }

    private void OnInputValuesChange(string s)
    {

        if (_targetInput.text != "")
        {
            _target = Convert.ToInt32(_targetInput.text);
        }

        if (_maxTurnsInput.text != "")
        {
            _maxTurns = Convert.ToInt32(_maxTurnsInput.text);
        }
    }
}
