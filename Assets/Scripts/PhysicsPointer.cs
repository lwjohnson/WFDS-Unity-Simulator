using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsPointer : MonoBehaviour
{

    public float defaultLength = 5.0f;
    public bool rightHand; //true if right hand, false if left hand
    public GameObject placeMarker;
    public GameObject gameManager;

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
        if(skip > 0) {
            skip--;
        } else {
            skip = 2;
            UpdateLength();
        }
    }

    private void UpdateLength()
    {
        lineRenderer.SetPosition(0, transform.position);
        CalculateEnd();
        lineRenderer.SetPosition(1, endPosition);

        Vector3 playerLocation = GameObject.Find("XR Origin").transform.position;
        Vector3 place = new Vector3(0, 0, 0);
        if(endPosition != DefaultEnd(defaultLength) && Mathf.Abs(endPosition.x - playerLocation.x) > 1 && Mathf.Abs(endPosition.z - playerLocation.z) > 1) {
            place = getNearestVector3(endPosition.x, endPosition.z);
        }
        placeMarker.transform.position = place;
        placeMarker.transform.rotation = Quaternion.identity;
    }

    private Vector3 CalculateEnd()
    {
        RaycastHit hit = CreateForwardRaycast();
        endPosition = DefaultEnd(defaultLength);

        if(hit.collider && hit.collider.gameObject.tag == "Ground")
        {
            endPosition = hit.point;
        }

        return endPosition;
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
        int cellsize = terrainManager.passCellsize();
        return vertices.Find( v => (v.x == (x - (x % cellsize)) && v.z == (z - (z % cellsize))) );
    }

}
