using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextStageTextController : MonoBehaviour
{
    private Animator _animator;
    [SerializeField] private float _lifeTime = 1.5f;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        StartCoroutine(Timer());
    }

    IEnumerator Timer()
    {
        for (float t = 0; t < _lifeTime; t += Time.deltaTime)
        {
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
