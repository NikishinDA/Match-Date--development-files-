using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DesireIndicator : MonoBehaviour
{
    private Sprite[] _activeSprites;
    [SerializeField] private Image _displayImg;
    [SerializeField] private Slider _sliderProgress;
    [SerializeField] private int _matchesNeeded;
    private ItemColor _color;
    private int _stage = 0;
    private float _progress = 0;

    [SerializeField] private Image[] _stagesImages;
    [SerializeField] private ParticleSystem _stageEffect;
    [SerializeField] private GameObject _nextStageText;
    private void Awake()
    {
        EventManager.AddListener<ItemDestoyedEvent>(OnItemDestroyed);
        EventManager.AddListener<DebugEvent>(OnDebugCall);
    }
    private void OnDestroy()
    {
        EventManager.RemoveListener<ItemDestoyedEvent>(OnItemDestroyed);
        EventManager.RemoveListener<DebugEvent>(OnDebugCall);
    }

    private void OnDebugCall(DebugEvent obj)
    {
        _matchesNeeded = obj.Target;
    }

    private void OnEnable()
    {
        _activeSprites = Match3.Instance.ActiveSprites;
        _displayImg.sprite = _activeSprites[0];
        _color = ItemColor.Red;
    }

    private void OnItemDestroyed(ItemDestoyedEvent obj)
    {
        if (_color == obj.Color)
        {
            UpdateProgress();
        }
    }

    private void NextItem()
    {
        _displayImg.sprite = _activeSprites[_stage];
        _color = (ItemColor) _stage + 1;
    }
    
    private void UpdateProgress()
    {
        _progress += (float)1 / _matchesNeeded;
        if (_progress >= 0.25f && _stage == 0)
        {
            
            _stagesImages[_stage].color = Color.yellow;
            Instantiate(_stageEffect, _stagesImages[_stage].transform);
            _stage++;
            NextItem();
            BroadcastStageChangeEvent(_stage);
            _nextStageText.SetActive(true);
        }
        else if(_progress >= 0.50f && _stage == 1)
        {
            
            _stagesImages[_stage].color = Color.yellow;
            Instantiate(_stageEffect, _stagesImages[_stage].transform);
            _stage++;
            NextItem();
            BroadcastStageChangeEvent(_stage);
            _nextStageText.SetActive(true);

        }
        else if(_progress >= 0.75f && _stage == 2)
        {
            
            _stagesImages[_stage].color = Color.yellow;
            Instantiate(_stageEffect, _stagesImages[_stage].transform);
            _stage++;
            NextItem();
            BroadcastStageChangeEvent(_stage);
            _nextStageText.SetActive(true);

        }
        else if (_progress > 1 && _stage == 3)
        {
            _progress = 1;
            _stage++;
            BroadcastStageChangeEvent(_stage);
            var evt = GameEventsHandler.GameOverEvent;
            evt.IsWin = true;
            EventManager.Broadcast(evt);
            _nextStageText.SetActive(true);

        }
        //_sliderProgress.value = _progress;
    }

    private void BroadcastStageChangeEvent(int stage)
    {
        var evt = GameEventsHandler.StageChangeEvent;
        evt.Stage = stage;
        EventManager.Broadcast(evt);
    }
    private void Update()
    {
        _sliderProgress.value = Mathf.Lerp(_sliderProgress.value, _progress, Time.deltaTime);
    }
}
