using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Startup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        cleanPersistentDataPath();
    }

    /// <summary>
    /// Clean the persistent data path.
    /// Deletes all folders and files in the persistent data path.
    /// </summary>
    private void cleanPersistentDataPath()
    {
        var directories = Directory.GetDirectories(Application.persistentDataPath);
        foreach (var directory in directories)
        {
            Directory.Delete(directory, true);
        }

        var files = Directory.GetFiles(Application.persistentDataPath);
        foreach (var file in files)
        {
            File.Delete(file);
        }
    }
}
