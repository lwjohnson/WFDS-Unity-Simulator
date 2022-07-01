using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsPointer : MonoBehaviour
{

    public float defaultLength = 5.0f;
    public bool rightHand; //true if right hand, false if left hand
    public GameObject betterPlaceMarker;
    public GameObject gameManager;
    public GameObject player;

    public static Vector3 endPosition = new Vector3(0, 0, 0);
    private static TerrainManager terrainManager;
    private static ControllerManager controllerManager;
    private LineRenderer lineRenderer = null;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        terrainManager = gameManager.GetComponent<TerrainManager>();
        controllerManager = gameManager.GetComponent<ControllerManager>();
    }

    private void Update()
    {
        if(controllerManager.gripPressed(rightHand == true)){
            UpdateLength();
        } else {
            ClearLength();
        }
    }

    // Update the line renderer to reflect the current length, and update placemarker position
    private void UpdateLength()
    {
        lineRenderer.SetPosition(0, transform.position);
        endPosition = CalculateEnd();
        lineRenderer.SetPosition(1, endPosition);

        Vector3 playerLocation = player.transform.position;
        float cellsize = terrainManager.passCellsize();
    
        if(endPosition != DefaultEnd(defaultLength) && getNearestVector3(endPosition.x, endPosition.z) != getNearestVector3(playerLocation.x, playerLocation.z)) {
            
            List<Vector3> verticeList = new List<Vector3>();
            
            Vector3 bl = getNearestVector3(endPosition.x, endPosition.z);
            verticeList.Add(bl);
            verticeList.Add(new Vector3(bl.x, 0, bl.z + cellsize));
            verticeList.Add(new Vector3(bl.x + cellsize, 0, bl.z));
            verticeList.Add(new Vector3(bl.x + cellsize, 0, bl.z + cellsize));

            getNearestVector3List(ref verticeList);
            
            betterPlaceMarker.GetComponent<MeshFilter>().mesh.vertices = verticeList.ToArray();
            betterPlaceMarker.GetComponent<MeshFilter>().mesh.triangles = new int[] { 0, 1, 3, 2, 0, 3 };
            betterPlaceMarker.GetComponent<MeshFilter>().mesh.RecalculateBounds();
            betterPlaceMarker.SetActive(true);
        } else {
            betterPlaceMarker.SetActive(false);
        }
    
    }

    // Clear the line renderer and placemarker
    private void ClearLength() {
        betterPlaceMarker.SetActive(false);
        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.zero);
    }

    // Calculate the end position of the line renderer
    private Vector3 CalculateEnd()
    {
        RaycastHit hit = CreateForwardRaycast();
        Vector3 end = DefaultEnd(defaultLength);

        if(hit.collider && hit.collider.gameObject.tag == "Ground")
        {
            end = hit.point;
        }

        return end;
    }

    // Create a raycast forward from the controller
    private RaycastHit CreateForwardRaycast()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);

        Physics.Raycast(ray, out hit, defaultLength);
        return hit;
    }

    // Get the default end of the line renderer
    private Vector3 DefaultEnd(float length)
    {
        return transform.position + (transform.forward * length);
    }

    // returns end position of the line renderer
    public Vector3 getEndPosition()
    {
        return endPosition;
    }

    // returns the nearest vector3 to the given x and z
    // float x -> x coordinate of the point
    // float z -> z coordinate of the point
    // bool adjust -> true if the point should be adjusted to middle of the cell, false if not
    private static Vector3 getNearestVector3(float x, float z, bool adjust = false)
    {
        List<Vector3> vertices = terrainManager.passVertices();
        float cellsize = terrainManager.passCellsize();
        Vector3 place = vertices.Find( v => (v.x == (x - (x % cellsize)) && v.z == (z - (z % cellsize))) );

        if(adjust) {               
            place.x += cellsize / 2;
            place.y -= cellsize / 4;
            place.z += cellsize / 2;
        }

        return place;
    }

    private static void getNearestVector3List(ref List<Vector3> vertices) {
        List<Vector3> verticeList = new List<Vector3>();
        foreach(Vector3 v in vertices) {
            verticeList.Add(getNearestVector3(v.x, v.z));
        }
        vertices = verticeList;
    }

}
