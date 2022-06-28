using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsPointer : MonoBehaviour
{

    public float defaultLength = 5.0f;
    public bool rightHand; //true if right hand, false if left hand
    public GameObject placeMarker;
    public GameObject gameManager;
    public GameObject player;

    public static Vector3 endPosition = new Vector3(0, 0, 0);
    private static TerrainManager terrainManager;
    private LineRenderer lineRenderer = null;
    private int skip = 2;

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
        float halfcellsize = terrainManager.passCellsize() / 2;

        if(endPosition != DefaultEnd(defaultLength)
            && 
            (Mathf.Abs(endPosition.x - playerLocation.x) > halfcellsize || Mathf.Abs(endPosition.z - playerLocation.z) > halfcellsize)) {
            placeMarker.transform.position = getNearestVector3(endPosition.x, endPosition.z);
            placeMarker.transform.rotation = Quaternion.identity;
            placeMarker.SetActive(true);
        } else {
            placeMarker.SetActive(false);
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

    private static Vector3 getNearestVector3(float x, float z)
    {
        List<Vector3> vertices = terrainManager.passVertices();
        float cellsize = terrainManager.passCellsize();
        Vector3 place = vertices.Find( v => (v.x == (x - (x % cellsize)) && v.z == (z - (z % cellsize))) );
        place.x += cellsize / 2;
        place.y -= cellsize / 4;
        place.z += cellsize / 2;
        return place;
    }

}
