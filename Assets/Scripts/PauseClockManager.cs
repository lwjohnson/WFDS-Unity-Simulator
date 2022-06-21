
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseClockManager : MonoBehaviour
{
    public Text wallclock_time;

    // Start is called before the first frame update
    void Start()
    {
        wallclock_time = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if(InteractionManager.restart_safety_tracker > 0) {
            wallclock_time.text = "Restart Cooldown: " + InteractionManager.restart_safety_tracker.ToString("n2") + "s";
        } else if(wallclock_time.text != "") {
            wallclock_time.text = "";
        }
    }
}