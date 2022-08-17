using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class SimulationManager : MonoBehaviour
{
    //change from unity Startup.cs
    public static bool data_collection_mode = false;
    public static bool uiOn = true;
    public static bool fds = false;

    public static string streamingAssetsPath = null;
    public static string persistentDataPath = null;
    public static string dataCollectionPath = null;
    public static string dataPath = null;

    public static int time_to_run = 0;
    public static bool wfds_run_once = false;
    public static bool wfds_setup = false;
    public static bool reading_fire = false;
    public static bool ready_to_read = false;
    public static bool read_fires_once = false;
    public static bool restart_guard = false;
    public static bool pause_guard = false;
    public static int slice_number;
    public static int level_set_mode = 1;
    // Update is called once per frame
    void Update()
    {

        if(!InteractionManager.interaction_done) {
            return;
        }

        if (wfds_setup && !VersionSwitcher.fds_running && !reading_fire && ready_to_read)
        {
            wfds_run_once = true;
            FireManager.readFireData();

            if(read_fires_once) {
                ready_to_read = false;
                VersionSwitcher.fds_running = true;
                VersionSwitcher.callFDS();
            }

            return;
        }

        if (!wfds_setup)
        {
            wfds_setup = true;
            SetupFileManager.setupInitialInputFile();
            VersionSwitcher.callFDS();
        }
    }

    void OnApplicationQuit()
    {
        VersionSwitcher.stopFDS();
    }
}
