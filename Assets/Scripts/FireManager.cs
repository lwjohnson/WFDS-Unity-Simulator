using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class FireManager : MonoBehaviour
{
    public static Dictionary<int, List<Vector3>> fire_TOA = new Dictionary<int, List<Vector3>>();
    public static List<GameObject> fires = new List<GameObject>();
    public static List<GameObject> stopped_fires = new List<GameObject>();

    [SerializeField]
    [Tooltip("The prefab for the fire")]
    private GameObject firePrefab;

    public static float wallclock_time = 0;
    public static int fires_to_keep = 200;

    // Update is called once per frame
    void Update()
    {
        if (SimulationManager.wfds_run_once && wallclock_time <= SimulationManager.time_to_run)
        {
            wallclock_time += Time.deltaTime;
        }

        if (fire_TOA.Count > 0)
        {
            // If fire_TOA contains a key less than wallclock_time, then Instantiate new fires
            foreach (int key in fire_TOA.Keys.ToList())
            {
                if (key <= wallclock_time)
                {
                    foreach (Vector3 point in fire_TOA[key])
                    {
                        GameObject new_fire = Instantiate(firePrefab, point, Quaternion.identity);
                        new_fire.transform.localScale = new Vector3(TerrainManager.cellsize, TerrainManager.cellsize, TerrainManager.cellsize);
                        fires.Add(new_fire);

                    }
                    // Remove the key from fire_TOA
                    fire_TOA.Remove(key);
                }
            }
        }

        // If fires.Count > fires_to_keep, then stop the animation of the oldest fire
        if (fires.Count > fires_to_keep)
        {
            GameObject oldest_fire = fires[0];
            ParticleSystem ps = oldest_fire.GetComponent<ParticleSystem>(); // Get the oldest fire's ParticleSystem
            var main = ps.main; // Get the main module

            main.simulationSpeed = 0; // Pause the particle system

            stopped_fires.Add(oldest_fire);
            fires.Remove(oldest_fire);
        }
    }

    public static void readFireData()
    {
        FileInfo toa_file = new FileInfo(Application.persistentDataPath + @"\input_lstoa.sf");
        using BinaryReader reader = new BinaryReader(toa_file.OpenRead());

        List<long> bounds = new List<long>();

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

                        if (arrival_time > 0) // Fire reaches this point
                        {
                            // Multiplied by 10 because in the SLCF file, the x would be 175 but it should be 1750 because of cellsize
                            Vector3 point = TerrainManager.getNearestVector3(x * TerrainManager.cellsize, z * TerrainManager.cellsize);

                            if (fire_TOA.ContainsKey(arrival_time))
                            {
                                fire_TOA[arrival_time].Add(point);
                            }
                            else
                            {
                                fire_TOA.Add(arrival_time, new List<Vector3>() { point });
                            }
                        }
                    }
                }
            }

            readFortranRecord(reader); // Fortran
        }

        reader.Close();
    }

    // I made this function so it was a bit clearer what was being read.
    // Fortran is weird
    private static void readFortranRecord(BinaryReader reader)
    {
        reader.ReadInt32();
    }
}
