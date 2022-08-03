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
    public static bool wfds_running = false;
    public static int wfds_runs = 0;

    public static void callFDS()
    {
        Thread wfds_thread = new Thread(startWFDS);
        wfds_thread.Start();
    }

    public static void startWFDS()
    {
        wfds_running = true;
        DateTime start = System.DateTime.Now;
        
        Process wfds_process = new Process();

        //Choose reference input or re-written input from persistent data path
        wfds_process.StartInfo.FileName = SimulationManager.streamingAssetsPath + @"/wfds_run.exe";

        if(!SimulationManager.wfds_run_once) {
            wfds_process.StartInfo.Arguments = "input.fds";
        } else {
            wfds_process.StartInfo.Arguments = SimulationManager.persistentDataPath + @"/input.fds";
        }
        
        wfds_process.StartInfo.WorkingDirectory = SimulationManager.persistentDataPath;
        wfds_process.StartInfo.UseShellExecute = false;
        wfds_process.StartInfo.RedirectStandardOutput = true;
        wfds_process.StartInfo.RedirectStandardError = true;
        wfds_process.StartInfo.CreateNoWindow = true;

        // Set up redirected output to be displayed in the Unity console
        wfds_process.OutputDataReceived += (sender, e) =>
        {
            logMessage(e.Data);
        };

        wfds_process.ErrorDataReceived += (sender, e) =>
        {
            logMessage(e.Data);
        };

        // Start the process
        wfds_process.Start();

        // Start the asynchronous read of the streams
        wfds_process.BeginOutputReadLine();
        wfds_process.BeginErrorReadLine();

        wfds_process.WaitForExit();
        wfds_runs++;
        logMessage("ADDING RUNS" + wfds_runs);
        wfds_running = false;

        SimulationManager.ready_to_read = true;
        SimulationManager.restart_guard = false;
        SimulationManager.pause_guard = false;

        DateTime end = System.DateTime.Now;

        if(SimulationManager.data_collection_mode) {
            TimeSpan duration = end - start;

            string log = "Run time|Covered : " + SimulationManager.time_to_run + " : " + duration.TotalSeconds.ToString() + "|" + SimulationManager.time_to_run * (wfds_runs) + "|";

            File.AppendAllText(SimulationManager.dataCollectionPath + @"/WFDS_Run_Logs.txt", log + Environment.NewLine);
        }
    }


    //Called after a pause the start in the simulation, runs simulation to current
    // wallclock chunk so we can continue from current time
    public static void runCatchUp() {
        wfds_running = true;
        
        logMessage("STARTING CATCH UP");

        Process wfds_process = new Process();

        //Choose reference input or re-written input from persistent data path

        wfds_process.StartInfo.FileName = SimulationManager.streamingAssetsPath + @"/wfds_run.exe";
        wfds_process.StartInfo.Arguments = SimulationManager.persistentDataPath + @"/input.fds";
        
        
        wfds_process.StartInfo.WorkingDirectory = SimulationManager.persistentDataPath;
        wfds_process.StartInfo.UseShellExecute = false;
        wfds_process.StartInfo.RedirectStandardOutput = true;
        wfds_process.StartInfo.RedirectStandardError = true;
        wfds_process.StartInfo.CreateNoWindow = true;

        // Set up redirected output to be displayed in the Unity console
        wfds_process.OutputDataReceived += (sender, e) =>
        {
            logMessage(e.Data);
        };

        wfds_process.ErrorDataReceived += (sender, e) =>
        {
            logMessage(e.Data);
        };

        // Start the process
        wfds_process.Start();

        // Start the asynchronous read of the streams
        wfds_process.BeginOutputReadLine();
        wfds_process.BeginErrorReadLine();

        wfds_process.WaitForExit();
        wfds_runs++;
        wfds_running = false;
        SimulationManager.ready_to_read = true;
        SimulationManager.pause_guard = false;
    }


    public static void logMessage(string message)
    {
        if (!String.IsNullOrEmpty(message))
        {
            UnityEngine.Debug.Log("WFDS: " + message);
        }
    }

    public static void stopWFDS()
    {
        using (StreamWriter writer = new StreamWriter(SimulationManager.persistentDataPath + @"\" + "input" + ".stop")) { }
    }
}
