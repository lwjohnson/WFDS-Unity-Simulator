using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class SimulationManager : MonoBehaviour
{
    public int time_to_run_inspector = 60;
    public bool data_collection_mode = false;

    public static int time_to_run = 0;
    public static bool wfds_run_once = false;
    public static bool wfds_setup = false;
    public static bool reading_fire = false;
    public static bool ready_to_read = false;

    public static SortedDictionary<string, string> fireSURF = new SortedDictionary<string, string>();
    public static List<GameObject> fires;
    public static List<GameObject> trees;
    public static List<GameObject> trenches;
    public static List<GameObject> objects;

    // Start is called before the first frame update
    void Start()
    {
        time_to_run = time_to_run_inspector;
    }

    // Update is called once per frame
    void Update()
    {

        if(!InteractionManager.interaction_done) {
            return;
        }

        if (wfds_setup && !WFDSManager.wfds_running && !reading_fire && ready_to_read)
        {
            wfds_run_once = true;

            FireManager.readFireData();

            if(FireManager.read_fires_once) {
                ready_to_read = false;
                WFDSManager.wfds_running = true;
                WFDSManager.callWFDS();
            }

            return;
        }

        if (!wfds_setup)
        {
            wfds_setup = true;
            setupInputFile();
            WFDSManager.callWFDS();
        }
    }

    void setupInputFile()
    {
        FileInfo map = new DirectoryInfo(Application.streamingAssetsPath).GetFiles("*.fds").FirstOrDefault();

        using StreamWriter writer = new StreamWriter(WFDSManager.persistentDataPath + @"\input.fds");
        using StreamReader reader = new StreamReader(map.OpenRead());

        fires = GameObject.FindGameObjectsWithTag("Fire").ToList();
        trees = GameObject.FindGameObjectsWithTag("Tree").ToList();
        trenches = GameObject.FindGameObjectsWithTag("Trench").ToList();
        objects = fires.Concat(trees).Concat(trenches).ToList();

        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();

            if (line.Contains("&HEAD"))
            {
                line = "&HEAD CHID='input' /";
            }
            else if (line.Contains("&TIME T_END"))
            {
                line = $"&TIME T_END= {FireManager.starting_time + time_to_run * (WFDSManager.wfds_runs + 1)} /";
            }
            else if (line.Contains("&TIME T_START"))
            {
                line = "&TIME T_START= 0 /";
            }
            else if (line.Contains("&DUMP"))
            {
                line = $"&DUMP DT_OUTPUT_LS={time_to_run}.0 /";
            }
            else if (line.Contains("&SURF") && line.Contains("VEG_LSET_IGNITE_TIME")) {
                line = setupFireSurface(line);
            }
            // Handle the GameObjects set by the user
            else if (line.Contains("&OBST"))
            {
                string[] split = TerrainManager.RemoveWhitespace(line).Replace("&OBSTXB=", string.Empty).Replace("/", string.Empty).Split(',');

                // Initially set each line to Grass
                line = Regex.Replace(line, "'.*'", "'GRASS'");

                int x = int.Parse(split[1]);
                int y = int.Parse(split[3]);

                line = setOBSTLine(line, x, y);
            }

            writer.WriteLine(line);
        }
    }

    private string setupFireSurface(string line) 
    {
        line = TerrainManager.RemoveQuotes(TerrainManager.RemoveWhitespace(line));
        string fireID = line.Substring(line.IndexOf("ID=") + 3).Split(',', '/')[0];
        string fireIgnite = line.Substring(line.IndexOf("VEG_LSET_IGNITE_TIME=") + 21).Split(',', '/')[0];
        fireSURF[fireID] = fireIgnite;
        
        if(fireSURF.Count == 1) {
            return "&SURF ID='INT_FIRE0', VEG_LSET_IGNITE_TIME=0, COLOR='RED' /";
        } else {
            return "";
        }
    }

    private string setOBSTLine(string line, float x, float z)
    {
        
        foreach (GameObject obj in objects.ToList())
        {
            string type = obj.tag;
            Vector3 transform = obj.transform.position;
            if (transform.x - FireManager.halfCellSize == x && transform.z - FireManager.halfCellSize == z)
            {
                objects.Remove(obj);
                if(type == "Fire"){
                    type = "INT_FIRE0";
                }
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
