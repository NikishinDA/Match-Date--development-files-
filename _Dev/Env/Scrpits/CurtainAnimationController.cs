using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurtainAnimationController : MonoBehaviour
{
    private Animator _animator;
    [SerializeField] private ParticleSystem _confetti;
    [SerializeField] private ParticleSystem _hearts;
    private void Awake()
    {
        EventManager.AddListener<GameOverEvent>(OnGameOver);
        EventManager.AddListener<GameStartEvent>(OnGameStart);

        _animator = GetComponent<Animator>();
    }
    private void OnDestroy()
    {
        EventManager.RemoveListener<GameOverEvent>(OnGameOver);
        EventManager.RemoveListener<GameStartEvent>(OnGameStart);
    }

    private void OnGameStart(GameStartEvent obj)
    {
        _animator.SetTrigger("Open");
    }

    private void OnGameOver(GameOverEvent obj)
    {
        //_animator.SetTrigger("Close");
        StartCoroutine(ActionCor(obj.IsWin));
    }

    private IEnumerator ActionCor(bool win)
    {
        if (win) _confetti.Play();
        for (float t = 0; t < 0.5f; t += Time.deltaTime)
        {
            yield return null;
        }
        _animator.SetTrigger("Close");
        for (float t = 0; t < 1.5f; t += Time.deltaTime)
        {
            yield return null;
        }
        if (win) _hearts.Play();
    }
}
