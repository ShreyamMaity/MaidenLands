using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TestScript : MonoBehaviour
{
    SpawnItem spawnee;
    Dictionary<int, string> dist = new Dictionary<int, string>();

    void Start()
    {
        spawnee = GetComponentInChildren<SpawnItem>();

        // dist.Add(0, "Yennefer");

        foreach(KeyValuePair<int, string> ie in dist)
        {
            Debug.Log(ie.Key);
            Debug.Log(ie.Value);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            spawnee.CreateSpawn();
        }
    }
}
