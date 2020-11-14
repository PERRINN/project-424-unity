using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project424 {
    public class LapDistanceReset : MonoBehaviour
    {
        float getLapDist;
        void OnTriggerEnter(Collider Perrin424)
        {
            Telemetry424.m_lapDistance = 0;
            getLapDist = Telemetry424.m_lapDistance;
            print(getLapDist);
        }        
    }
}
