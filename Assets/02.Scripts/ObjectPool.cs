using System.Collections.Generic;
using UnityEngine;

public static class ObjectPool
{
    // 각 오브젝트 종류에 대한 풀. key로 식별하고 큐에 비활성화된 오브젝트를 보관함.
    private static Dictionary<string, Queue<GameObject>> pool = new();

    // key에 대응하는 프리팹 정보. Resources.Load()로 불러온 프리팹을 저장함.
    private static Dictionary<string, GameObject> prefabMap = new();

    // Initialize()가 한 번만 실행되도록 막기 위한 플래그
    private static bool initialized = false;

    // 오브젝트 풀에 등록할 항목
    private class PoolSetting
    {
        public string key;       // 키 이름 (프리팹 이름)
        public string path;      // Resources 내 프리팹 경로
        public int count;        // 초기 생성 수량

        public PoolSetting(string key, string path, int count)
        {
            this.key = key;
            this.path = path;
            this.count = count;
        }
    }

    //=================================================================================

    // 등록할 오브젝트 풀 리스트. (추가작성 가능)
    private static List<PoolSetting> settings = new()
    {
        new PoolSetting("Projectile", "Prefabs/Projectile", 20)
    };

    //=================================================================================

    // 실제 오브젝트 풀을 초기화
    private static void Initialize()
    {
        // 이미 초기화된 경우 중복 실행 방지
        if (initialized) return;
        initialized = true;

        // 사전 정의된 오브젝트 풀 리스트(settings)에 따라 각 오브젝트 풀 생성
        foreach (var setting in settings)
        {
            // Resources 폴더에서 프리팹 로드
            GameObject prefab = Resources.Load<GameObject>(setting.path);

            // 프리팹이 없으면 해당 항목 건너뜀
            if (prefab == null)
            {
                continue;
            }
            // key에 해당하는 프리팹과 큐를 딕셔너리에 저장
            prefabMap[setting.key] = prefab;                  // 나중에 참조용
            pool[setting.key] = new Queue<GameObject>();       // 오브젝트 보관용 큐

            // 설정된 수량만큼 오브젝트를 미리 생성하여 비활성화 상태로 큐에 삽입
            for (int i = 0; i < setting.count; i++)
            {
                GameObject obj = GameObject.Instantiate(prefab); // 복제 생성
                obj.SetActive(false);                            // 오브젝트 비활성화
                pool[setting.key].Enqueue(obj);                  // 큐에 저장
            }
        }
    }


    // key에 해당하는 오브젝트를 풀에서 꺼냄. 없으면 null 반환.
    public static GameObject Get(string key)
    {
        // 초기화가 안 되어 있으면 자동으로 Initialize 실행 (최초 1회)
        if (!initialized) Initialize();

        // 해당 key에 대한 풀 자체가 없거나, 큐에 오브젝트가 없으면 null 반환
        if (!pool.ContainsKey(key) || pool[key].Count == 0)
        {
            // 풀에 등록되지 않았거나, 모두 사용 중인 상태
            return null;
        }

        // 큐에서 오브젝트 하나 꺼냄
        GameObject obj = pool[key].Dequeue();

        // 꺼낸 오브젝트를 활성화해서 씬에 등장시킴
        obj.SetActive(true);

        // 호출한 쪽으로 반환
        return obj;
    }

    // key에 해당하는 오브젝트를 다시 풀에 반환
    public static void Return(string key, GameObject obj)
    {
        // 초기화가 안 되어 있으면 자동으로 Initialize 실행
        if (!initialized) Initialize();

        // 오브젝트를 비활성화
        obj.SetActive(false);

        // key에 해당하는 풀(queue)이 없으면 새로 생성
        if (!pool.ContainsKey(key))
            pool[key] = new Queue<GameObject>();

        // 비활성화된 오브젝트를 다시 큐에 넣음
        pool[key].Enqueue(obj);
    }

}
