using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Linq;

public class StartupManager : MonoBehaviour
{
    [Tooltip("The name of the file located in the streaming assets folder.")]
    public string map = "input_BT10m_2x2km_LS1.fds";
    [Tooltip("How many seconds make up a slice.")]
    public int slice_size = 20;
    [Tooltip("How many slices make up a chunk. A chunk is how long WFDS simulates in seconds. It is slice_size * slices_per_chunk.")]
    public int slices_per_chunk = 3;

    void Start()
    {
        Debug.Log("Starting Program");
        cleanPersistentDataPath();

        InputManager.slice_size = slice_size;
        InputManager.slices_per_chunk = slices_per_chunk;
        InputManager.writeInitialInput();

        WFDSManager.streamingAssetsPath = Application.streamingAssetsPath;
        WFDSManager.persistentDataPath = Application.persistentDataPath;
        Thread wfds_thread = new Thread(WFDSManager.startWFDS);
        wfds_thread.Start();

        FireManager.slice_size = slice_size;
    }

    void Update()
    {

    }

    void OnApplicationQuit()
    {
        WFDSManager.stopWFDS(); // Close WFDS when application is quit.
    }

    void cleanPersistentDataPath()
    {
        DirectoryInfo data_directory = new DirectoryInfo(Application.persistentDataPath);
        foreach (FileInfo old_data in data_directory.EnumerateFiles())
        {
            old_data.Delete(); // delete each file.
        }
        foreach (DirectoryInfo old_dir in data_directory.EnumerateDirectories())
        {
            old_dir.Delete(true); // recursively delete each directory and the files contained inside.
        }
    }
}
