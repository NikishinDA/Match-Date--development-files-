using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorSpawn : MonoBehaviour
{
    [SerializeField] private GameObject[] _actorPrefabs;

    //[SerializeField] private Transform _parent;
    // Start is called before the first frame update
    void Start()
    {
        int level = PlayerPrefs.GetInt("Level", 1);
        GameObject go = Instantiate(_actorPrefabs[level % _actorPrefabs.Length], transform);
    }
}
