using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public static class RestartManager
{
    public static int current_slice = InputManager.slice_size;
    public static string old_restart_file_path;

    public static void renameRestartToOldRestart() 
    {
        FileInfo restart_file = new DirectoryInfo(Application.persistentDataPath).GetFiles("current_test_0001.restart").FirstOrDefault();
        if (System.IO.File.Exists(Application.persistentDataPath + @"\current_test_0001.restart"))
        {
            old_restart_file_path = Application.persistentDataPath + @"\" + current_slice + ".old_restart";
            restart_file.MoveTo(old_restart_file_path);
            current_slice += InputManager.slice_size;
        }
    }
}
