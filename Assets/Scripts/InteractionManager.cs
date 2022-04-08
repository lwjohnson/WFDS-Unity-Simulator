using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static bool interaction_done = false;
    GameObject XR_Origin = null;

    [SerializeField]
    [Tooltip("The prefab for the fire")]
    private GameObject firePrefab;

    // Start is called before the first frame update
    void Start()
    {
        XR_Origin = GameObject.Find("XR Origin");
    }

    // Update is called once per frame
    void Update()
    {
        if (interaction_done) { return; }

        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 point = TerrainManager.getNearestVector3(XR_Origin.transform.position.x, XR_Origin.transform.position.z);
            if (!fireExistsAt(point))
            {
                GameObject new_fire = Instantiate(firePrefab, point, Quaternion.identity);
                new_fire.transform.localScale = new Vector3(TerrainManager.cellsize, TerrainManager.cellsize, TerrainManager.cellsize);
            }
        }
        if (Input.GetKey(KeyCode.Backspace))
        {
            Vector3 point = TerrainManager.getNearestVector3(XR_Origin.transform.position.x, XR_Origin.transform.position.z);
            if (fireExistsAt(point))
            {
                GameObject fire = getFireAt(point);
                Destroy(fire);
            }
        }
        if (Input.GetKey(KeyCode.R))
        {
            interaction_done = true;
        }
    }

    bool fireExistsAt(Vector3 point)
    {
        foreach (GameObject fire in GameObject.FindGameObjectsWithTag("Fire"))
        {
            if (fire.transform.position.x == point.x && fire.transform.position.z == point.z)
            {
                return true;
            }
        }
        return false;
    }

    GameObject getFireAt(Vector3 point)
    {
        foreach (GameObject fire in GameObject.FindGameObjectsWithTag("Fire"))
        {
            if (fire.transform.position.x == point.x && fire.transform.position.z == point.z)
            {
                return fire;
            }
        }
        return null;
    }
}
