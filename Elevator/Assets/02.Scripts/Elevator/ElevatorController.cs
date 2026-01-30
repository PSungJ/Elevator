using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ¹® ¿­¸²/´ÝÈû
// Èçµé¸² ¿¬Ãâ
public class ElevatorController : MonoBehaviour
{
    public List<NPCController> npcList;
    public float enterDelay = 0.8f;
    Animator ani;

    void Start()
    {
        ani = GetComponent<Animator>();
        StartCoroutine(ElevatorArrivedSequence());
    }

    IEnumerator ElevatorArrivedSequence()
    {
        OpenDoor();
        yield return new WaitForSeconds(enterDelay);

        NPCController npc = SelectNPC();
        if (npc != null)
            npc.EnterElevator();

        yield return new WaitForSeconds(2f);
        CloseDoor();
    }

    NPCController SelectNPC()
    {
        if (npcList.Count == 0) return null;

        // ·£´ý
        int index = Random.Range(0, npcList.Count);
        return npcList[index];
    }

    void OpenDoor() 
    {
        ani.SetBool("isOpen", true);
    }
    void CloseDoor() 
    {
        ani.SetBool("isOpen", false);
    }
}
