
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseClockManager : MonoBehaviour
{
    public static Text status;

    // Start is called before the first frame update
    void Start()
    {
        status = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!SimulationManager.wfds_run_once){
            if(WFDSManager.wfds_running) {
                status.text = "Starting Simulation...";
            } else {
                status.text = "Start Simulation";
            } 
        } else if(SimulationManager.wfds_run_once){
            if(InteractionManager.interaction_done) {
                if(InteractionManager.pause_guard) {
                    status.text = "Pause blocked while FDS loads...";
                } else {
                    status.text = "Pause Available";
                }
            } else {
                if(InteractionManager.restart_guard) {
                    status.text = "Restart blocked while FDS loads...";
                } else {
                    status.text = "Restart Available";
                }
            }

        }   
    }
}