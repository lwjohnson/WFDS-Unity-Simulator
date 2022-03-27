using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireManager : MonoBehaviour
{
    public static Dictionary<int, List<Vector3>> fire_TOA = new Dictionary<int, List<Vector3>>();
    public static List<GameObject> fires = new List<GameObject>();

    public static GameObject firePrefab;

    public static float wallclock_time = 0;


    // Start is called before the first frame update
    void Start()
    {
        firePrefab = Resources.Load("WildFire") as GameObject;

    }

    // Update is called once per frame
    void Update()
    {
        if (fire_TOA.Count > 0)
        {
            wallclock_time += Time.deltaTime;

            // If fire_TOA contains a key less than wallclock_time, then Instantiate new fires
            foreach (int key in fire_TOA.Keys)
            {
                if (key <= wallclock_time)
                {
                    foreach (Vector3 point in fire_TOA[key])
                    {
                        GameObject new_fire = Instantiate(firePrefab, point, Quaternion.identity);
                        new_fire.transform.localScale = new Vector3(TerrainManager.cellsize, TerrainManager.cellsize, TerrainManager.cellsize);
                        fires.Add(new_fire);
                    }
                }
            }
        }
    }

    public static void readFireData()
    {

    }
}
