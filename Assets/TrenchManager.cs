using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrenchManager : MonoBehaviour
{
    [Tooltip("The prefab for the trenches")]
    public GameObject trenchPrefabEditor;
    public static GameObject trenchPrefab;

    void Start()
    {
        trenchPrefab = trenchPrefabEditor;
    }

    void Update()
    {

    }

    public static void createTrenchAt(Vector3 point)
    {
        GameObject new_trench = Instantiate(trenchPrefab, point, Quaternion.identity);
        new_trench.transform.localScale = Vector3.one * TerrainManager.cellsize;
    }

    public static void removeTrenchAt(Vector3 point)
    {
        foreach (GameObject trench in GameObject.FindGameObjectsWithTag("Trench"))
        {
            if (trench.transform.position.x == point.x && trench.transform.position.z == point.z)
            {
                Destroy(trench);
            }
        }
    }

    public static bool trenchExistsAt(Vector3 point)
    {
        foreach (GameObject trench in GameObject.FindGameObjectsWithTag("Trench"))
        {
            if (trench.transform.position.x == point.x && trench.transform.position.z == point.z)
            {
                return true;
            }
        }
        return false;
    }

    public static GameObject getTrenchAt(Vector3 point)
    {
        foreach (GameObject trench in GameObject.FindGameObjectsWithTag("Trench"))
        {
            if (trench.transform.position.x == point.x && trench.transform.position.z == point.z)
            {
                return trench;
            }
        }
        return null;
    }
}
