using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System;

public static class VersionSwitcher
{
    public static bool fds_running = false;
    public static int fds_runs = 0;

    public static void callFDS()
    {
        fds_running = true;
        Thread thread;
        if(SimulationManager.fds) {
            thread = new Thread(FDSManager.startFDS);
        } else {
            thread = new Thread(WFDSManager.startWFDS);
        }

        thread.Start();
    }

    //Called after a pause the start in the simulation, runs simulation to current
    // wallclock chunk so we can continue from current time
    public static void runCatchUp() {
        if(SimulationManager.fds) {
            FDSManager.runCatchUp();
        } else {
            WFDSManager.runCatchUp();
        }
    }

    public static void stopFDS()
    {
        using (StreamWriter writer = new StreamWriter(SimulationManager.persistentDataPath + @"\" + "input" + ".stop")) { }
    }
}
