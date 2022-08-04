using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Startup : MonoBehaviour
{
    public bool data_collection_mode = false;
    public float starting_time = 0;
    public float time_multiplier = 1;
    public int time_to_run_inspector = 60;
    public bool uiOn = true;
    public bool fds = false;

    // Start is called before the first frame update
    void Start()
    {
        // Set up variables in other files
        SimulationManager.persistentDataPath = Application.dataPath + "/PersistentData";
        SimulationManager.streamingAssetsPath = Application.streamingAssetsPath;
        SimulationManager.dataCollectionPath = Application.dataPath + "/DataCollection";
        SimulationManager.dataPath = Application.dataPath;

        SimulationManager.time_to_run = time_to_run_inspector;
        SimulationManager.data_collection_mode = data_collection_mode;
        SimulationManager.uiOn = uiOn;
        SimulationManager.fds = fds;
        Debug.Log(fds);

        ItemManager.uiOn = uiOn;

        FireManager.starting_time = starting_time;
        FireManager.time_multiplier = time_multiplier;
        
        cleanPersistentDataPath();
    }

    /// <summary>
    /// Clean the persistent data path.
    /// Deletes all folders and files in the persistent data path.
    /// </summary>
    public static void cleanPersistentDataPath()
    {
        DirectoryInfo persistentDataPath = new DirectoryInfo(SimulationManager.persistentDataPath);
        foreach (FileInfo file in persistentDataPath.GetFiles())
        {
            file.Delete();
        }
        foreach (DirectoryInfo dir in persistentDataPath.GetDirectories())
        {
            dir.Delete(true);
        }
    }
}
