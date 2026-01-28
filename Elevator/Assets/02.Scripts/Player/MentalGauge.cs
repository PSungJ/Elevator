using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MentalGauge : MonoBehaviour
{
    public float current;
    public float max = 100f;

    public void AddStress(float value)
    {
        current += value;
        if (current >= max)
        {
            GameManager.Instance.GameOver(false);
        }
    }
}
