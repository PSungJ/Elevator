using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UI ¡ﬂæ” ≈Î¡¶
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public PressureBorderUI pressureBorderUI;

    void Awake()
    {
        Instance = this;
    }
}
