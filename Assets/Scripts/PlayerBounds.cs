using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBounds : MonoBehaviour
{
    private GameObject terrain;
    private GameObject XROrigin;
    private int xmax = 0;
    private int ymax = 0;
    private int zmax = 0;
    private int xmin = 0;
    private int ymin = 0;
    private int zmin = 0;
    private int offset = 5; // The offset from the edge of the terrain.

    // Start is called before the first frame update
    void Start()
    {
        terrain = GameObject.Find("Ground");
        XROrigin = GameObject.Find("XR Origin");

        xmax = TerrainManager.xmax - offset;
        ymax = TerrainManager.ymax + offset;
        zmax = TerrainManager.zmax - offset;
        xmin = TerrainManager.xmin + (offset * 2) + offset; // Because we are missing the first row and column of the terrain.
        ymin = TerrainManager.ymin - offset;
        zmin = TerrainManager.zmin + (offset * 2) + offset; // Because we are missing the first row and column of the terrain.
    }

    // Update is called once per frame
    void Update()
    {
        Transform transform = XROrigin.transform;

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

        if (transform.position.y <= ymin)
        {
            Vector3 new_position = TerrainManager.getNearestVector3(transform.position.x, transform.position.z);
            new_position.y += 20; // Move the player up a bit so that they are not clipping through the terrain. This happens on hills.
            transform.position = new_position;
        }
    }
}
