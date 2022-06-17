using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireLifeTime : MonoBehaviour
{
    public float lifetime = 30;
    public bool static_fire = false;

    public float ignite_time = 0;

    // Update is called once per frame
    void Update()
    {
        if(!static_fire){
            if (FireManager.wallclock_time <= 0) { return; }

            if (lifetime > 0)
            {
                lifetime -= Time.deltaTime;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
