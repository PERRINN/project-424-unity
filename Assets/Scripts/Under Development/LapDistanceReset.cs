using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project424 {
    public class LapDistanceReset : MonoBehaviour
    {
        void OnTriggerEnter(Collider Perrin424)
        {
            Telemetry424.m_lapDistance = 0;
        }
    }
}
