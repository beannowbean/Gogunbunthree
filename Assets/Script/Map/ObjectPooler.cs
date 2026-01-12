using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [Header("BackGround Tile")]
    public List<GameObject> backgroundBuildings;

    [Header("Car Tile")]
    public List<GameObject> easyObstacles;
    public List<GameObject> normalObstacles;
    public List<GameObject> hardObstacles;
    public List<GameObject> tutorialObstacles;
    
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        Instance = this;
        
        // 모든 리스트의 프리팹들을 하나의 딕셔너리에 초기화 (각 2개씩)
        InitializePool(backgroundBuildings);
        InitializePool(easyObstacles);
        InitializePool(normalObstacles);
        InitializePool(hardObstacles);
        InitializePool(tutorialObstacles);
    }

    private void InitializePool(List<GameObject> prefabList)
    {
        foreach (GameObject prefab in prefabList)
        {
            if (prefab == null || poolDictionary.ContainsKey(prefab)) continue;

            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < 2; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.AddComponent<PoolMember>().myPrefab = prefab;
                // obj.name = prefab.name; 
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(prefab, objectPool);
        }
    }

    public GameObject GetPool(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (prefab == null && !poolDictionary.ContainsKey(prefab)) return null;

        // 큐에서 꺼냈는데 혹시 파괴된 오브젝트라면 다시 꺼내도록 루프
        GameObject obj = null;
        while (poolDictionary[prefab].Count > 0)
        {
            obj = poolDictionary[prefab].Dequeue();
            if (obj != null) break; // 정상적인 오브젝트면 탈출
        }

        // 만약 큐가 비어서 새로 만들어야 한다면 (보험용)
        if (obj == null)
        {
            obj = Instantiate(prefab);
            obj.AddComponent<PoolMember>().myPrefab = prefab;
        }

        obj.SetActive(true);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.SetParent(parent);
        obj.transform.localScale = Vector3.one;

        return obj;
    }

    public void ReturnPool(GameObject obj)
    {
        if(Instance == null || obj == null) return;
        PoolMember member = obj.GetComponent<PoolMember>();
        if (member != null && poolDictionary.ContainsKey(member.myPrefab))
        {
            obj.SetActive(false);
            poolDictionary[member.myPrefab].Enqueue(obj);
        } else Destroy(obj);
    }
}

public class PoolMember : MonoBehaviour
{
    public GameObject myPrefab;
}