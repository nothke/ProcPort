using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Flight
{
    public string number;
    public string destination;

    public enum Progress { Arriving, Landing, GoToGate, Boarding, Departing, Cancelled }
    public Progress progress;

    public bool sheduledArrival;

    public float arrivalTime;
    public float departureTime;

    public bool AtGate()
    {
        if (!plane) return false;
        if (plane.state == Plane.State.AtGate) return true;
        return false;
    }

    public void UpdateStatus()
    {
        if (plane)
        {
            if (plane.state == Plane.State.AtGate &&
                progress == Progress.Landing)
            {
                Debug.Log(number + " arrived at gate. Time to departure: " + (departureTime - Time.time));
                progress = Progress.GoToGate;
            }
        }
        else
        {
            if (progress == Progress.Landing)
            {
                progress = Progress.Cancelled;
                Debug.LogWarning(number + " flight canceled");
            }
        }
    }

    public string PlaneRegistration()
    {
        if (!plane) return "N/A";
        return plane.name;
    }

    public Plane plane;
}

public class FlightScheduler : MonoBehaviour
{
    List<Flight> flights = new List<Flight>();

    public ATC atc;

    private void Start()
    {
        //BenchmarkOvercrowded();
        BenchmarkScheduleMany(20);

        SortFlightsByArrivalTime();
    }

    void BenchmarkOvercrowded()
    {
        for (int i = 0; i < 20; i++)
        {
            ScheduleRandomArrivingFlight(0);
        }
    }

    void BenchmarkScheduleMany(int count)
    {
        for (int i = 0; i < count; i++)
        {
            ScheduleRandomArrivingFlight(Random.Range(0, 2000));
        }
    }

    void ScheduleRandomArrivingFlight(float inTime)
    {
        Flight flight = CreateRandomFlight();
        flight.arrivalTime = Time.time + inTime + Random.Range(5, 20);
        flight.departureTime = Time.time + inTime + Random.Range(500, 700);
    }

    Flight CreateRandomFlight()
    {
        Flight flight = new Flight();
        flight.number = GetRandomFlightNumber();
        flight.destination = GetRandomDestination();

        flights.Add(flight);

        return flight;
    }

    private void Update()
    {
        if (flights.Count > 0)
            for (int i = 0; i < flights.Count; i++)
            {
                flights[i].UpdateStatus();

                if (Time.time > flights[i].arrivalTime && flights[i].progress == Flight.Progress.Arriving)
                {
                    if (!atc.FreeGatesExist())
                    {
                        Debug.LogWarning("No free gates, flights canceled");
                        flights[i].progress = Flight.Progress.Cancelled;
                        return;
                    }

                    Plane plane = atc.CreatePlane();
                    flights[i].plane = plane;
                    plane.gameObject.SetActive(false);

                    atc.SubmitPlaneForLanding(plane);

                    flights[i].progress = Flight.Progress.Landing;
                }

                if (Time.time > flights[i].departureTime && flights[i].AtGate())
                {
                    flights[i].progress = Flight.Progress.Departing;
                    flights[i].plane.state = Plane.State.TaxiingToRunway;
                }
            }
    }

    const string glyphs = "abcdefghijklmnopqrstuvwxyz"; //add the characters you want

    string GetRandomFlightNumber()
    {
        string str = "";

        str += char.ToUpper(glyphs[Random.Range(0, glyphs.Length)]);
        str += char.ToUpper(glyphs[Random.Range(0, glyphs.Length)]);

        int numNum = Random.Range(3, 4);
        for (int i = 0; i < numNum; i++)
            str += Random.Range(0, 9);

        return str;
    }

    string GetRandomDestination()
    {
        // TODO: Add your own destinations
        return "Nowhere City";
    }

    void SortFlightsByArrivalTime()
    {
        flights = flights.OrderBy(f => f.arrivalTime).ToList();
    }

    private void OnGUI()
    {
        if (flights.Count > 0)
            for (int i = 0; i < flights.Count; i++)
            {
                GUILayout.Label(
                    string.Format("{0}, {1}, {2}, {3}, land: {4}, dprt: {5}",
                    flights[i].number,
                    flights[i].destination,
                    flights[i].progress,
                    flights[i].PlaneRegistration(),
                    flights[i].arrivalTime,
                    flights[i].departureTime));
            }
    }
}
