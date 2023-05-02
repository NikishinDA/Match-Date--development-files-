using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundsController : MonoBehaviour
{
    [SerializeField] private GameObject[] _bgs;
    void Start()
    {
        _bgs[Random.Range(0, _bgs.Length)].SetActive(true);
    }
    
}
