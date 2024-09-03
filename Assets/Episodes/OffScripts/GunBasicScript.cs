using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBasicScript : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 15f;
    
    public Camera fpsCam;
    public ParticleSystem muzzleFlash;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }
    
    void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);
            
            TargetBasicScript target = hit.transform.GetComponent<TargetBasicScript>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }
    }
}
