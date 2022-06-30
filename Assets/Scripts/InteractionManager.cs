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
    public static float restart_safety_time = 10; // time until can restart simulation
    public static float restart_safety_tracker = 0;

    public static bool restart_guard = false;
    public static bool pause_guard = false;
    public static bool catch_up_guard = false;

    void Start()
    {
        XR_Origin = GameObject.Find("XR Origin");
    }

    void Update()
    {   
        Debug.Log("Restart guard: " + restart_guard);     
        Debug.Log("Pause guard: " + pause_guard);


        if(placement_cooldown_tracker > 0) { //Tracker to avoid instant placing a million items
            placement_cooldown_tracker -= Time.deltaTime;
        }

        if (interaction_done) { 
            //Pause simulation
            if (Input.GetKey(KeyCode.P) || ControllerManager.menuPressed() && !pause_guard) { //pause the simulation
                interaction_done = false;
                WFDSManager.stopWFDS();
                WFDSManager.wfds_runs = Mathf.FloorToInt(FireManager.wallclock_time / SimulationManager.time_to_run); //Gets current time chunk from wallclock
                restart_guard = true;

                if(!WFDSManager.wfds_running) {
                    pause_guard = false;
                    restart_guard = false;
                }
            }
            return; 
        }

        // End Interaction (Calls WFDS After)
        if ((Input.GetKey(KeyCode.R) || ControllerManager.menuPressed()) && !restart_guard && !pause_guard)
        {            
            pause_guard = true;

            if(SimulationManager.wfds_run_once && !catch_up_guard) { //restarting from 
                catch_up_guard = true;
                File.Delete(WFDSManager.persistentDataPath + @"\" + "input" + ".stop"); //remove stop file
                FireManager.setupInputFile();

                Thread catchup = new Thread(catchUp);
                catchup.Start();
            } else {
                interaction_done = true;
            }
        }

        if(placement_cooldown_tracker <= 0) { //can make a place/remove interaction

            // Instantiate fire
            if (Input.GetKey(KeyCode.F) || ControllerManager.gripTriggerPressed(true)) { //right hand
                doInteraction(0, true);
            } else if(ControllerManager.gripTriggerPressed(false)) { //left hand
                doInteraction(0, false);
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

        switch(interaction_type) {
            case 0: //Place Fire
                if(placeMarker.active && canInteractAt(point)) {
                    FireManager.createFireAt(point);
                    interaction_made = true;
                }
                break;
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
        WFDSManager.runCatchUp();
        catch_up_guard = false;
        interaction_done = true;            
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
