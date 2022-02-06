using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireManager : MonoBehaviour
{
    public static Dictionary<int, List<Vector3>> fire_TOA; // Dictionary of [Slice => [List of vector3 locations]]
    public static int current_slice = 0; // The current slice we are rendering
    public static int slice_size;
    public static float wallclock_time = 0;
    public static List<GameObject> fire_objects; // List of GameObject fires which have been instantiated
    [Tooltip("Attach the fire prefab here.")]
    public GameObject fire_prefab; // Fire prefab model

    // Start is called before the first frame update
    void Start()
    {
        fire_TOA = new Dictionary<int, List<Vector3>>();
        fire_objects = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (fire_TOA.ContainsKey(current_slice)) // There is data about the slice we are currently on
        {
            wallclock_time += Time.deltaTime; // Increase time
            instantiateFires();
        }
        if (wallclock_time >= (current_slice + slice_size)) // We need to render the next slice
        {
            current_slice += slice_size; // Move the current slice to the next slice
        }
    }

    // instantiate fire prefab GameObjects from fire_TOA[current_slice]
    void instantiateFires()
    {
        List<Vector3> slice = fire_TOA[current_slice]; // The slice that corresponds with the current wallclock time
        if (slice.Count > 0) // There is at least one fire to render
        {
            Debug.Log("Instantiating new fires in slice: " + current_slice);
            foreach (Vector3 point in slice)
            {
                if (checkNotSameLocation(point))
                {
                    GameObject new_fire = Instantiate(fire_prefab, point, Quaternion.identity); // Add the fire to Unity
                    new_fire.transform.localScale = new Vector3(TerrainManager.cellsize, TerrainManager.cellsize, TerrainManager.cellsize); // Transform the fire to the game scale
                    fire_objects.Add(new_fire); // Add the fire to our list of game objects
                }
                else
                {
                    Debug.Log("Cannot instantiate fire at this location. Already has existing fire.");
                }
            }
            slice.Clear(); // Remove all fires from the current slice
        }
    }

    // This function returns true if there is not a fire already in the location of new_fire
    bool checkNotSameLocation(Vector3 new_fire)
    {
        foreach (GameObject existing_fire in fire_objects)
        {
            if (existing_fire.transform.position == new_fire) // Same location
            {
                return false;
            }
        }
        return true;
    }

    // Reads the lstoa.sf data in
    // updates the fire dictionary with new data
    public static void updateFireDictionary()
    {
        Dictionary<int, List<Vector2>> new_TOA = TOAManager.usefulTOA(); // Read the lstoa data

        foreach (KeyValuePair<int, List<Vector2>> slice in new_TOA)
        {
            Debug.Log("Adding fires to: " + slice.Key);

            List<Vector3> new_fire_list = new List<Vector3>();
            foreach (Vector2 new_TOA_fire in new_TOA[slice.Key])
            {
                Debug.Log("Adding fire at: " + new_TOA_fire);
                Vector3 new_fire = TerrainManager.find_vert(new_TOA_fire * 10);
                new_fire_list.Add(new_fire);
            }
            fire_TOA[slice.Key] = new_fire_list;
        }
    }
}
