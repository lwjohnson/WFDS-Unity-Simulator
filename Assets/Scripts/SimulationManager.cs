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
    public bool fds = true; //true for fds, false for wfds

    public static int time_to_run = 0;
    public static bool wfds_run_once = false;
    public static bool wfds_setup = false;
    public static bool reading_fire = false;
    public static bool ready_to_read = false;
    public static bool FDS;
    private static string fireSURF;

    // Start is called before the first frame update
    void Start()
    {
        time_to_run = time_to_run_inspector;
        FDS = fds;
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
        FileInfo map;
        if(FDS) {
            map = new DirectoryInfo(Application.streamingAssetsPath + "/fds").GetFiles("*.fds").FirstOrDefault();
        } else {
            map = new DirectoryInfo(Application.streamingAssetsPath).GetFiles("*.fds").FirstOrDefault();
        }

        using StreamWriter writer = new StreamWriter(WFDSManager.persistentDataPath + @"\input.fds");
        using StreamReader reader = new StreamReader(map.OpenRead());

        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();

            if (line.Contains("&HEAD"))
            {
                line = setupHeaderAndMisc(line);
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
                line = setupDump(line);
            }
            else if (line.Contains("&SURF") && line.Contains("FIRE")) {

                fireSURF = TerrainManager.RemoveWhitespace(line).Replace("&SURFID='", string.Empty).Split('\'')[0];
                Debug.Log(fireSURF);
                line = $"&SURF " + TerrainManager.RemoveWhitespace(line).Replace("&SURF", string.Empty);
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

                line = setOBSTLine(line, fireSURF, fires, x, y);
                line = setOBSTLine(line, "TREE", trees, x, y);
                line = setOBSTLine(line, "TRENCH", trenches, x, y);
            }
            else if (line.Contains("MISC"))
            {
                line = "";
            }

            writer.WriteLine(line);
        }
        Debug.Log("Input file setup");
    }

    private string setupHeaderAndMisc(string line)
    {
        line = "&HEAD CHID='input' /@";
        if(FDS) {
            line += "&MISC LEVEL_SET_MODE=" + TerrainManager.level_set_mode + " /";
        }
        line = line.Replace("@", System.Environment.NewLine);
        return line;
    }

    private string setupDump(string line)
    {
        if(FDS) {
            line = $"&DUMP DT_SLCF=1, DT_RESTART=20.0 /";
        } else {
            line = $"&DUMP DT_OUTPUT_LS={time_to_run}.0 /";
        }
        return line;
    }

    private string setOBSTLine(string line, string type, List<GameObject> objects, float x, float z)
    {
        foreach (GameObject obj in objects.ToList())
        {
            Vector3 transform = obj.transform.position;
            if (transform.x - FireManager.halfCellSize == x && transform.z - FireManager.halfCellSize == z)
            {
                objects.Remove(obj);
                return line = Regex.Replace(line, "'.*'", $"'{type}'");
            }
        }

        return line;
    }

    void OnApplicationQuit()
    {
        WFDSManager.stopFDS();
    }

    public static bool getFDS() {
        return FDS;
    }
}
