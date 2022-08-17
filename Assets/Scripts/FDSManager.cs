using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System;

public static class FDSManager
{
 public static void startFDS()
    {
        DateTime start = System.DateTime.Now;
        
        Process fds_process = new Process();

        fds_process.StartInfo.FileName = SimulationManager.streamingAssetsPath + @"/fds/fds_run.exe";
        fds_process.StartInfo.Arguments = SimulationManager.persistentDataPath + @"/input.fds";
        
        fds_process.StartInfo.WorkingDirectory = SimulationManager.persistentDataPath;
        fds_process.StartInfo.UseShellExecute = false;
        fds_process.StartInfo.RedirectStandardOutput = true;
        fds_process.StartInfo.RedirectStandardError = true;
        fds_process.StartInfo.CreateNoWindow = true;

        // Set up redirected output to be displayed in the Unity console
        fds_process.OutputDataReceived += (sender, e) =>
        {
            logMessage(e.Data);
        };

        fds_process.ErrorDataReceived += (sender, e) =>
        {
            logMessage(e.Data);
        };

        // Start the process
        fds_process.Start();

        // Start the asynchronous read of the streams
        fds_process.BeginOutputReadLine();
        fds_process.BeginErrorReadLine();
        fds_process.WaitForExit();

        VersionSwitcher.fds_runs++;
        logMessage("ADDING RUNS" + VersionSwitcher.fds_runs);
        VersionSwitcher.fds_running = false;

        SimulationManager.ready_to_read = true;
        SimulationManager.restart_guard = false;
        SimulationManager.pause_guard = false;

        DateTime end = System.DateTime.Now;

        if(SimulationManager.data_collection_mode) {
            TimeSpan duration = end - start;

            string log = "Run time|Covered : " + SimulationManager.time_to_run + " : " + duration.TotalSeconds.ToString() + "|" + SimulationManager.time_to_run * (VersionSwitcher.fds_runs) + "|";

            File.AppendAllText(SimulationManager.dataCollectionPath + @"/FDS_Run_Logs.txt", log + Environment.NewLine);
        }
    }


    //Called after a pause the start in the simulation, runs simulation to current
    // wallclock chunk so we can continue from current time
    public static void runCatchUp() {
        VersionSwitcher.fds_running = true;
        
        logMessage("STARTING CATCH UP");

        Process fds_process = new Process();

        fds_process.StartInfo.FileName = SimulationManager.streamingAssetsPath + @"/fds/fds_run.exe";
        fds_process.StartInfo.Arguments = SimulationManager.persistentDataPath + @"/input.fds";
        
        fds_process.StartInfo.WorkingDirectory = SimulationManager.persistentDataPath;
        fds_process.StartInfo.UseShellExecute = false;
        fds_process.StartInfo.RedirectStandardOutput = true;
        fds_process.StartInfo.RedirectStandardError = true;
        fds_process.StartInfo.CreateNoWindow = true;

        // Set up redirected output to be displayed in the Unity console
        fds_process.OutputDataReceived += (sender, e) =>
        {
            logMessage(e.Data);
        };

        fds_process.ErrorDataReceived += (sender, e) =>
        {
            logMessage(e.Data);
        };

        // Start the process
        fds_process.Start();

        // Start the asynchronous read of the streams
        fds_process.BeginOutputReadLine();
        fds_process.BeginErrorReadLine();

        fds_process.WaitForExit();
        VersionSwitcher.fds_runs++;
        VersionSwitcher.fds_running = false;
        SimulationManager.ready_to_read = true;
        SimulationManager.pause_guard = false;
    }


    public static void logMessage(string message)
    {
        if (!String.IsNullOrEmpty(message))
        {
            UnityEngine.Debug.Log("FDS: " + message);
        }
    }

}
