using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseScreen : MonoBehaviour
{
    [SerializeField] Button _restartButton;

    private void Awake()
    {
        _restartButton.onClick.AddListener(OnRestartButtonPress);
        
    }
    private void OnRestartButtonPress()
    {
        PlayerPrefs.SetInt("Level",PlayerPrefs.GetInt("Level",1) + 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

