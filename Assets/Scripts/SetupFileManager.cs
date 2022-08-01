
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class SetupFileManager : MonoBehaviour
{

    public static List<GameObject> fires;
    public static List<GameObject> trees;
    public static List<GameObject> trenches;
    public static List<GameObject> objects;
    public static SortedDictionary<string, string> fireSURF = new SortedDictionary<string, string>();

    public static void setupInitialInputFile()
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
                line = $"&TIME T_END= {FireManager.starting_time + SimulationManager.time_to_run * (WFDSManager.wfds_runs + 1)} /";
            }
            else if (line.Contains("&TIME T_START"))
            {
                line = "&TIME T_START= 0 /";
            }
            else if (line.Contains("&DUMP"))
            {
                line = $"&DUMP DT_OUTPUT_LS={SimulationManager.time_to_run}.0 /";
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

                line = setInitialOBSTLine(line, x, y);
            }

            writer.WriteLine(line);
        }
    }

    private static string setupFireSurface(string line) 
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

    private static string setInitialOBSTLine(string line, float x, float z)
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

    private static bool afterZero(GameObject g) {
        return g.GetComponent<FireLifeTime>().ignite_time <= 0;
    }

    //gets input file ready with new end time and new fire surfaces
    public static void readFireDataFileSetup(){
        FileUtil.DeleteFileOrDirectory(WFDSManager.persistentDataPath + @"\input_copy.fds");
        FileUtil.CopyFileOrDirectory(WFDSManager.persistentDataPath + @"\input.fds", WFDSManager.persistentDataPath + @"\input_copy.fds");
        FileInfo map = new DirectoryInfo(WFDSManager.persistentDataPath).GetFiles("input_copy.fds").FirstOrDefault();

        using StreamWriter writer = new StreamWriter(WFDSManager.persistentDataPath + @"\input.fds");
        using StreamReader reader = new StreamReader(map.OpenRead());
        
        bool added_surfaces = false;

        List<GameObject> fires = GameObject.FindGameObjectsWithTag("Fire").ToList();
        fires.RemoveAll(afterZero);

        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();

            if (line.Contains("&TIME T_END")) {

                line = $"&TIME T_END= {FireManager.starting_time + SimulationManager.time_to_run * (WFDSManager.wfds_runs + 1)} /";
            } else if (line.Contains("&OBST")) {
                line = readFireSetupHelper(line, ref fires);
            } else if (!added_surfaces && line.Contains("&SURF")) {
                //inserting the new fire surfaces
                foreach(float fire in FireManager.fires.Keys) {
                    writer.WriteLine($"&SURF ID ='INT_FIRE{fire}',VEG_LSET_IGNITE_TIME={fire},COLOR = 'RED' /");
                }
                added_surfaces = true;
            }
            
            if(!line.Contains("INT_FIRE") || !line.Contains("&SURF")) {
                writer.WriteLine(line);
            }
        }
    }

    private static string readFireSetupHelper(string line, ref List<GameObject> fires) {
        string[] split = TerrainManager.RemoveWhitespace(line).Replace("&OBSTXB=", string.Empty).Replace("/", string.Empty).Split(',');

        int x = int.Parse(split[1]);
        int y = int.Parse(split[3]);

        line = setOBSTLine(line, "FIRE", ref fires, x, y);

        return line;
    }

    private static string setOBSTLine(string line, string type, ref List<GameObject> objects, int x, int z)
    {
        foreach (GameObject obj in objects.ToList())
        {
            Vector3 transform = obj.transform.position;
            if (transform.x - FireManager.halfCellSize == x && transform.z - FireManager.halfCellSize == z)
            {
                float time = obj.GetComponent<FireLifeTime>().ignite_time;
                objects.Remove(obj);
                return line = Regex.Replace(line, "'.*'", $"'INT_{type}{time}'");
            }
        }

        return line;
    }




}