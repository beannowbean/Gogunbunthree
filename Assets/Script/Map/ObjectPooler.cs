using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오브젝트 풀링 스크립트
/// </summary>
public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;    // 싱글톤 인스턴스

    [Header("배경 배열")]
    public List<GameObject> backgroundBuildings;    // 빌딩 배열

    [Header("장애물 배열")]
    public List<GameObject> level_1Obstacles;   // 레벨1 배열
    public List<GameObject> level_2Obstacles;   // 레벨2 배열
    public List<GameObject> level_3Obstacles;   // 레벨3 배열
    public List<GameObject> tutorialObstacles;  // 튜토리얼 배열

    [Header("아이템 배열")]
    public List<GameObject> starObstacles;  // 별 아이템 배열
    public List<GameObject> heliObstacles;  // 헬기 아이템 배열
    
    // 내부 변수
    Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>(); // 프리팹-오브젝트 큐 딕셔너리

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        Instance = this;
        
        // 모든 리스트의 프리팹들을 하나의 딕셔너리에 초기화 (각 2개씩)
        InitializePool(backgroundBuildings);

        InitializePool(level_1Obstacles);
        InitializePool(level_2Obstacles);
        InitializePool(level_3Obstacles);
        InitializePool(tutorialObstacles);

        InitializePool(starObstacles);
        InitializePool(heliObstacles);
    }

    // 프리팹 리스트를 받아서 딕셔너리에 초기화
    private void InitializePool(List<GameObject> prefabList)
    {
        // 각 프리팹마다 오브젝트 풀 생성
        foreach (GameObject prefab in prefabList)
        {
            // 이미 딕셔너리에 있거나 null이면 건너뜀
            if (prefab == null || poolDictionary.ContainsKey(prefab)) continue;

            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < 2; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.AddComponent<PoolMember>().myPrefab = prefab;   // 풀 멤버 컴포넌트 추가 (어떤 프리팹에서 왔는지 추적용)
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(prefab, objectPool);
        }
    }

    // 오브젝트 풀에서 꺼내는 함수
    public GameObject GetPool(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        // 이미 딕셔너리에 있거나 null이면 null 반환
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

    // 오브젝트 풀에 반환하는 함수
    public void ReturnPool(GameObject obj)
    {
        // 인스턴스가 없거나 null이면 건너뜀
        if(Instance == null || obj == null) return;

        PoolMember member = obj.GetComponent<PoolMember>(); // 풀 멤버 컴포넌트 참조 (어떤 프리팹에서 왔는지 추적용)

        // 딕셔너리에 해당 프리팹이 있으면 큐에 반환, 없으면 파괴
        if (member != null && poolDictionary.ContainsKey(member.myPrefab))
        {
            obj.SetActive(false);
            poolDictionary[member.myPrefab].Enqueue(obj);
        } else Destroy(obj);
    }
}

// 풀 멤버 컴포넌트 (어떤 프리팹에서 왔는지 추적용)
public class PoolMember : MonoBehaviour
{
    public GameObject myPrefab;
}