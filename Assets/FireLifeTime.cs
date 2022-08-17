using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireLifeTime : MonoBehaviour
{
    public float lifetime = 30;
    public bool static_fire = false;

    public float ignite_time = 0;

    public void swapFire(){
        Destroy(gameObject);
        FireManager.createFireAt(transform.position, true);
    }
}
