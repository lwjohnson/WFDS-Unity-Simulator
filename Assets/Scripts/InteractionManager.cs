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

        float x = XR_Origin.transform.position.x;
        float z = XR_Origin.transform.position.z;

        // Instantiate fire
        if (Input.GetKey(KeyCode.F))
        {
            Vector3 point = TerrainManager.getNearestVector3(x, z);
            if (canInteractAt(point))
            {
                FireManager.createFireAt(point);
            }
        }

        // Instantiate Tree
        if (Input.GetKey(KeyCode.T))
        {
            Vector3 point = TerrainManager.getNearestVector3(x, z);
            if (canInteractAt(point))
            {
                TreeManager.createTreeAt(point);
            }
        }

        // Instantiate Trench
        if (Input.GetKey(KeyCode.G))
        {
            Vector3 point = TerrainManager.getNearestVector3(x, z);
            if (canInteractAt(point))
            {
                TrenchManager.createTrenchAt(point);
            }
        }

        // Delete GameObjects
        if (Input.GetKey(KeyCode.Backspace))
        {
            Vector3 point = TerrainManager.getNearestVector3(x, z);

            FireManager.removeFireAt(point);
            TreeManager.removeTreeAt(point);
            TrenchManager.removeTrenchAt(point);
        }

        // End Interaction (Calls WFDS After)
        if (Input.GetKey(KeyCode.R))
        {
            interaction_done = true;
        }
    }

    private bool canInteractAt(Vector3 point)
    {
        if (FireManager.fireExistsAt(point)) { return false; }
        if (TreeManager.treeExistsAt(point)) { return false; }
        if (TrenchManager.trenchExistsAt(point)) { return false; }

        return true;
    }
}
