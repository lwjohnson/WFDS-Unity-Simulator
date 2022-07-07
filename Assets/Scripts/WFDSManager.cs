using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System;

public static class WFDSManager
{
    public static string streamingAssetsPath = null;
    public static string persistentDataPath = null;
    public static string dataCollectionPath = null;
    public static string dataPath = null;

    public static bool data_collection_mode = false;

    public static bool wfds_running = false;
    public static int wfds_runs = 0;

    //bool fds = true for fds, false for wfds
    public static void callWFDS(bool fds = false)
    {
        Thread fds_thread;
        if(fds){
            fds_thread = new Thread(startFDS);
        } else {
            fds_thread = new Thread(startWFDS);
        }
        
        fds_thread.Start();
    }

    public static void startWFDS()
    {
        DateTime start = System.DateTime.Now;
        
        Process wfds_process = new Process();

        //Choose reference input or re-written input from persistent data path
        wfds_process.StartInfo.FileName = streamingAssetsPath + @"/wfds_run.exe";
        // if(!SimulationManager.wfds_run_once) {
            wfds_process.StartInfo.Arguments = "input.fds";
        // } else {
        //     wfds_process.StartInfo.Arguments = persistentDataPath + @"/input.fds";
        // }
        
        wfds_process.StartInfo.WorkingDirectory = persistentDataPath;
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
        InteractionManager.restart_guard = false;
        InteractionManager.pause_guard = false;

        DateTime end = System.DateTime.Now;

        if(data_collection_mode) {
            TimeSpan duration = end - start;

            string log = "Run time|Covered : " + SimulationManager.time_to_run + " : " + duration.TotalSeconds.ToString() + "|" + SimulationManager.time_to_run * (wfds_runs) + "|";

            File.AppendAllText(dataCollectionPath + @"/WFDS_Run_Logs.txt", log + Environment.NewLine);
        }
    }

    public static void startFDS()
    {
        DateTime start = System.DateTime.Now;
        
        Process fds_process = new Process();

        //Choose reference input or re-written input from persistent data path
        fds_process.StartInfo.FileName = streamingAssetsPath + @"/fds/fds_run.exe";
        if(!SimulationManager.wfds_run_once) {
            fds_process.StartInfo.Arguments = "/fds/input.fds";
        } else {
            fds_process.StartInfo.Arguments = persistentDataPath + @"/input.fds";
        }
        
        fds_process.StartInfo.WorkingDirectory = persistentDataPath;
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

        wfds_runs++;
        logMessage("ADDING RUNS" + wfds_runs);
        wfds_running = false;

        SimulationManager.ready_to_read = true;
        InteractionManager.restart_guard = false;
        InteractionManager.pause_guard = false;

        DateTime end = System.DateTime.Now;

        if(data_collection_mode) {
            TimeSpan duration = end - start;

            string log = "Run time|Covered : " + SimulationManager.time_to_run + " : " + duration.TotalSeconds.ToString() + "|" + SimulationManager.time_to_run * (wfds_runs) + "|";

            File.AppendAllText(dataCollectionPath + @"/WFDS_Run_Logs.txt", log + Environment.NewLine);
        }
    }



    //Called after a pause the start in the simulation, runs simulation to current
    // wallclock chunk so we can continue from current time
    public static void runCatchUp() {
        wfds_running = true;
        
        logMessage("STARTING CATCH UP");

        Process wfds_process = new Process();

        //Choose reference input or re-written input from persistent data path

        wfds_process.StartInfo.FileName = streamingAssetsPath + @"/wfds_run.exe";
        wfds_process.StartInfo.Arguments = persistentDataPath + @"/input.fds";
        
        
        wfds_process.StartInfo.WorkingDirectory = persistentDataPath;
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
        InteractionManager.pause_guard = false;
    }


    public static void logMessage(string message)
    {
        if (!String.IsNullOrEmpty(message))
        {
            UnityEngine.Debug.Log("WFDS: " + message);
        }
    }

    public static void stopFDS()
    {
        using (StreamWriter writer = new StreamWriter(WFDSManager.persistentDataPath + @"\" + "input" + ".stop")) { }
    }
}
