using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class FireManager : MonoBehaviour
{
    public static Dictionary<int, List<Vector3>> fire_TOA = new Dictionary<int, List<Vector3>>();
    public static List<GameObject> fires = new List<GameObject>();

    [SerializeField]
    [Tooltip("The prefab for the fire")]
    private GameObject firePrefab;

    public static float wallclock_time = 0;
    public static int fires_to_keep = 100;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (fire_TOA.Count > 0)
        {
            wallclock_time += Time.deltaTime;

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

        // If fires.Count > fires_to_keep, then Destroy the oldest fire
        if (fires.Count > fires_to_keep)
        {
            Destroy(fires[0]);
            fires.RemoveAt(0);
        }
    }

    public static void readFireData()
    {
        FileInfo toa_file = new FileInfo(Application.persistentDataPath + @"\input_lstoa.sf");
        using BinaryReader reader = new BinaryReader(toa_file.OpenRead());

        List<long> bounds = new List<long>();

        // Quantity
        reader.ReadInt32(); // Read the opening Fortran record size
        Debug.Log($"QUANTITY: {new string(reader.ReadChars(30))}");
        reader.ReadInt32(); // Read the closing Fortran record size

        // Short Name
        reader.ReadInt32(); // Fortran
        Debug.Log($"SHORT:    {new string(reader.ReadChars(30))}");
        reader.ReadInt32(); // Fortran

        // Units
        reader.ReadInt32(); // Fortran
        Debug.Log($"UNITS:    {new string(reader.ReadChars(30))}");
        reader.ReadInt32(); // Fortran

        // Bounds of the SLCF file
        reader.ReadInt32(); // Fortran

        for (int i = 0; i < 6; i++)
        {
            long bound = reader.ReadInt32();
            bounds.Add(bound); // Read the bounds and add them to the bounds array
            Debug.Log($"BOUND {i}: {bound}");
        }
        reader.ReadInt32(); // Fortran

        // Time and Data
        // Read to the end of the file
        while (reader.BaseStream.Position != reader.BaseStream.Length)
        {
            // Time
            reader.ReadInt32(); // Fortran

            float time = reader.ReadSingle(); // Read the time
            Debug.Log($"TIME:     {time}");

            reader.ReadInt32(); // Fortran

            Debug.Log("i\tj\tk\tQQ");
            // Data
            reader.ReadInt32(); // Fortran

            for (long y = bounds[4]; y <= bounds[5]; y++)
            {
                for (long z = bounds[2]; z <= bounds[3]; z++)
                {
                    for (long x = bounds[0]; x <= bounds[1]; x++)
                    {
                        int arrival_time = (int)reader.ReadSingle(); // Read the arrival_time and convert to int instead of float

                        if (arrival_time > 0) // Fire reaches this point
                        {
                            // Find the vector from the TerrainManager with x and z
                            // Multiplied by 10 because in the SLCF file, the x would be 175 but it should be 1750 because of cellsize
                            Vector3 point = TerrainManager.getVector3(x * 10, z * 10);

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

            reader.ReadInt32(); // Fortran
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
