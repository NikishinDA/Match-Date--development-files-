using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemController : MonoBehaviour
{
   private void Start()
   {
      float time = GetComponent<ParticleSystem>().main.duration;
      StartCoroutine(LifeTimer(time));
   }

   IEnumerator LifeTimer(float time)
   {
      for (float t = 0; t < time; t += Time.deltaTime)
      {
         yield return null;
      }
      Destroy(gameObject);
   }
}
