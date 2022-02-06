using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBounds : MonoBehaviour
{
    private GameObject terrain;
    private GameObject XROrigin;
    private int xmax = 0;
    private int zmax = 0;
    private int offset = 5; // The offset from the edge of the terrain.

    // Start is called before the first frame update
    void Start()
    {
        terrain = GameObject.Find("Terrain Manager");
        XROrigin = GameObject.Find("XR Origin");
        xmax = TerrainManager.xmax;
        zmax = TerrainManager.zmax;
    }

    // Update is called once per frame
    void Update()
    {
        var transform = XROrigin.transform;

        var xmax = this.xmax - offset;
        var xmin = offset * 2;
        var zmax = this.zmax - offset;
        var zmin = offset * 2;

        if (transform.position.x >= xmax)
        {
            transform.position = new Vector3(xmax, transform.position.y, transform.position.z);
        }
        if (transform.position.x <= xmin)
        {
            transform.position = new Vector3(xmin, transform.position.y, transform.position.z);
        }

        if (transform.position.z >= zmax)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, zmax);
        }
        if (transform.position.z <= zmin)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, zmin);
        }
    }
}
