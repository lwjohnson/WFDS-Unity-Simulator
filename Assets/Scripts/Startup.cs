using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Startup : MonoBehaviour
{
    public bool data_collection_mode = false;
    public float starting_time = 0;
    public float time_multiplier = 1;

    // Start is called before the first frame update
    void Start()
    {
        // Set up variables in other files
        WFDSManager.persistentDataPath = Application.dataPath + "/PersistentData";
        WFDSManager.streamingAssetsPath = Application.streamingAssetsPath;
        WFDSManager.dataCollectionPath = Application.dataPath + "/DataCollection";
        WFDSManager.dataPath = Application.dataPath;
        WFDSManager.data_collection_mode = data_collection_mode;

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
        DirectoryInfo persistentDataPath = new DirectoryInfo(WFDSManager.persistentDataPath);
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
