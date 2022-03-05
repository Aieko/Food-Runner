using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public static ObjectPooler Instance;

    public List<Pool> pools;

    public Dictionary<string, Queue<GameObject>> poolDictionary;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
  
    }

    // Start is called before the first frame update
    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (var pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);

            }

            poolDictionary.Add(pool.tag, objectPool);
        }
        
    }

    public GameObject SpawnFromPool(string tag,Vector3 pos, Quaternion rot = new Quaternion())
    {
        if(!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        var objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.transform.position = pos;
        objectToSpawn.transform.rotation = rot;
        objectToSpawn.SetActive(true);
        

        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }

}
