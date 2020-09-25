using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LapDistanceReset : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Project424.Telemetry424.m_lapDistance = 0;
    }
}
