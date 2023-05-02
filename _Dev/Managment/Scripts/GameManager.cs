using System;
using System.Collections;
using System.Collections.Generic;
using GameAnalyticsSDK;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool _gameEnded = false;
    private float _playTimer;
    private void Awake()
    {
        GameAnalytics.Initialize();
        EventManager.AddListener<GameOverEvent>(OnGameOver);
        EventManager.AddListener<GameStartEvent>(OnGameStart);
        _gameEnded = false;
    }
    private void OnDestroy()
    {
        EventManager.RemoveListener<GameOverEvent>(OnGameOver);
        EventManager.RemoveListener<GameStartEvent>(OnGameStart);


    }

    private void OnGameStart(GameStartEvent obj)
    {
        GameAnalytics.NewProgressionEvent (
            GAProgressionStatus.Start,
            "Level_" + PlayerPrefs.GetInt("Level",1));
        _playTimer = 0;
        StartCoroutine(Timer());
    }

    private void OnGameOver(GameOverEvent obj)
    {
        if (!_gameEnded)
        {
            _gameEnded = true;
            var status = (obj.IsWin) ? GAProgressionStatus.Complete : GAProgressionStatus.Fail;
            GameAnalytics.NewProgressionEvent(
                status,
                "Level_" + PlayerPrefs.GetInt("Level",1),
                "PlayTime_" + Mathf.RoundToInt(_playTimer));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            PlayerPrefs.SetInt("Level", 1);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    IEnumerator Timer()
    {
        for (;;)
        {
            _playTimer += Time.deltaTime;
            yield return null;
        }
    }
}
