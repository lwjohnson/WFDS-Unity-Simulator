using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class SimulationManager : MonoBehaviour
{
    public int time_to_run_inspector = 60;
    public bool data_collection_mode = false;

    public static int time_to_run = 0;
    public static bool wfds_run_once = false;
    public static bool wfds_setup = false;
    public static bool reading_fire = false;
    public static bool ready_to_read = false;


    // Start is called before the first frame update
    void Start()
    {
        time_to_run = time_to_run_inspector;
    }

    // Update is called once per frame
    void Update()
    {

        if(!InteractionManager.interaction_done) {
            return;
        }

        if (wfds_setup && !WFDSManager.wfds_running && !reading_fire && ready_to_read)
        {
            wfds_run_once = true;

            FireManager.readFireData();

            if(FireManager.read_fires_once) {
                ready_to_read = false;
                WFDSManager.wfds_running = true;
                WFDSManager.callWFDS();
            }

            return;
        }

        if (!wfds_setup)
        {
            wfds_setup = true;
            SetupFileManager.setupInitialInputFile();
            WFDSManager.callWFDS();
        }
    }

    void OnApplicationQuit()
    {
        WFDSManager.stopWFDS();
    }
}
