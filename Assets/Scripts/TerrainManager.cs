using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    private string[] lines = null; // The lines of the .fds file.
    private static List<Vector3> vertices = null; // The vertices of the terrain.
    private List<int> triangles = null; // The triangles of the terrain.
    public static int cellsize = 0; // The size of each cell.
    private int ncols = 0; // The number of columns in the terrain.
    private int nrows = 0; // The number of rows in the terrain.
    private string chid = string.Empty; // The CHID of the terrain.
    private int tStart = 0; // Time Start
    private int tEnd = 0; // Time End
    private float tStep = 0; // Time Step
    public int xmax = 0;
    public int ymax = 0;
    public int zmax = 0;

    public static List<GameObject> initial_fires;

    [SerializeField]
    [Tooltip("The prefab for the fire")]
    private GameObject firePrefab;

    void Start()
    {
        lines = System.IO.File.ReadAllLines(Application.streamingAssetsPath + "/" + "input.fds");

        setUsefulValues();

        vertices = getVertices();
        triangles = getTriangles();

        generateTerrain();
        setCameraPosition();

        initial_fires = getInitialFires();
    }

    /// <summary>
    /// Finds the vector3 with coordinates x and z
    /// </summary>
    /// <param name="x">The x coordinate</param>
    /// <param name="z">The z coordinate</param>
    /// <returns>The vector3 with coordinates x and z</returns>
    public static Vector3 getVector3(long x, long z)
    {
        return vertices.Find(v => (v.x == x && v.z == z));
    }

    /// <summary>
    /// Finds the nearest Vector3 to x, z
    /// </summary>
    /// <param name="x">The x coordinate</param>
    /// <param name="z">The z coordinate</param>
    /// <returns>The nearest Vector3 to x, z</returns>
    public static Vector3 getNearestVector3(float x, float z)
    {
        Vector3 nearest = new Vector3(0, 0, 0);
        float minDistance = float.MaxValue;

        foreach (Vector3 v in vertices)
        {
            float distance = Vector3.Distance(new Vector3(x, 0, z), v);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = v;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Generates the terrain and adds it to the scene.
    /// </summary>
    public void generateTerrain()
    {
        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    /// <summary>
    /// Returns a string with all whitespace removed.
    /// </summary>
    /// <param name="input">The string to remove whitespace from.</param>
    /// <returns>The string with all whitespace removed.</returns>
    public static string RemoveWhitespace(string input)
    {
        return new string(input.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    /// <summary>
    /// Sets the camera to the center of the terrain.
    /// </summary>
    private void setCameraPosition()
    {
        GameObject XR_Origin = GameObject.Find("XR Origin");

        Vector3 position = getVector3(xmax / 2, zmax / 2);
        XR_Origin.transform.position = position;
    }

    /// <summary>
    /// Sets the useful values for the simulation.
    /// </summary>
    private void setUsefulValues()
    {
        foreach (string line in lines)
        {
            string clean = RemoveWhitespace(line);

            if (clean.Contains("&OBST")) break; // Past the useful information. Breaks to save time.
            if (clean.Contains("&HEAD"))
            {
                chid = clean.Substring(clean.IndexOf('\'') + 1, clean.LastIndexOf('\'') - (1 + clean.IndexOf('\'')));
            }
            else if (clean.Contains("&TIMET_END"))
            {
                tEnd = int.Parse(clean.Substring(clean.IndexOf('=') + 1, (clean.LastIndexOf('/')) - (1 + clean.IndexOf('='))));
            }
            else if (clean.Contains("&TIMET_BEGIN"))
            {
                tStart = int.Parse(clean.Substring(clean.IndexOf('=') + 1, (clean.LastIndexOf('/')) - (1 + clean.IndexOf('='))));
            }
            else if (clean.Contains("DT_OUTPUT_LS"))
            {
                tStep = float.Parse(clean.Substring(clean.IndexOf('=') + 1, (clean.LastIndexOf('/')) - (1 + clean.IndexOf('='))));
            }
            else if (clean.Contains("&MESH"))
            {
                string[] mesh = clean.Replace("&MESHIJK=", string.Empty).Replace("XB=", string.Empty).Replace("/", string.Empty).Split(',');

                int numx = int.Parse(mesh[0]);
                int numy = int.Parse(mesh[1]);
                int numz = int.Parse(mesh[2]);
                int xmin = int.Parse(mesh[3]);
                xmax = int.Parse(mesh[4]);
                int ymin = int.Parse(mesh[7]);
                ymax = int.Parse(mesh[8]);
                int zmin = int.Parse(mesh[5]);
                zmax = int.Parse(mesh[6]);

                cellsize = (xmax - xmin) / numx;
                ncols = xmax / cellsize;
                nrows = zmax / cellsize;
            }
        }
    }

    /// <summary>
    /// Gets the vertices of the terrain.
    /// </summary>
    /// <returns>The vertices of the terrain.</returns>
    private List<Vector3> getVertices()
    {
        List<Vector3> vertices = new List<Vector3>();

        lines.Where(l => l.Contains("&OBST")).ToList().ForEach(l =>
        {
            string[] split = RemoveWhitespace(l).Replace("&OBSTXB=", string.Empty).Replace("/", string.Empty).Split(',');

            vertices.Add(new Vector3(int.Parse(split[1]), int.Parse(split[5]), int.Parse(split[3]))); // x, z, y because WFDS is weird.
        });

        return vertices;
    }

    /// <summary>
    /// Gets the triangles of the terrain.
    /// </summary>
    /// <returns>The triangles of the terrain.</returns>
    private List<int> getTriangles()
    {
        List<int> triangles = new List<int>();

        for (int i = 0; i < (nrows - 1) * ncols; i++)
        {
            if (i != (ncols - 1) + (ncols * (i / ncols)))
            {
                triangles.Add(i);
                triangles.Add(i + 1);
                triangles.Add(i + ncols);

                triangles.Add(i + 1);
                triangles.Add(i + ncols + 1);
                triangles.Add(i + ncols);
            }
        }

        return triangles;
    }

    private List<GameObject> getInitialFires()
    {
        List<GameObject> fires = new List<GameObject>();

        lines.Where(l => l.Contains("&OBST") && l.Contains("FIRE")).ToList().ForEach(l =>
        {
            string[] split = RemoveWhitespace(l).Replace("&OBSTXB=", string.Empty).Replace("SURF_ID='FIRE'/", string.Empty).Split(',');

            Vector3 point = getNearestVector3(float.Parse(split[1]), float.Parse(split[3]));

            GameObject fire = Instantiate(firePrefab, point, Quaternion.identity);
            fire.transform.localScale = new Vector3(cellsize, cellsize, cellsize);
            fires.Add(fire);
        });

        return fires;
    }
}
