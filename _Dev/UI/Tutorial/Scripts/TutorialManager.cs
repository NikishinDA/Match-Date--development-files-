using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _tutorials;
    private void Awake()
    {
        EventManager.AddListener<GameStartEvent>(OnGameStart);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<GameStartEvent>(OnGameStart);
    }

    private void OnGameStart(GameStartEvent obj)
    {
        switch (PlayerPrefs.GetInt("Level",1))
        {
            case 1:
            {
                _tutorials[0].SetActive(true);
            }
                break;
            case 3:
            {
                _tutorials[1].SetActive(true);
            }
                break;
            case 4:
            {
                _tutorials[2].SetActive(true);
            }
                break;
            case 5:
            {
                _tutorials[3].SetActive(true);
            }
                break;
        }
    }
}
