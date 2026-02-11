using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FloorData
{
    public int floorNumber;

    [Header("NPC Spawn Count")]
    public int minNPCCount;
    public int maxNPCCount;

    [Header("Allowed NPC Types")]
    public NPCType[] allowedNPCTypes;
}
