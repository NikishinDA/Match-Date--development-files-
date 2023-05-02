using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class ActorAnimationController : MonoBehaviour
{
    private Animator _animator;

    private bool _ready = true;

    private static readonly int hug = Animator.StringToHash("Hug");
    private static readonly int hand = Animator.StringToHash("Hand");
    private static readonly int kiss = Animator.StringToHash("Kiss");

    // Start is called before the first frame update
    private void Awake()
    {
        EventManager.AddListener<StageChangeEvent>(OnStageChange);
        EventManager.AddListener<GameOverEvent>(OnGameOver);
        _animator = GetComponent<Animator>();
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<StageChangeEvent>(OnStageChange);
        EventManager.RemoveListener<GameOverEvent>(OnGameOver);

    }

    private void OnGameOver(GameOverEvent obj)
    {
        if (obj.IsWin)
        {
            _animator.SetTrigger(hug);
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Alpha1))
        {
            _animator.SetTrigger("Hand");
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            _animator.SetTrigger("Hug");
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            _animator.SetTrigger("Kiss");
        }
    }

    private void OnAnimationEnd()
    {
        _ready = true;
    }
    private void OnStageChange(StageChangeEvent obj)
    {
        if (_ready)
        {
            switch (obj.Stage)
            {
                case 1:
                {
                    _animator.SetTrigger(hand);
                    //_ready = false;
                }
                    break;
                case 3:
                {
                    _animator.SetTrigger(hug);
                    
                    //_ready = false;
                }
                    break;
                case 2:
                {
                    _animator.SetTrigger(kiss);
                    
                    //_ready = false;
                }
                    break;
            }
        }
    }
}
