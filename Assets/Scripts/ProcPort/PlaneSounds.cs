using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nothke.ProcPort
{
    public class PlaneSounds : MonoBehaviour
    {
        public Plane plane;
        public AudioSource jet;
        public AnimationCurve volumeCurve;
        public AudioSource turbulence;
        public AnimationCurve turbulenceVolumeCurve;

        float dampVelo;
        float smoothThrottle = 1;

        void Update()
        {
            float target = plane.throttle;

            float smoothing = 4;
            if (target == 0) smoothing = 10;
            if (plane.isPushback) smoothing = 10;

            smoothThrottle = Mathf.SmoothDamp(smoothThrottle, target, ref dampVelo, smoothing);
            jet.pitch = smoothThrottle * 1.5f;
            jet.volume = volumeCurve.Evaluate(smoothThrottle);
            turbulence.volume = turbulenceVolumeCurve.Evaluate(smoothThrottle);
        }
    }
}