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
    public static int xmax = 0;
    public static int ymax = 0;
    public static int zmax = 0;
    public static int xmin = 0;
    public static int ymin = 0;
    public static int zmin = 0;
    [SerializeField]
    [Tooltip("The prefab for the ground")]
    private GameObject groundPrefab;

    void Start()
    {
        if(SimulationManager.fds){
            lines = System.IO.File.ReadAllLines(Application.streamingAssetsPath + "/fds/fds_input.fds");
            Debug.Log(lines.Length);
            setUsefulFDSValues();
        } else {
            lines = System.IO.File.ReadAllLines(Application.streamingAssetsPath + "/small_input.fds");
            setUsefulWFDSValues();
        }

        vertices = getVertices();
        triangles = getTriangles();

        generateTerrain();
        setCameraPosition();

        FireManager.instantiateInitialFires(lines);
    }

    public List<Vector3> passVertices() {
        return vertices;
    }

    public int passCellsize() {
        return cellsize;
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
        return vertices.Find( v => (v.x == (x - (x % cellsize)) && v.z == (z - (z % cellsize))) );
    }

    /// <summary>
    /// Generates the terrain and adds it to the scene.
    /// </summary>
    public void generateTerrain()
    {
        Vector3 point = new Vector3 (0,0,0);

        int count = 0;
        for(int i = 0; i <= triangles.Count-255; i=i+255) {
            GameObject ground = Instantiate(groundPrefab, point, Quaternion.identity);
            Mesh mesh = new Mesh();

            mesh.vertices = vertices.ToArray();
            List<int> local_triangles = new List<int>{};
            for(int j = i; j < i+255; j++) {
                local_triangles.Add(triangles[j]);
            }
            mesh.triangles = local_triangles.ToArray();
            
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.Optimize();

            ground.GetComponent<MeshFilter>().mesh = mesh;
            ground.GetComponent<MeshCollider>().sharedMesh = mesh;
            count = i;
        } 

        GameObject groundExtra = Instantiate(groundPrefab, point, Quaternion.identity);
        Mesh meshExtra = new Mesh();

        meshExtra.vertices = vertices.ToArray();
        List<int> local_triangles_extra = new List<int>{};
        for(int j = 0; j < triangles.Count - count; j++) {
            local_triangles_extra.Add(triangles[count + j]);
        }
        meshExtra.triangles = local_triangles_extra.ToArray();
        
        meshExtra.RecalculateNormals();
        meshExtra.RecalculateBounds();
        meshExtra.Optimize();

        groundExtra.GetComponent<MeshFilter>().mesh = meshExtra;
        groundExtra.GetComponent<MeshCollider>().sharedMesh = meshExtra;
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
    /// Returns a string with all quotes removed.
    /// </summary>
    /// <param name="input">The string to remove quotes from.</param>
    /// <returns>The string with all quotes removed.</returns>
    public static string RemoveQuotes(string input)
    {
        return new string(input.ToCharArray().Where(c => !(c == '"' || c == '\'')).ToArray());
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
    private void setUsefulWFDSValues()
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

                xmin = int.Parse(mesh[3]);
                xmax = int.Parse(mesh[4]);
                ymin = int.Parse(mesh[7]);
                ymax = int.Parse(mesh[8]);
                zmin = int.Parse(mesh[5]);
                zmax = int.Parse(mesh[6]);

                cellsize = (xmax - xmin) / numx;
                FireManager.halfCellSize = cellsize / 2;
                ncols = xmax / cellsize;
                nrows = zmax / cellsize;
            }
        }
    }

    
    /// <summary>
    /// Sets the useful values for the simulation.
    /// </summary>
    private void setUsefulFDSValues()
    {
        int slice_count = 0;

        foreach (string line in lines)
        {
            string clean = RemoveWhitespace(line);

            if (clean.Contains("&OBST")) { break; } // Past the useful information. Breaks to save time.
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
            else if (clean.Contains("&SLCF"))
            {
                slice_count++;
                if (clean.Contains("QUANTITY='TIMEOFARRIVAL'")) 
                {
                    SimulationManager.slice_number = slice_count;
                }
            }
            else if(clean.Contains("&MISC"))
            {   
                getMiscValues(clean);
            }
            else if (clean.Contains("&MESH"))
            {
                string[] mesh = clean.Replace("&MESHIJK=", string.Empty).Replace("XB=", string.Empty).Replace("/", string.Empty).Split(',');

                int numx = int.Parse(mesh[0]);
                int numy = int.Parse(mesh[1]);
                int numz = int.Parse(mesh[2]);

                xmin = int.Parse(mesh[3]);
                xmax = int.Parse(mesh[4]);
                ymin = int.Parse(mesh[7]);
                ymax = int.Parse(mesh[8]);
                zmin = int.Parse(mesh[5]);
                zmax = int.Parse(mesh[6]);

                cellsize = (xmax - xmin) / numx;
                FireManager.halfCellSize = cellsize / 2;
                ncols = xmax / cellsize;
                nrows = zmax / cellsize;
            }
        }
    }

    private void getMiscValues(string clean)
    {
        if(clean.Contains("LEVEL_SET_MODE="))
        {
            int index = clean.IndexOf("LEVEL_SET_MODE=") + "LEVEL_SET_MODE=".Length;
            int.TryParse(clean.Substring(index, 1), out SimulationManager.level_set_mode);
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
}
