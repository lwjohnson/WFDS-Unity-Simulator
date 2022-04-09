using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeManager : MonoBehaviour
{

    [Tooltip("The prefab for the tree")]
    public GameObject treePrefabEditor;
    public static GameObject treePrefab;

    void Start()
    {
        treePrefab = treePrefabEditor;
    }

    void Update()
    {

    }

    public static void createTreeAt(Vector3 point)
    {
        GameObject new_tree = Instantiate(treePrefab, point, Quaternion.LookRotation(Vector3.up));
        new_tree.transform.localScale = new Vector3(TerrainManager.cellsize, TerrainManager.cellsize, TerrainManager.cellsize);
    }

    public static void removeTreeAt(Vector3 point)
    {
        foreach (GameObject tree in GameObject.FindGameObjectsWithTag("Tree"))
        {
            if (tree.transform.position.x == point.x && tree.transform.position.z == point.z)
            {
                Destroy(tree);
            }
        }
    }

    public static bool treeExistsAt(Vector3 point)
    {
        foreach (GameObject tree in GameObject.FindGameObjectsWithTag("Tree"))
        {
            if (tree.transform.position.x == point.x && tree.transform.position.z == point.z)
            {
                return true;
            }
        }
        return false;
    }

    public static GameObject getTreeAt(Vector3 point)
    {
        foreach (GameObject tree in GameObject.FindGameObjectsWithTag("Tree"))
        {
            if (tree.transform.position.x == point.x && tree.transform.position.z == point.z)
            {
                return tree;
            }
        }
        return null;
    }
}
