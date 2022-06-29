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
    private LineRenderer lineRenderer = null;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        terrainManager = gameManager.GetComponent<TerrainManager>();
    }

    private void Update()
    {
        UpdateLength();
    }

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
            betterPlaceMarker.GetComponent<MeshFilter>().mesh.triangles = new int[] { 0, 1, 2, 2, 1, 3 };
            betterPlaceMarker.GetComponent<MeshFilter>().mesh.RecalculateBounds();
            betterPlaceMarker.SetActive(true);
        } else {
            betterPlaceMarker.SetActive(false);
        }
    
    }

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

    private RaycastHit CreateForwardRaycast()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);

        Physics.Raycast(ray, out hit, defaultLength);
        return hit;
    }

    private Vector3 DefaultEnd(float length)
    {
        return transform.position + (transform.forward * length);
    }

    public Vector3 getEndPosition()
    {
        return endPosition;
    }

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
