using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ãþ ÁøÇà ´ã´ç
public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance;

    [Header("Floor Sequence")]
    public FloorData[] floors;

    public int CurrentFloorIndex { get; private set; }
    public FloorData CurrentFloor => floors[CurrentFloorIndex];

    void Awake()
    {
        Instance = this;
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
    }
}
