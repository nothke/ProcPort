using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nothke.ProcPort;

public class Test_FlightScheduler : MonoBehaviour
{
    FlightScheduler _scheduler;
    FlightScheduler scheduler { get { if (!_scheduler) _scheduler = GetComponent<FlightScheduler>(); return _scheduler; } }

    void Start()
    {
        scheduler.BenchmarkOvercrowded();
        scheduler.SortFlightsByArrivalTime();
    }
}
