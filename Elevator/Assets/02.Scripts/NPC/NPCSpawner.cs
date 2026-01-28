using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 랜덤 인원 생성
public class NPCSpawner : MonoBehaviour
{
    public GameObject[] npcPrefabs;

    void Start()
    {
        int count = Random.Range(1, 5);
        for (int i = 0; i < count; i++)
        {
            Instantiate(npcPrefabs[Random.Range(0, npcPrefabs.Length)]);
        }
    }
}
