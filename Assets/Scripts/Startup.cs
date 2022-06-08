using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Startup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Set up variables in other files
        WFDSManager.persistentDataPath = Application.dataPath + "/PersistentData";
        WFDSManager.streamingAssetsPath = Application.streamingAssetsPath;
        WFDSManager.dataCollectionPath = Application.dataPath + "/DataCollection";
        WFDSManager.dataPath = Application.dataPath;

        cleanPersistentDataPath();
    }

    /// <summary>
    /// Clean the persistent data path.
    /// Deletes all folders and files in the persistent data path.
    /// </summary>
    private void cleanPersistentDataPath()
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
