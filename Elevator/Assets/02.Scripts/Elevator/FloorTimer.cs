using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// “아무 일도 안 일어나길 기다리는 시간”
public class FloorTimer : MonoBehaviour
{
    float rideTime;

    void Start()
    {
        rideTime = Random.Range(10f, 30f);
        Invoke(nameof(FinishRide), rideTime);
    }

    void FinishRide()
    {
        FindObjectOfType<ElevatorController>().EndRide();
    }
}
