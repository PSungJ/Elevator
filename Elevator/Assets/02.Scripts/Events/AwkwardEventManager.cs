using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ·£´ý ÀÌº¥Æ® ¹ßµ¿
public class AwkwardEventManager : MonoBehaviour
{
    void Start()
    {
        InvokeRepeating(nameof(TriggerEvent), 5f, 7f);
    }

    void TriggerEvent()
    {
        // ±âÄ§, Èçµé¸², Ä§¹¬
    }
}
