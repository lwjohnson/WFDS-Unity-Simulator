using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;
using System.Linq;

public class ContinuousSimulationManager : MonoBehaviour
{
    public static System.DateTime restart_file_time_to_skip; // the time that we wrote the current_test_0001.restart file
    public static bool skip = false; // skip is used to determine whether we should be moving current_test_0001.restart to slice.old_restart
    public static string restart_file_path; // path to the file that wfds output and is expecting

    // Start is called before the first frame update
    void Start()
    {
        restart_file_path = Application.persistentDataPath + @"\" + TerrainManager.chid + "_0001.restart";
    }

    // Update is called once per frame
    void Update()
    {
        if (!WFDSManager.wfds_running) // WFDS has finished running, get ready for the next chunk
        {
            if (System.IO.File.Exists(RestartManager.old_restart_file_path)) // ASSERT: the correct old_restart file (chunk_end.old_restart) exists 
            {
                Debug.Log("Getting WFDS ready to restart");

                FireManager.updateFireDictionary(); // Update the toa data for the fires. Will call on TOAManager to read the lstoa file

                if (RestartManager.old_restart_file_path != (Application.persistentDataPath + @"\" + InputManager.chunk_end + ".old_restart"))
                // This is to test if the .old_restart file that we were expecting did not exist
                {
                    Debug.Log("Moving: " + RestartManager.old_restart_file_path);
                    Debug.Log("Expected: " + Application.persistentDataPath + @"\" + InputManager.chunk_end + ".old_restart");
                }

                System.IO.File.Copy(RestartManager.old_restart_file_path, restart_file_path); // Copy the restart from the last chunk end to current_test_0001.restart as that is what wfds is looking for.
                restart_file_time_to_skip = System.IO.File.GetLastWriteTime(RestartManager.old_restart_file_path); // Get the time that we finished writing the file
                InputManager.writeRestartInput(); // Write restart input
                Thread wfds_thread = new Thread(WFDSManager.startWFDS);
                wfds_thread.Start(); // Start wfds in a new thread
                Debug.Log("WFDS has been started");
            }
        }
        else if (WFDSManager.wfds_running) // When WFDS is running we need to dynamically be capturing the restart files that it outputs.
        {
            if (System.IO.File.Exists(restart_file_path)) // ASSERT: the current_test_0001.restart file exists
            {
                if (!skip && restart_file_time_to_skip == System.IO.File.GetLastWriteTime(restart_file_path))
                // ASSERT: the "current_test_0001.restart" file that is there is the one we put there so we want to wait until the time it was written is different
                {
                    skip = true;
                }
                if (skip && (restart_file_time_to_skip != System.IO.File.GetLastWriteTime(restart_file_path)))
                // ASSERT: we don't want to rename this file because it is the one we wrote. We need to wait until wfds is done reading it, delete it, then we can continue the normal process
                {
                    try
                    {
                        System.IO.File.Delete(restart_file_path); // Delete "current_test_0001.restart" because wfds has read it, but has not written a new one yet.
                    }
                    catch (System.Exception) { } // We ignore this exception because we don't know when wfds will be done reading it.
                    if (!System.IO.File.Exists(restart_file_path)) // The file has been deleted
                    {
                        skip = false; // set skip to false since we do want to copy the next restart file output by wfds.
                    }
                }
            }
        }
        if (!skip && System.IO.File.Exists(restart_file_path) && (restart_file_time_to_skip != System.IO.File.GetLastWriteTime(restart_file_path)))
        // ASSERT: wfds wrote the current_test_0001.restart file so try renaming it
        {
            try
            {
                RestartManager.renameRestartToOldRestart(); // try to rename the file to slice.old_restart (can only happen once wfds is done writing to it)
            }
            catch (System.Exception) { } // We ignore this exception because the function constantly tries to move the file, but it is only able to once WFDS is done writing to it. 
        }
    }
}
