using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Linq;

public static class WFDSManager
{
    public static string streamingAssetsPath;
    public static string persistentDataPath;
    public static bool wfds_running = false;

    public static void startWFDS()
    {
        UnityEngine.Debug.Log("Starting WFDS");
        wfds_running = true;

        Process wfds = new Process(); // create a new process
        wfds.StartInfo.FileName = streamingAssetsPath + @"\wfds9977_win_64.exe"; //wfds exe
        wfds.StartInfo.Arguments = new DirectoryInfo(persistentDataPath).GetFiles("*.fds").FirstOrDefault().Name; // which file to open
        wfds.StartInfo.UseShellExecute = true;
        wfds.StartInfo.WorkingDirectory = persistentDataPath; // where WFDS will work
        wfds.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; //uncomment to hide WFDS window
        wfds.StartInfo.CreateNoWindow = true;
        wfds.Start(); // Start the process
        wfds.WaitForExit(); // Wait until WFDS exits

        wfds_running = false;
        UnityEngine.Debug.Log("Exited WFDS");
    }

    public static void stopWFDS()
    {
        using (StreamWriter stopper = new StreamWriter(Application.persistentDataPath + @"\" + "current_test" + ".stop")) { } // replace current_test with the CHID from .fds file
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
