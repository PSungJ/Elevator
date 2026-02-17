using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GimmickNPC : BaseNPC
{
    public override bool CanInteract => isActive;

    public abstract void OnArrivedInElevator();
}
