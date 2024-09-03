using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Template : ExpendableObject    // "Template" durch Namen des Effekts ersetzen
{
    public float timer = 0.0f;
    
    public override void ExecuteEffect(GameObject other)
    {
        PlayerController fpsc = other.GetComponent<PlayerController>(); // Skriptnamen anpassen falls nötig
        if (fpsc != null)
        {
            // Expendable-Effekt hier rein
        }
    }
    // Zerstört das GameObject, wenn timer = 0 - Ansonsten wird es nach x Sekunden respawned
    public override void ExecuteRemovalStrategy() 
    {
        if (timer == 0) { Destroy(gameObject); }
        else { StartCoroutine(Respawn(timer)); }
    }
}

