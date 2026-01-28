using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ¹® ¿­¸²/´ÝÈû
// Èçµé¸² ¿¬Ãâ
public class ElevatorController : MonoBehaviour
{
    public void StartRide()
    {
        // ¹® ´ÝÈû ¾Ö´Ï¸ÞÀÌ¼Ç
    }

    public void EndRide()
    {
        GameManager.Instance.GameOver(true);
    }
}
