using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC 풀링 & 랜덤 제공 담당
/// ElevatorController는 이 스크립트만 의존
/// </summary>
public class NPCSpawner : MonoBehaviour
{
    public static NPCSpawner Instance;

    [Header("NPC Prefabs")]
    public List<NPCController> npcPrefabs;

    [Header("Pooling")]
    public int poolSizePerType = 3;

    List<NPCController> pool = new List<NPCController>();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializePool();
    }

    // =========================
    // 풀 초기화
    // =========================

    void InitializePool()
    {
        foreach (var prefab in npcPrefabs)
        {
            for (int i = 0; i < poolSizePerType; i++)
            {
                NPCController npc = Instantiate(prefab, transform);
                npc.gameObject.SetActive(false);
                pool.Add(npc);
            }
        }
    }

    // =========================
    // NPC 요청
    // =========================

    /// <summary>
    /// 엘리베이터에서 사용할 랜덤 NPC 반환
    /// </summary>
    public NPCController GetRandomNPCExcludeTypes(HashSet<NPCType> usedTypes)
    {
        // 사용 가능한 후보 수집
        List<NPCController> candidates = new List<NPCController>();

        foreach (var npc in pool)
        {
            if (!usedTypes.Contains(npc.npcType))
            {
                candidates.Add(npc);
            }
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("No NPC available for unique spawn");
            return null;
        }

        // 랜덤 선택
        NPCController selected = candidates[Random.Range(0, candidates.Count)];
        pool.Remove(selected);

        selected.gameObject.SetActive(true);
        ResetNPC(selected);

        return selected;
    }

    // =========================
    // NPC 반환
    // =========================

    public void Despawn(NPCController npc)
    {
        npc.GetComponent<BaseNPC>()?.OnRideEnd();
        npc.gameObject.SetActive(false);
        pool.Add(npc);
    }

    // =========================
    // 상태 초기화
    // =========================

    void ResetNPC(NPCController npc)
    {
        npc.transform.rotation = Quaternion.identity;
        npc.defaultLookTarget = ElevatorController.Instance.elevatorLookTarget;

        // NavMeshAgent 리셋
        var agent = npc.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent)
        {
            // NavMesh 위에 있는지 확인
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
                agent.isStopped = true;
            }
            else
            {
                // 아직 NavMesh 위가 아니면 agent 비활성화
                agent.enabled = false;
            }
        }
    }
}
