using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ControllerManager : MonoBehaviour
{
    public XRNode rightHand;
    public XRNode leftHand;

    public InputDevice leftController;
    public InputDevice rightController;

    public static bool leftTriggerPressed = false;
    public static bool leftGripPressed = false;
    public static bool leftPrimaryPressed = false;
    public static bool leftSecondaryPressed = false;
    public static bool leftMenuPressed = false;

    public static bool rightTriggerPressed = false;
    public static bool rightGripPressed = false;
    public static bool rightPrimaryPressed = false;
    public static bool rightSecondaryPressed = false;
    // no right menu, that is the oculus menu

    void Start()
    {
        leftController = InputDevices.GetDeviceAtXRNode(leftHand);
        rightController = InputDevices.GetDeviceAtXRNode(rightHand);
    }

    void Update()
    {        
        //up here if need its input even when paused
        leftController.TryGetFeatureValue( CommonUsages.menuButton, out leftMenuPressed); 

        if (InteractionManager.interaction_done || SimulationManager.pause_guard) { 
            leftTriggerPressed = false;
            leftGripPressed = false;
            leftPrimaryPressed = false;
            leftSecondaryPressed = false;

            rightTriggerPressed = false;
            rightGripPressed = false;
            rightPrimaryPressed = false;
            rightSecondaryPressed = false;
            return; 
        }

        //down here if do not need its input when paused
        leftController.TryGetFeatureValue( CommonUsages.triggerButton, out leftTriggerPressed);
        leftController.TryGetFeatureValue( CommonUsages.gripButton, out leftGripPressed);
        leftController.TryGetFeatureValue( CommonUsages.primaryButton, out leftPrimaryPressed);
        leftController.TryGetFeatureValue( CommonUsages.secondaryButton, out leftSecondaryPressed);

        rightController.TryGetFeatureValue( CommonUsages.triggerButton, out rightTriggerPressed);
        rightController.TryGetFeatureValue( CommonUsages.gripButton, out rightGripPressed);
        rightController.TryGetFeatureValue( CommonUsages.primaryButton, out rightPrimaryPressed);
        rightController.TryGetFeatureValue( CommonUsages.secondaryButton, out rightSecondaryPressed);
    }


    // returns true if both grip and trigger are pressed for desired hand
    // bool right -> true for right, false for left
    public static bool gripTriggerPressed(bool right) {
        if(right) {
            return rightGripPressed && rightTriggerPressed;
        }
        return leftGripPressed && leftTriggerPressed;
    }

    // returns true if both menu button is pressed
    public static bool menuPressed() {
        return leftMenuPressed || Input.GetKey(KeyCode.R);
    }

    // returns true if grip is pressed for desired hand
    // bool right -> true for right, false for left
    public bool gripPressed(bool right) {
        if(right) {
            return rightGripPressed;
        }
        return leftGripPressed;
    }

    // returns true if primary button is pressed for desired hand
    // bool right -> true for right, false for left
    public static bool primaryPressed(bool right) {
        if(right) {
            return rightPrimaryPressed;
        }
        return leftPrimaryPressed;
    }
    
    // returns true if secondary button is pressed for desired hand
    // bool right -> true for right, false for left
    public static bool secondaryPressed(bool right) {
        if(right) {
            return rightSecondaryPressed;
        }
        return leftSecondaryPressed;
    }

}
