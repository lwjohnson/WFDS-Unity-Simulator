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

            // Handle the GameObjects set by the user
            else if (line.Contains("&OBST"))
            {
                string[] split = TerrainManager.RemoveWhitespace(line).Replace("&OBSTXB=", string.Empty).Replace("/", string.Empty).Split(',');

                List<GameObject> fires = GameObject.FindGameObjectsWithTag("Fire").ToList();
                List<GameObject> trees = GameObject.FindGameObjectsWithTag("Tree").ToList();
                List<GameObject> trenches = GameObject.FindGameObjectsWithTag("Trench").ToList();

                // Initially set each line to Grass
                line = Regex.Replace(line, "'.*'", "'GRASS'");

                int x = int.Parse(split[1]);
                int y = int.Parse(split[3]);

                line = setOBSTLine(line, "FIRE", fires, x, y);
                line = setOBSTLine(line, "TREE", trees, x, y);
                line = setOBSTLine(line, "TRENCH", trenches, x, y);

            }

            writer.WriteLine(line);
        }
    }

    private string setOBSTLine(string line, string type, List<GameObject> objects, float x, float z)
    {
        foreach (GameObject obj in objects.ToList())
        {
            Vector3 transform = obj.transform.position;
            if (transform.x == x && transform.z == z)
            {
                objects.Remove(obj);
                return line = Regex.Replace(line, "'.*'", $"'{type}'");
            }
        }

        return line;
    }

    void OnApplicationQuit()
    {
        WFDSManager.stopWFDS();
    }
}
