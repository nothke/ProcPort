using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimescaleHelper : MonoBehaviour {
    
	void Update () {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            Time.timeScale = 1;

        if (Input.GetKeyDown(KeyCode.Alpha2))
            Time.timeScale = 3;

        if (Input.GetKeyDown(KeyCode.Alpha3))
            Time.timeScale = 6;

        if (Input.GetKeyDown(KeyCode.Alpha4))
            Time.timeScale = 10;

        if (Input.GetKeyDown(KeyCode.Alpha5))
            Time.timeScale = 20;
    }
}
