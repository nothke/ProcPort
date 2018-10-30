using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runway : MonoBehaviour
{

    public float taxiwayDistance = 100;
    public float taxiwayThreshold = -10;

    public bool inUse;

    public float GetTaxiwayZ()
    {
        return transform.position.z + taxiwayDistance;
    }

    public float GetThreshold()
    {
        return transform.position.z + taxiwayThreshold;
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
