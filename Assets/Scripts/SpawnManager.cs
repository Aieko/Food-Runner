using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]private GameObject[] obstaclePrefab;
    [SerializeField] private GameObject[] ediblePrefab;
    [SerializeField] private GameObject[] inediblePrefab;
    [SerializeField]private GameObject[] pickablePrefab;
    [SerializeField] private GameObject[] rowsOfEdiblePrefab;
    [SerializeField] private Dictionary<GameObject, int> someDictionary = new Dictionary<GameObject, int>();

    [Header("Obstacles")]
    [Header("Chance to Spawn")]
    [SerializeField, Range(0, 100)] private int zeroInArray = 50;
    [SerializeField, Range(0, 100)] private int firstInArray = 50;
    [SerializeField, Range(0, 100)] private int secondInArray = 50;
    [SerializeField, Range(0, 100)] private int thirdInArray = 50;
    [Header("Pickables")]
    [Tooltip("Chance to spawn pickable on obstacle")]
    [SerializeField, Range(0, 100)] private int _pickableOnObstacle = 50;
    [SerializeField, Range(0, 100)] private int _edible = 50;
    [SerializeField, Range(0, 100)] private int _rowOfEdible = 50;
    [SerializeField, Range(0, 100)] private int _inedible = 50;
    [SerializeField, Range(0, 100)] private int _powerup = 50;
    [Header("Rows")]
    [Tooltip("Extra row after diagonal row")]
    [SerializeField, Range(0, 100)] private int extraRow;
    [SerializeField, Range(0, 100)] private int zeroInRowArray = 50;
    [SerializeField, Range(0, 100)] private int firstInRowArray = 50;
    [SerializeField, Range(0, 100)] private int secondInRowArray = 50;
    [SerializeField, Range(0, 100)] private int thirdInRowArray = 50;
    [SerializeField, Range(0, 100)] private int fourthInRowArray = 50;

    [SerializeField] private Vector3 spawnPosition = new Vector3(160, 0, 0);
    private List<int> takenPos = new List<int>();
    private double min = 1f;
    private double max = 3f;

    public int wavesSpawned { get; private set; }
    public static SpawnManager Instance { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnObstacle());

        StartCoroutine(SpawnBarrier());

        StartCoroutine(SpawnEnviroment());

        wavesSpawned = 0;
    }

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

    private IEnumerator SpawnEnviroment()
    {
        while(GameManager.Instance.gameOver == false)
        {
            float spawnRate = Random.Range(10, 50) / (GameManager.Instance.OMS.speed * Time.fixedDeltaTime);

            yield return new WaitForSeconds(Random.Range(1, 15));

            ObjectPooler.Instance.SpawnFromPool("Cactus1", new Vector3(Random.Range(160, 260), 0, Random.Range(15f, 40f)));
            ObjectPooler.Instance.SpawnFromPool("Cactus1", new Vector3(Random.Range(160, 260), 0, Random.Range(-15f, -40f)));
        }
    }

    private IEnumerator SpawnBarrier()
    {
        while (!GameManager.Instance.gameOver)
        {
            float fixedFrames = 10 / (GameManager.Instance.OMS.speed * Time.fixedDeltaTime);
            yield return new WaitForSeconds(fixedFrames * Time.fixedDeltaTime);

            ObjectPooler.Instance.SpawnFromPool("Barrier", new Vector3(160, 0, 9));
            ObjectPooler.Instance.SpawnFromPool("Barrier", new Vector3(160, 0, -9), Quaternion.Euler(0, 180, 0));
        }
    }

    private IEnumerator SpawnObstacle()
    {
        while(!GameManager.Instance.gameOver)
        {
            var spawnRate = ((100 / GameManager.Instance.OMS.speed) + (1 / GameManager.Instance.gamePhase)) / Random.Range((float)min,(float)max);

            yield return new WaitForSeconds(spawnRate);

            Spawn();
        }
       
    }

    private void Spawn()
    {
        takenPos.Clear();

        var obstacleNum = Random.Range(1, 4);

        var rowSpawned = false;
        var verticalRow = false;
        var wallNum = 0;
        var edibleNum = 0;

        for(int i = 0; i < obstacleNum; i++)
        {
            GameObject obstacle = null;
            var spawnPosition = new Vector3();

            var spawnRow = Random.Range(0, 101);

            if (wallNum == 2)
            {
                obstacle = GenerateObstacle(out Vector3 spawnPos, 0);

                spawnPosition = spawnPos;
            }
            else if(spawnRow <= _rowOfEdible && !rowSpawned)
            {
                obstacle = GenerateRow(out Vector3 spawnPos);

                rowSpawned = true;

                spawnPosition = spawnPos;
            }
            else
            {
                obstacle = GenerateObstacle(out Vector3 spawnPos);

                spawnPosition = spawnPos;
            }

            if (takenPos.Contains((int)spawnPosition.z) || obstacle == null) return;

            var spawnedObstacle = Instantiate(obstacle, spawnPosition, obstacle.transform.rotation);

            if (obstacle.Equals(obstaclePrefab[1])) wallNum++;

            else if (obstacle.Equals(obstaclePrefab[0]) || obstacle.Equals(obstaclePrefab[3]))
            {
                var ranNum = Random.Range(0, 101);

                if (ranNum <= _pickableOnObstacle)
                {
                    var pickablePlatform = Instantiate(obstaclePrefab[2], spawnPosition, obstacle.transform.rotation);

                    float spawnHeight = obstacle.Equals(obstaclePrefab[0]) ? 2.5f : 1f;

                    pickablePlatform.transform.position = new Vector3(pickablePlatform.transform.position.x, spawnHeight, pickablePlatform.transform.position.z);

                    var pickableToSpawn = wallNum == 2 ? GeneratePickable(true) : GeneratePickable();

                    Instantiate(pickableToSpawn, pickablePlatform.transform);
                }
  
            }
            else if (obstacle.Equals(obstaclePrefab[2]) && !verticalRow)
            {
                spawnedObstacle.transform.position = new Vector3(spawnedObstacle.transform.position.x,
                            spawnedObstacle.transform.position.y + 1,
                            spawnedObstacle.transform.position.z);

                var pickableToSpawn = GeneratePickable();

                if (pickableToSpawn.TryGetComponent(out Edible edible))
                {
                   
                    if (edibleNum == 1)
                    {
                        spawnedObstacle.transform.position = new Vector3
                            (spawnedObstacle.transform.position.x + 10f,
                            spawnedObstacle.transform.position.y,
                            spawnedObstacle.transform.position.z);
                    }
                    else if (edibleNum == 2)
                    {
                        spawnedObstacle.transform.position = new Vector3
                            (spawnedObstacle.transform.position.x + 20f,
                            spawnedObstacle.transform.position.y,
                            spawnedObstacle.transform.position.z);
                    }

                    edibleNum++;
                }

                var pickable = Instantiate(pickableToSpawn, spawnedObstacle.transform);
            }
            else if(obstacle.Equals(rowsOfEdiblePrefab[0]) || obstacle.Equals(rowsOfEdiblePrefab[1]))
            {
                verticalRow = true;

                var ranNum = Random.Range(0, 101);

                if (ranNum <= extraRow)
                {
                    var extraRow = GenerateRow(out Vector3 spawnPos, -1, true);

                    if (extraRow == null) return;

                    var extraRowPos = spawnPosition;

                    if (spawnPosition.z == 0)
                    {
                        var left = Random.Range(0, 2);
                        var num = 0;
                        if (left == 0)
                        {
                            num = 6;
                        }
                        else
                        {
                            num = -6;
                        }

                        extraRowPos = new Vector3(spawnPosition.x + 15, spawnPosition.y, spawnPosition.z + num);
                    }
                    else if (spawnPosition.z == -6)
                    {
                        extraRowPos = new Vector3(spawnPosition.x + 15, spawnPosition.y, spawnPosition.z +6);
                    }
                    else if(spawnPosition.z == 6)
                    {
                        extraRowPos = new Vector3(spawnPosition.x + 15, spawnPosition.y, spawnPosition.z - 6);
                    }

                    

                    Instantiate(extraRow, extraRowPos, extraRow.transform.rotation);
                }

                
            }

            takenPos.Add((int)spawnPosition.z);
        }

        wavesSpawned++;
    }

    public void ClearWavesCounting()
    {
        wavesSpawned = 0;

        if(min > 0.5)
        {
            min -= 0.05;
        }
        if(max > 1.5)
        {
            max -= 0.1;
        }
        
    }

    private GameObject GeneratePickable(bool noInedible = false)
    {
        List<int> res = new List<int>();

        GameObject pickableToSpawn = null;

        while (res.Count == 0)
        {
            for (int j = 0; j < 3; j++)
            {
                if (RandomizePickables(j))
                {
                    res.Add(j);
                }
            }
        }

        while (res.Count != 1)
        {
            for (int j = 0; j < res.Count; j++)
            {
                if (!RandomizePickables(res[j]))
                {
                    res.RemoveAt(j);
                }

            }
        }

        if (res.Count == 1)
        {

            switch (res[0])
            {
                case 0:
                    pickableToSpawn = ediblePrefab[Random.Range(0, ediblePrefab.Length)];
                    break;
                case 1:
                    pickableToSpawn = noInedible ? null : inediblePrefab[Random.Range(0, inediblePrefab.Length)];
                    break;
                case 2:
                    pickableToSpawn = pickablePrefab[Random.Range(0, pickablePrefab.Length)];
                    break;
                default:

                    break;
            }
        }


        return pickableToSpawn;
    }

    private GameObject GenerateObstacle(out Vector3 spawnPos, int certainObst = -1)
    {
        GameObject obstacle = null;

        if (certainObst == -1)
        {
            List<int> res = new List<int>();

            for (int i = 0; i < obstaclePrefab.Length; i++)
            {
                if(RandomizeObstacles(i))
                {
                    res.Add(i);
                }
                
            }

            if (res.Count == 0)
            {
                spawnPos = Vector3.zero;
                return obstacle;
            }

            while(res.Count != 1)
            {
                for (int i = 0; i < res.Count; i++)
                {
                    if (!RandomizeObstacles(res[i]))
                    {
                        res.RemoveAt(i);
                    }

                }
            }

            if(res.Count == 1)
            {
                obstacle = obstaclePrefab[res[0]];
            }   
        }
        else
        {
           obstacle = obstaclePrefab[certainObst];
        }

        var randomNum = Random.Range(1, 4);
        spawnPos = spawnPosition;
        switch (randomNum)
        {
            case 1:
                spawnPos += new Vector3(0, 0, 0);
                break;
            case 2:
                spawnPos += new Vector3(0, 0, -6);
                break;
            case 3:
                spawnPos += new Vector3(0, 0, 6);
                break;
            default:
                break;
        }

        return obstacle;
    }
    // Fix spawning two edibles in one place
    private GameObject GenerateRow(out Vector3 spawnPos, int certainRow = -1, bool isExtraRow = false)
    {
        GameObject row = null;

        if (certainRow == -1)
        {
            List<int> res = new List<int>();

            for (int i = isExtraRow ? 2 : 0; i < rowsOfEdiblePrefab.Length; i++)
            {
                if (RandomizeRows(i))
                {
                    res.Add(i);
                }

            }

            if (res.Count == 0)
            {
                spawnPos = Vector3.zero;
                return row;
            }

            while (res.Count != 1)
            {
                for (int i = 0; i < res.Count; i++)
                {
                    if (!RandomizeRows(res[i]))
                    {
                        res.RemoveAt(i);
                    }

                }
            }

            if (res.Count == 1)
            {
                row = rowsOfEdiblePrefab[res[0]];
            }
        }
        else
        {
            row = rowsOfEdiblePrefab[certainRow];
        }

        var randomNum = Random.Range(1, 4);
        spawnPos = spawnPosition;
        switch (randomNum)
        {
            case 1:
                spawnPos += new Vector3(0, 0, 0);
                break;
            case 2:
                spawnPos += new Vector3(0, 0, -6);
                break;
            case 3:
                spawnPos += new Vector3(0, 0, 6);
                break;
            default:
                break;
        }

        if(row.Equals(rowsOfEdiblePrefab[0]) || row.Equals(rowsOfEdiblePrefab[1]))
        {
            if(spawnPos.z == 6)
            {
                row = rowsOfEdiblePrefab[1];
            }
            else if(spawnPos.z == -6)
            {
                row = rowsOfEdiblePrefab[0];
            }
        }

        return row;
    }

    private bool RandomizeRows(int num)
    {
        int randNum = Random.Range(0, 101);

        bool res = false;

        switch (num)
        {
            case 0:
                if (randNum <= zeroInRowArray)
                {
                    res = true;
                }
                break;
            case 1:
                if (randNum <= firstInRowArray)
                {
                    res = true;
                }
                break;
            case 2:
                if (randNum <= secondInRowArray)
                {
                    res = true;
                }
                break;
            case 3:
                if (randNum <= thirdInRowArray)
                {
                    res = true;
                }
                break;
            case 4:
                if(randNum <= fourthInRowArray)
                {
                    res = true;
                }
                break;
            default:
                break;
        }

        return res;
    }

    private bool RandomizeObstacles(int num)
    {
        int randNum = Random.Range(0, 101);

        bool res = false;

        switch (num)
        {
            case 0:
                if(randNum <= zeroInArray)
                {
                    res = true;
                }
                break;
            case 1:
                if (randNum <= firstInArray)
                {
                    res = true;
                }
                break;
            case 2:
                if (randNum <= secondInArray)
                {
                    res = true;
                }
                break;
            case 3:
                if(randNum <= thirdInArray)
                {
                    res = true;
                }
                break;
            default:
                break;
        }

        return res;
    }

    private bool RandomizePickables(int type)
    {
        int randNum = Random.Range(0, 101);

        var res = false;

        switch ((Pickables)type)
        {
            case Pickables.edible:
                if (randNum <= _edible)
                {
                    res = true;
                }
                break;
            case Pickables.inedible:
                if (randNum <= _inedible)
                {
                    res = true;
                }
                break;
            case Pickables.powerup:
                if (randNum <= _powerup)
                {
                    res = true;
                }
                break;
            default:
                break;
        }

        return res;
    }

    private enum Pickables
    {
        edible,
        inedible,
        powerup
    }

    //junkyard
    /* if(obstacle.Equals(rowsOfEdiblePrefab[0]))
                 {
                     if (spawnPosition.z == 0)
                     {
                         takenPos.Add(6);
                     }
                     else takenPos.Add(0);  
                 }
                 else
                 {
                     if (spawnPosition.z == 0)
                     {
                         takenPos.Add(-6);
                     }
                     else takenPos.Add(0);
                 }*/

}
