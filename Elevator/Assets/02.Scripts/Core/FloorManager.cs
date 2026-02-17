using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 층 진행 담당
public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance;

    [Header("Floor Sequence")]
    public FloorData[] floors;

    public int CurrentFloorIndex { get; private set; }
    public FloorData CurrentFloor => floors[CurrentFloorIndex];

    [Header("Floor UI")]
    [SerializeField] Text floorText;   // 기존 Text UI

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateFloorUI(); // 시작 시 초기 층 표시
    }

    public void MoveToNextFloor()
    {
        CurrentFloorIndex++;

        if (CurrentFloorIndex >= floors.Length)
        {
            Debug.Log("Last Floor Reached");
            CurrentFloorIndex = floors.Length - 1;
        }

        Debug.Log($"Arrived at Floor {CurrentFloor.floorNumber}");
        UpdateFloorUI(); // 층 이동과 동시에 UI 갱신
    }

    void UpdateFloorUI()
    {
        if (floorText == null)
            return;

        // 숫자만 표시
        floorText.text = CurrentFloor.floorNumber.ToString();
    }
}
