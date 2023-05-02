using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombExplController : MonoBehaviour
{
    public void OnAnimationEnd()
    {
        Destroy(gameObject);
    }
}
