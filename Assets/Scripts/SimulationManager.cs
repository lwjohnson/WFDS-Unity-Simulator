using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class SimulationManager : MonoBehaviour
{
    public int time_to_run_inspector = 60;
    public static int time_to_run = 0;
    public static bool wfds_run_once = false;
    public static bool wfds_setup = false;

    // Start is called before the first frame update
    void Start()
    {
        time_to_run = time_to_run_inspector;
    }

    // Update is called once per frame
    void Update()
    {
        if (InteractionManager.interaction_done && !wfds_setup)
        {
            wfds_setup = true;
            setupInputFile();
            WFDSManager.callWFDS();
        }

        if (wfds_setup && !WFDSManager.wfds_running && !wfds_run_once)
        {
            wfds_run_once = true;

            FireManager.readFireData();
        }
    }

    void setupInputFile()
    {
        FileInfo map = new DirectoryInfo(Application.streamingAssetsPath).GetFiles("*.fds").FirstOrDefault();

        using StreamWriter writer = new StreamWriter(Application.persistentDataPath + @"\input.fds");
        using StreamReader reader = new StreamReader(map.OpenRead());

        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();

            if (line.Contains("&HEAD"))
            {
                line = "&HEAD CHID='input' /";
            }
            else if (line.Contains("&TIME T_END"))
            {
                line = $"&TIME T_END= {time_to_run} /";
            }
            else if (line.Contains("&TIME T_START"))
            {
                line = "&TIME T_START= 0 /";
            }
            else if (line.Contains("&DUMP"))
            {
                line = $"&DUMP DT_OUTPUT_LS={time_to_run}.0 /";
            }

            // For each of the remaining lines we need to see if InteractionManager.initial_fires contains a fire
            // at the same location as the line. If it does, then we need to add the fire to the input file.
            else if (line.Contains("&OBST"))
            {
                string[] split = TerrainManager.RemoveWhitespace(line).Replace("&OBSTXB=", string.Empty).Replace("/", string.Empty).Split(',');

                line = Regex.Replace(line, "'.*'", "'GRASS'"); // Initially set the OBST to GRASS

                foreach (GameObject fire in InteractionManager.initial_fires.ToList())
                {
                    if (fire.transform.position.x == float.Parse(split[1]) && fire.transform.position.z == float.Parse(split[3]))
                    {
                        line = Regex.Replace(line, "'.*'", "'FIRE'"); // Only if the user has set the cell to a fire do we set it to FIRE
                        InteractionManager.initial_fires.Remove(fire);
                    }
                }
            }

            writer.WriteLine(line);
        }
    }

    void OnApplicationQuit()
    {
        WFDSManager.stopWFDS();
    }
}
