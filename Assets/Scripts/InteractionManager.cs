using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;

public class InteractionManager : MonoBehaviour
{
    public static bool interaction_done = false;
    GameObject XR_Origin = null;
    public GameObject rightPlaceMarker;
    public GameObject leftPlaceMarker;

    public static float placement_cooldown = 0.2f;
    public static float placement_cooldown_tracker = 0;

    public static bool catch_up_guard = false;
    public static GameObject[] firesTemp;
    void Start()
    {
        XR_Origin = GameObject.Find("XR Origin");
    }

    void Update()
    {   
        if(placement_cooldown_tracker > 0) { //Tracker to avoid instant placing a million items
            placement_cooldown_tracker -= Time.deltaTime;
        }

        if (interaction_done) { 
            //Pause simulation
            if (ControllerManager.menuPressed() && !SimulationManager.pause_guard) { //pause the simulation
                interaction_done = false;
                VersionSwitcher.stopFDS();
                VersionSwitcher.fds_runs = Mathf.FloorToInt(FireManager.wallclock_time / SimulationManager.time_to_run); //Gets current time chunk from wallclock
                SimulationManager.restart_guard = true;

                shiftFires();

                if(!VersionSwitcher.fds_running) {
                    SimulationManager.pause_guard = false;
                    SimulationManager.restart_guard = false;
                }
            }
            return; 
        }

        // End Interaction (Calls WFDS After)
        if (ControllerManager.menuPressed() && !SimulationManager.restart_guard && !SimulationManager.pause_guard)
        {            
            SimulationManager.pause_guard = true;

            if(SimulationManager.wfds_run_once && !catch_up_guard) { //restarting from 
                catch_up_guard = true;
                File.Delete(SimulationManager.persistentDataPath + @"\" + "input" + ".stop"); //remove stop file
                SetupFileManager.readFireDataFileSetup();

                Thread catchup = new Thread(catchUp);
                catchup.Start();
            } else {
                interaction_done = true;
            }
        }

        if(placement_cooldown_tracker <= 0 && !SimulationManager.pause_guard) { //can make a place/remove interaction

            // Instantiate fire
            if (ControllerManager.gripTriggerPressed(true)) { //right hand
                doInteraction(0, true);
            } else if(ControllerManager.gripTriggerPressed(false)) { //left hand
                doInteraction(0, false);
            }

            if(ControllerManager.primaryPressed(true)) { //right hand
                ItemManager.SwitchItem(true);
            } else if(ControllerManager.secondaryPressed(true)) { //right hand
                ItemManager.SwitchItem(false);
            } 
        }
    }

    // carries out an interaction with the environment
    // int interaction_type -> 0 = fire
    // bool right -> true for right hand, false for left
    private void doInteraction(int interaction_type, bool right) {
        Vector3 point;
        GameObject placeMarker;
        bool interaction_made = false;

        getInteractionPoint(right, out point, out placeMarker);

        switch(ItemManager.currently_selected_item) { //match cases with ItemManager.items array index
            case 0: //Place Fire
                if(canInteractAt(point)) {
                    FireManager.createFireAt(point);
                    interaction_made = true;
                }
                break;
            // case 1: //Place Tree
            //     if(placeMarker.active && canInteractAt(point)) {
            //         FireManager.createFireAt(point);
            //         interaction_made = true;
            //     }
            //     break;
            // case 2: //Place Trench
            //     if(placeMarker.active && canInteractAt(point)) {
            //         FireManager.createFireAt(point);
            //         interaction_made = true;
            //     }
            //     break;
            
        }

        if(interaction_made) {
            placement_cooldown_tracker = placement_cooldown;
        }
    }

    //stores point where interaction is made in point and returns placemarker of interaction
    //bool right -> true for right hand, false for left
    //Vector3 point -> Vector3 object, can be empty or not, must be from variable (out)
    //GameObject placeMarker -> GameObject object, can be empty or not, must be from variable (out)
    private void getInteractionPoint(bool right, out Vector3 point, out GameObject placeMarker) {
        Vector3[] mesh;

        if(right) {
            mesh = rightPlaceMarker.GetComponent<MeshFilter>().mesh.vertices;
            placeMarker = rightPlaceMarker;
        } else {
            mesh = leftPlaceMarker.GetComponent<MeshFilter>().mesh.vertices;
            placeMarker = leftPlaceMarker;
        }

        Vector3 bl = new Vector3(0, 0, 0);
        if(mesh.Length > 0) {
            bl = mesh[0];
        }
        
        point = TerrainManager.getNearestVector3(bl.x, bl.z);
    }

    //calls catch up function for when we restart after pause
    private static void catchUp() {
        VersionSwitcher.runCatchUp();
        catch_up_guard = false;
        interaction_done = true;            
    }

    private static void shiftFires() {
        firesTemp = GameObject.FindGameObjectsWithTag("Fire");
        foreach (GameObject fire in firesTemp)
        {
            fire.GetComponent<FireLifeTime>().swapFire();
        }
    }

    //determins if the point is valid for interaction
    //Vector3 point -> Vector3 with point to check
    private bool canInteractAt(Vector3 point)
    {
        if (FireManager.fireExistsAt(point)) { return false; }
        if (TreeManager.treeExistsAt(point)) { return false; }
        if (TrenchManager.trenchExistsAt(point)) { return false; }

        return true;
    }

}
