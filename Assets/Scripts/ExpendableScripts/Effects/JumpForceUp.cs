using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpForceUp : ExpendableObject
{
    public float timer = 0.0f;
    public int modJuFo = 2;

    public override void ExecuteEffect(GameObject other)
    {
        PlayerController fpsc = other.GetComponent<PlayerController>(); // Skriptnamen anpassen
        if (fpsc != null)
        {
            fpsc.jumpForce *= modJuFo;
        }
    }
    public override void ExecuteRemovalStrategy() 
    {
        if (timer == 0) { Destroy(gameObject); }
        else { StartCoroutine(Respawn(timer)); }
    }
}