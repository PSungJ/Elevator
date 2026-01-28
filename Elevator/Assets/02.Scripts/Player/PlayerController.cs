using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    public PlayerState currentState = PlayerState.Idle;

    GazeController gaze;
    ActionController action;

    void Awake()
    {
        Instance = this;
        gaze = GetComponent<GazeController>();
        action = GetComponent<ActionController>();
    }

    void Update()
    {
        HandleIdleStress();
    }

    void HandleIdleStress()
    {
        if (currentState == PlayerState.Idle)
        {
            MentalGauge mg = FindObjectOfType<MentalGauge>();
            mg.AddStress(Time.deltaTime * 0.5f); // 가만히 있어도 불안
        }
    }

    public void SetState(PlayerState state)
    {
        currentState = state;
    }
}
