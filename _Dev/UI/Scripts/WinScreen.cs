using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinScreen : MonoBehaviour
{
    [SerializeField] Button _restartButton;
    [SerializeField] private GameObject[] _stars;
    public int Stage = 0;
    private void Awake()
    {
       // EventManager.AddListener<StageChangeEvent>(OnStageChange);
        _restartButton.onClick.AddListener(OnRestartButtonPress);
        
    }
    private void OnDestroy()
    {
        //EventManager.RemoveListener<StageChangeEvent>(OnStageChange);
    }

    private void OnEnable()
    {
        if (Stage >= 2)
        {
            _stars[0].SetActive(true);
        }

        if (Stage >= 3)
        {
            _stars[1].SetActive(true);
        }

        if (Stage >= 4)
        {
            _stars[2].SetActive(true);
        }
    }


    private void OnRestartButtonPress()
    {
        PlayerPrefs.SetInt("Level",PlayerPrefs.GetInt("Level",1) + 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
