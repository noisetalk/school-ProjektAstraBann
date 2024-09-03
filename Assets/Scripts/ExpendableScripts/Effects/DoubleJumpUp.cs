using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleJumpUp : ExpendableObject
{
    public float timer = 0.0f;
    
public override void ExecuteEffect(GameObject other)
{
    PlayerController fpsc = other.GetComponent<PlayerController>(); // Skriptnamen anpassen
    if (fpsc != null)
    {
        fpsc.xJumpLimit += 100;
    }
}
public override void ExecuteRemovalStrategy() 
    {
        if (timer == 0) { Destroy(gameObject); }
        else { StartCoroutine(Respawn(timer)); }
    }
}