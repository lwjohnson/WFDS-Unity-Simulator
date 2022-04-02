
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WallclockManager : MonoBehaviour
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
        wallclock_time.text = "Time: " + FireManager.wallclock_time.ToString("n2") + "s";
    }
}