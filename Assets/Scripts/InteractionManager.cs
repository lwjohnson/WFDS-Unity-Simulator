using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static bool interaction_done = false;
    GameObject XR_Origin = null;

    void Start()
    {
        XR_Origin = GameObject.Find("XR Origin");
    }

    void Update()
    {
        if (interaction_done) { return; }

        // Instantiate fire
        if (Input.GetKey(KeyCode.F))
        {
            Vector3 point = TerrainManager.getNearestVector3(XR_Origin.transform.position.x, XR_Origin.transform.position.z);
            if (!FireManager.fireExistsAt(point) && !TreeManager.treeExistsAt(point))
            {
                FireManager.createFireAt(point);
            }
        }

        // Instantiate Tree
        if (Input.GetKey(KeyCode.T))
        {
            Vector3 point = TerrainManager.getNearestVector3(XR_Origin.transform.position.x, XR_Origin.transform.position.z);
            if (!TreeManager.treeExistsAt(point) && !FireManager.fireExistsAt(point))
            {
                TreeManager.createTreeAt(point);
            }
        }

        // Delete GameObjects
        if (Input.GetKey(KeyCode.Backspace))
        {
            Vector3 point = TerrainManager.getNearestVector3(XR_Origin.transform.position.x, XR_Origin.transform.position.z);
            if (FireManager.fireExistsAt(point))
            {
                GameObject fire = FireManager.getFireAt(point);
                Destroy(fire);
            }

            if (TreeManager.treeExistsAt(point))
            {
                GameObject tree = TreeManager.getTreeAt(point);
                Destroy(tree);
            }
        }

        // End Interaction (Calls WFDS After)
        if (Input.GetKey(KeyCode.R))
        {
            interaction_done = true;
        }
    }
}
