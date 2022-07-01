using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text.RegularExpressions;

public class FireManager : MonoBehaviour
{
    public static SortedDictionary<int, List<Vector3>> fire_TOA = new SortedDictionary<int, List<Vector3>>();

    [Tooltip("The prefab for the fires")]
    public GameObject firePrefabEditor;
    public GameObject firePrefabStatic;
    public static GameObject firePrefab;

    public bool staticFire;


    private static int current_key = 0; 
    public static float wallclock_time = 0;
    public static float starting_time = 0;
    public static float time_multiplier = 1;
    public static bool read_fires_once = false;
    public static GameObject staticFirePrefab;
    public static float halfCellSize;

    public static SortedDictionary<float, List<int>> fires = new SortedDictionary<float, List<int>>();

    void Start()
    {
        wallclock_time = starting_time;
        if(staticFire) {
          firePrefab = firePrefabStatic;
        } else {
          firePrefab = firePrefabEditor;
        }
        staticFirePrefab = firePrefabStatic;
    }

    void Update()
    {
        if(!InteractionManager.interaction_done) {
            return;
        }

        if (SimulationManager.wfds_run_once && read_fires_once && wallclock_time <= starting_time + SimulationManager.time_to_run * WFDSManager.wfds_runs)
        {
            wallclock_time += Time.deltaTime * time_multiplier;
        }

        if (fire_TOA.Count > 0)
        {
            // If fire_TOA contains a key less than wallclock_time, then Instantiate new fires
            current_key = fire_TOA.Keys.ToList()[0];

            if (current_key <= wallclock_time)
            {
                foreach (Vector3 point in fire_TOA[current_key])
                {
                    createFireAt(point);
                }
                fire_TOA.Remove(current_key);
            }
        }
    }

    // Creates a fire at the given point
    // Vector3 point: The bottom left corner of the square where the fire will be placed
    public static void createFireAt(Vector3 point, bool stat = false)
    {
        Vector3 newPoint = new Vector3(point.x + halfCellSize, point.y, point.z + halfCellSize);
        GameObject new_fire;

        if(stat) {
          new_fire = Instantiate(staticFirePrefab, newPoint, Quaternion.identity);
        } else {
          new_fire = Instantiate(firePrefab, newPoint, Quaternion.identity);
        }
        
        new_fire.transform.localScale = Vector3.one * TerrainManager.cellsize;

        if(!InteractionManager.interaction_done) {
            new_fire.GetComponent<FireLifeTime>().ignite_time = wallclock_time;

            int id = new_fire.GetInstanceID();

            if (fires.ContainsKey(wallclock_time)) {
                fires[wallclock_time].Add(id);
            } else {
                fires.Add(wallclock_time, new List<int>() { id });
            }
        }
    }

    // Removes a fire if exists at point
    // Vector3 point: The bottom left corner of the square where the fire will be removed
    public static void removeFireAt(Vector3 point)
    {
        foreach (GameObject fire in GameObject.FindGameObjectsWithTag("Fire"))
        {
            if (fire.transform.position.x - halfCellSize == point.x && fire.transform.position.z - halfCellSize == point.z)
            {
                Destroy(fire);
            }
        }
    }

    // Determines if a fire exists at point
    // Vector3 point: The bottom left corner of the square where to check for fire
    public static bool fireExistsAt(Vector3 point)
    {
        foreach (GameObject fire in GameObject.FindGameObjectsWithTag("Fire"))
        {
            if (fire.transform.position.x - halfCellSize == point.x && fire.transform.position.z - halfCellSize == point.z)
            {
                return true;
            }
        }
        return false;
    }

    // Return fire gamboject at point
    // Vector3 point: The bottom left corner of the square where to get the fire
    public static GameObject getFireAt(Vector3 point)
    {
        foreach (GameObject fire in GameObject.FindGameObjectsWithTag("Fire"))
        {
            if (fire.transform.position.x - halfCellSize == point.x && fire.transform.position.z - halfCellSize == point.z)
            {
                return fire;
            }
        }
        return null;
    }

    // Creates new fires from the initial input file
    public static void instantiateInitialFires(string[] lines)
    {
        List<GameObject> fires = new List<GameObject>();

        lines.Where(l => l.Contains("&OBST") && l.Contains("FIRE")).ToList().ForEach(l =>
        {
            string[] split = TerrainManager.RemoveWhitespace(l).Replace("&OBSTXB=", string.Empty).Replace("SURF_ID='FIRE'/", string.Empty).Split(',');

            Vector3 point = TerrainManager.getNearestVector3(float.Parse(split[1]), float.Parse(split[3]));

            createFireAt(point);
        });
    }

    // Starts thread to read fires from the lstoa file
    public static void readFireData()
    {   
        SimulationManager.reading_fire = true;
        //Copy output file so can begin another simulation
        FileUtil.DeleteFileOrDirectory(WFDSManager.persistentDataPath + @"\input_lstoa_copy.sf");
        FileUtil.CopyFileOrDirectory(WFDSManager.persistentDataPath + @"\input_lstoa.sf", WFDSManager.persistentDataPath + @"\input_lstoa_copy.sf");
        setupInputFile();

        Thread read_fire_thread = new Thread(readFires);
        read_fire_thread.Start();
    }

    //reads fire data from the lstoa file
    private static void readFires(){
        
        Debug.Log("Reading fires");

        //want to read from the output of fires
        FileInfo toa_file = new FileInfo(WFDSManager.persistentDataPath + @"\input_lstoa_copy.sf");
        using BinaryReader reader = new BinaryReader(toa_file.OpenRead());

        List<long> bounds = new List<long>();
        SortedDictionary<int, List<Vector3>> fire_TOA_copy = new SortedDictionary<int, List<Vector3>>();

        // Quantity
        readFortranRecord(reader); // Read the opening Fortran record size
        reader.ReadChars(30);
        readFortranRecord(reader); // Read the closing Fortran record size

        // Short Name
        readFortranRecord(reader); // Fortran
        reader.ReadChars(30);
        readFortranRecord(reader); // Fortran

        // Units
        readFortranRecord(reader); // Fortran
        reader.ReadChars(30);
        readFortranRecord(reader); // Fortran

        // Bounds of the SLCF file
        readFortranRecord(reader); // Fortran

        for (int i = 0; i < 6; i++)
        {
            bounds.Add(reader.ReadInt32()); // Read the bounds and add them to the bounds array
        }
        readFortranRecord(reader); // Fortran

        // Time and Data
        // Read to the end of the file
        while (reader.BaseStream.Position != reader.BaseStream.Length)
        {
            // Time
            readFortranRecord(reader); // Fortran

            float time = reader.ReadSingle(); // Read the time

            readFortranRecord(reader); // Fortran

            // Data
            readFortranRecord(reader); // Fortran

            for (long y = bounds[4]; y <= bounds[5]; y++)
            {
                for (long z = bounds[2]; z <= bounds[3]; z++)
                {
                    for (long x = bounds[0]; x <= bounds[1]; x++)
                    {
                        int arrival_time = (int)reader.ReadSingle(); // Read the arrival_time and convert to int instead of float
                        if (arrival_time >= current_key || (WFDSManager.wfds_runs == 0 && arrival_time >= 0)) // Fire reaches this point
                        {
                            // Multiplied by 10 because in the SLCF file, the x would be 175 but it should be 1750 because of cellsize
                            Vector3 point = TerrainManager.getNearestVector3(x * TerrainManager.cellsize, z * TerrainManager.cellsize);

                            if (fire_TOA_copy.ContainsKey(arrival_time)) {
                                if(fire_TOA_copy[arrival_time].Contains(point)) {
                                 continue;
                                }
                                fire_TOA_copy[arrival_time].Add(point);
                            } else {
                                fire_TOA_copy.Add(arrival_time, new List<Vector3>() { point });
                            }
                        }
                    }
                }
            }

            readFortranRecord(reader); // Fortran
        }
        fire_TOA = fire_TOA_copy;
        reader.Close();
        SimulationManager.reading_fire = false;
        
        read_fires_once = true;

        Debug.Log("Finished reading fires");
    }

    // I made this function so it was a bit clearer what was being read.
    // Fortran is weird
    private static void readFortranRecord(BinaryReader reader)
    {
        reader.ReadInt32();
    }

    //gets input file ready with new end time and new fire surfaces
    public static void setupInputFile()
    {
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

                line = $"&TIME T_END= {starting_time + SimulationManager.time_to_run * (WFDSManager.wfds_runs + 1)} /";
            } else if (line.Contains("&OBST")) {
                line = setupHelper(line, ref fires);
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

    private static string setupHelper(string line, ref List<GameObject> fires) {
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
            if (transform.x - halfCellSize == x && transform.z - halfCellSize == z)
            {
                float time = obj.GetComponent<FireLifeTime>().ignite_time;
                objects.Remove(obj);
                return line = Regex.Replace(line, "'.*'", $"'INT_{type}{time}'");
            }
        }

        return line;
    }

    private static bool afterZero(GameObject g) {
        return g.GetComponent<FireLifeTime>().ignite_time <= 0;
    }
}
