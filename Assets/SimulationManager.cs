using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Linq;

public class SimulationManager : MonoBehaviour
{
    public static int time_to_run = 30;
    public static bool wfds_run_once = false;

    // Start is called before the first frame update
    void Start()
    {
        setupInputFile();
        WFDSManager.callWFDS();
    }

    // Update is called once per frame
    void Update()
    {
        if (!WFDSManager.wfds_running && !wfds_run_once)
        {
            wfds_run_once = true;

            FireManager.readFireData();
        }
    }

    void setupInputFile()
    {
        FileInfo map = new DirectoryInfo(Application.streamingAssetsPath).GetFiles("*.fds").FirstOrDefault(); // TODO: Get the correct map from the user
        UnityEngine.Debug.Log("Map: " + map.FullName);

        using StreamWriter writer = new StreamWriter(Application.persistentDataPath + @"\input.fds");
        using StreamReader reader = new StreamReader(map.OpenRead());

        string line;
        do
        {
            line = reader.ReadLine();

            if (line.Contains("&HEAD"))
            {
                line = "&HEAD CHID='input' /";
            }
            else if (line.Contains("&TIME T_END"))
            {
                line = "&TIME T_END= " + time_to_run + " /";
            }
            else if (line.Contains("&TIME T_START"))
            {
                line = "&TIME T_START= 0 /";
            }

            writer.WriteLine(line);
        } while (!line.Contains("&OBST"));

        // Write the rest of the file after &OBST
        writer.Write(reader.ReadToEnd());
    }

    void OnApplicationQuit()
    {
        WFDSManager.stopWFDS();
    }
}
