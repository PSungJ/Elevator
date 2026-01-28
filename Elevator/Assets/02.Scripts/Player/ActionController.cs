using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionController : MonoBehaviour
{
    public void UsePhone()
    {
        PlayerController.Instance.SetState(PlayerState.UsingPhone);
        Invoke(nameof(StopPhone), 2f);
    }

    void StopPhone()
    {
        PlayerController.Instance.SetState(PlayerState.Idle);
    }

    public void PressButton()
    {
        PlayerController.Instance.SetState(PlayerState.PressingBtn);
        Invoke(nameof(StopButton), 0.5f);
    }

    void StopButton()
    {
        PlayerController.Instance.SetState(PlayerState.Idle);
    }
}
