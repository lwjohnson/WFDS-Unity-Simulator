using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ContinuousMovement : MonoBehaviour
{

    public XRNode inputSource;
    private Vector2 inputAxis;
    private CharacterController character;
    private float speed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        character = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
       InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);
       device.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis);
    }
}
