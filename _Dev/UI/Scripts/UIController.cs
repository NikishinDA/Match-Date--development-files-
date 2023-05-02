using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] GameObject _loseScreen;
    [SerializeField] GameObject _winScreen;
    [SerializeField] GameObject _startScreen;
    [SerializeField] GameObject _overlay;

    private int _stage;

    private void Awake()
    {
        EventManager.AddListener<GameOverEvent>(OnGameOver);
        EventManager.AddListener<GameStartEvent>(OnGameStart);
        EventManager.AddListener<StageChangeEvent>(OnStageChange);
    }
    private void OnDestroy()
    {
        EventManager.RemoveListener<GameOverEvent>(OnGameOver);
        EventManager.RemoveListener<GameStartEvent>(OnGameStart);
        EventManager.RemoveListener<StageChangeEvent>(OnStageChange);


    }
    private void Start()
    {
        _startScreen.SetActive(true);
    }
    private void OnStageChange(StageChangeEvent obj)
    {
        _stage = obj.Stage;
    }
    private void OnGameOver(GameOverEvent obj)
    {
        _overlay.SetActive(false);
        if (obj.IsWin)
        {
            //_winScreen.SetActive(true);
            _winScreen.GetComponent<WinScreen>().Stage = _stage;
            StartCoroutine(Timer(3f, _winScreen));
        }
        else
        {
            _loseScreen.SetActive(true);
        }
    }
    
    private void OnGameStart(GameStartEvent obj)
    {
        _overlay.SetActive(true);
        
    }

    private IEnumerator Timer(float time, GameObject go)
    {
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            yield return null;
        }
        go.SetActive(true);
    }
}
