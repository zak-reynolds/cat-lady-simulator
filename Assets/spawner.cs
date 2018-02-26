using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawner : MonoBehaviour {

    public enum SpawnState { Spawning, Waiting, Counting }
    public GameObject catPrefab;
    //allows us to change values of instances of this class inside unity inspector
    [System.Serializable]
    public class spawn
    {
        public string name;
        public Transform cat;
        public int count;
        public float rate =5;
    }

    public spawn[] spawns;
    private int nextWave = 0;

    public float timeBetweenSpawns = 5f;
    public float spawnCountdown;

    private float searchCountdown = 1;

    private SpawnState state = SpawnState.Counting;

	// Use this for initialization
	void Start () {
        spawnCountdown = timeBetweenSpawns;
	}
	
	// Update is called once per frame
	void Update () {
        if(state == SpawnState.Waiting)
        {
            //check if enemies are still alive
            if (!catIsAlive())
            {
                Debug.Log("Wave completed");
                return;
            }
            else
            {
                return;
            }
        }

		if(spawnCountdown <= 0)
        {
            if(state != SpawnState.Spawning)
            {
                //Start spawning baby
                StartCoroutine(SpawnWave(spawns[nextWave]));
            }
        }
        else
        {
            spawnCountdown -= Time.deltaTime;
        }
	}

    bool catIsAlive()
    {
        searchCountdown -= Time.deltaTime;
        if (searchCountdown <= 0)
        {
            searchCountdown = 1f;
            if (GameObject.FindGameObjectWithTag("spazcat") == null)
            {
                return false;//no cat is alive
            }                                   
        }
        return true;
    }

    IEnumerator SpawnWave (spawn _spawn)
    {
        Debug.Log("SPawning wave");
        state = SpawnState.Spawning;
        //spawn
        for(int i =0; i < _spawn.count; i++)
        {
            SpawnCat();
            yield return new WaitForSeconds(1f / _spawn.rate);
        }

        state = SpawnState.Waiting;//waits for player to kill enemies

        yield break;
    }

    void SpawnCat ()
    {
        //spawn cat
        Vector3 spawnPos = transform.position;
        spawnPos.y = 0;
        Instantiate(catPrefab, spawnPos, Quaternion.identity);
        Debug.Log("Spawning cat" + catPrefab.name);
    }

}
