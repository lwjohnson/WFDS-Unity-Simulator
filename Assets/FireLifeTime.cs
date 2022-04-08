using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireLifeTime : MonoBehaviour
{
    public float lifetime = 30;

    // Update is called once per frame
    void Update()
    {
        if (!InteractionManager.interaction_done) { return; }

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
