using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class InteractionManager : MonoBehaviour
{
    public static bool interaction_done = false;
    GameObject XR_Origin = null;

    public static float restart_safety_time = 10;
    public static float restart_safety_tracker = 0;

    void Start()
    {
        XR_Origin = GameObject.Find("XR Origin");
    }

    void Update()
    {
        if(restart_safety_tracker > 0) { //Tracker to avoid W/FDS opening files still in use
            restart_safety_tracker -= Time.deltaTime;
        }

        if (interaction_done) { 
            //Pause simulation
            if (Input.GetKey(KeyCode.P)) {
                interaction_done = false;
                WFDSManager.stopWFDS();
                WFDSManager.wfds_runs = Mathf.FloorToInt(FireManager.wallclock_time / SimulationManager.time_to_run); //Gets current time chunk from wallclock
                restart_safety_tracker = restart_safety_time;
            }
            return; 
        }

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
        if (Input.GetKey(KeyCode.R) && restart_safety_tracker <= 0)
        {            
            if(SimulationManager.wfds_run_once) { //restarting from 
                File.Delete(WFDSManager.persistentDataPath + @"\" + "input" + ".stop"); //remove stop file
                FireManager.setupInputFile();
                WFDSManager.runCatchUp();
            } else { // initial run
                interaction_done = true;
            }            
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
