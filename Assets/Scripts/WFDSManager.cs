using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Linq;

public static class WFDSManager
{
    public static string streamingAssetsPath = null;
    public static string persistentDataPath = null;
    public static bool wfds_running = false;

    public static void callWFDS()
    {
        Thread wfds_thread = new Thread(startWFDS);
        wfds_thread.Start();
    }

    public static void startWFDS()
    {
        wfds_running = true;

        Process wfds_process = new Process();
        wfds_process.StartInfo.FileName = streamingAssetsPath + @"\wfds.exe";
        wfds_process.StartInfo.Arguments = persistentDataPath + @"\wfds_input.fds";
        wfds_process.StartInfo.UseShellExecute = false;
        wfds_process.StartInfo.RedirectStandardOutput = true;
        wfds_process.StartInfo.RedirectStandardError = true;
        // wfds_process.StartInfo.CreateNoWindow = true; // TODO: Uncomment this line to show the window

        wfds_process.Start();

        string output = wfds_process.StandardOutput.ReadToEnd();
        string error = wfds_process.StandardError.ReadToEnd();

        wfds_process.WaitForExit();

        wfds_running = false;

        UnityEngine.Debug.Log("WFDS OUTPUT: " + output);
        UnityEngine.Debug.Log("WFDS ERROR:  " + error);
    }

    public static void stopWFDS()
    {
        using (StreamWriter stopper = new StreamWriter(Application.persistentDataPath + @"\" + "current_test" + ".stop")) { }
    }

    public static void deleteStopFile()
    {
        FileInfo stop_file = new DirectoryInfo(Application.persistentDataPath).GetFiles("*.stop").FirstOrDefault();
        if (stop_file.Exists)
        {
            stop_file.Delete();
        }
    }
}
