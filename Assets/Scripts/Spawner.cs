using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Spawnee[] Spawnees;
    private float[] timers;

    private void Start()
    {
        timers = new float[Spawnees.Length];
        for (int i = 0; i < Spawnees.Length; i++)
        {
            timers[i] = Spawnees[i].AutoSpawnIntervalSec;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < Spawnees.Length; i++)
        {
            var s = Spawnees[i];
            if (timers[i] <= 0)
            {
                Spawn(s.O);
                timers[i] = s.AutoSpawnIntervalSec;
            } else
            {
                timers[i] -= Time.deltaTime;
            }
        }
    }

    void Spawn(GameObject o)
    {
        Instantiate<GameObject>(o, transform);
    }
}

[System.Serializable]
public class Spawnee
{
    public GameObject O;
    public float AutoSpawnIntervalSec;
}
