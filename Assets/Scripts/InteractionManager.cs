using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class InteractionManager : MonoBehaviour
{
    public static bool interaction_done = false;
    GameObject XR_Origin = null;
    public GameObject rightPlaceMarker;
    public GameObject leftPlaceMarker;
    public InputDevice leftController;
    public InputDevice rightController;

    public static float placement_cooldown = 0.2f;
    public static float placement_cooldown_tracker = 0;
    public static float restart_safety_time = 10;
    public static float restart_safety_tracker = 0;

    void Start()
    {
        XR_Origin = GameObject.Find("XR Origin");
        // leftController = InputDevices.GetDeviceAtXRNode(leftHand);
    }

    void Update()
    {
        if(restart_safety_tracker > 0) { //Tracker to avoid W/FDS opening files still in use
            restart_safety_tracker -= Time.deltaTime;
        }

        if(placement_cooldown_tracker > 0) { //Tracker to avoid W/FDS opening files still in use
            placement_cooldown_tracker -= Time.deltaTime;
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

        // if (Input.GetButtonDown(1)){
        // }

        // Instantiate fire
        if (Input.GetKey(KeyCode.F))
        {
            doInteraction(0);
        }

        // // Instantiate Tree
        // if (Input.GetKey(KeyCode.T))
        // {
        //     Vector3 point = TerrainManager.getNearestVector3(x, z);
        //     if (canInteractAt(point))
        //     {
        //         TreeManager.createTreeAt(point);
        //     }
        // }

        // // Instantiate Trench
        // if (Input.GetKey(KeyCode.G))
        // {
        //     Vector3 point = TerrainManager.getNearestVector3(x, z);
        //     if (canInteractAt(point))
        //     {
        //         TrenchManager.createTrenchAt(point);
        //     }
        // }

        // // Delete GameObjects
        // if (Input.GetKey(KeyCode.Backspace))
        // {
        //     Vector3 point = TerrainManager.getNearestVector3(x, z);

        //     FireManager.removeFireAt(point);
        //     TreeManager.removeTreeAt(point);
        //     TrenchManager.removeTrenchAt(point);
        // }

        // End Interaction (Calls WFDS After)
        if (Input.GetKey(KeyCode.R) && restart_safety_tracker <= 0)
        {            
            if(SimulationManager.wfds_run_once) { //restarting from 
                File.Delete(WFDSManager.persistentDataPath + @"\" + "input" + ".stop"); //remove stop file
                FireManager.setupInputFile();

                Thread catchup = new Thread(catchUp);
                catchup.Start();
            } else { // initial run
                interaction_done = true;
            }            
        }
    }

    private void doInteraction(int interaction_type) {

        if(placement_cooldown_tracker > 0) {
            return;
        }

        float rx = rightPlaceMarker.transform.position.x;
        float rz = rightPlaceMarker.transform.position.z;
        float lx = leftPlaceMarker.transform.position.x;
        float lz = leftPlaceMarker.transform.position.z;

        Vector3 point = TerrainManager.getNearestVector3(rx, rz);

        bool interaction_made = false;

        if (canInteractAt(point)) {
            if(interaction_type == 0 && rightPlaceMarker.active) {
                FireManager.createFireAt(point);
                placement_cooldown_tracker = placement_cooldown;
                interaction_made = true;
            }
        }

        if(interaction_made) {
            placement_cooldown_tracker = placement_cooldown;
        }
    }

    private static void catchUp() {
        WFDSManager.runCatchUp();
    }

    private bool canInteractAt(Vector3 point)
    {
        if (FireManager.fireExistsAt(point)) { return false; }
        if (TreeManager.treeExistsAt(point)) { return false; }
        if (TrenchManager.trenchExistsAt(point)) { return false; }

        return true;
    }
}
