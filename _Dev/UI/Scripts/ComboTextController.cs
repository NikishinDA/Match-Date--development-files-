using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ComboTextController : MonoBehaviour
{
    [SerializeField] private float _lifeTime;
    private void OnEnable()
    {
        StartCoroutine(LifeTimer(_lifeTime));
    }

    private IEnumerator LifeTimer(float time)
    {
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
