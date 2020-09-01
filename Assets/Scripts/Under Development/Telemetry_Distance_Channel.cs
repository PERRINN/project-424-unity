using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehiclePhysics;

public class Telemetry_Distance_Channel : MonoBehaviour
{
    /*
     https://i.imgur.com/6iPmROn.png
     https://i.imgur.com/nifyuKv.png
     https://i.imgur.com/fxdvd73.png
         */

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        
    }

    public class TotalDistanceChart : PerformanceChart
    {
        public override string Title ()
        {
            return "Total Distance Travelled";
        }

        public override void Initialize()
        {
            dataLogger.topLimit = 12.0f;
            dataLogger.bottomLimit = -0.5f;
        }

        public override void ResetView()
        {
            dataLogger.rect = new Rect(0.0f, -0.5f, 30.0f, 12.5f);
        }

        public override void RecordData()
        {
            throw new System.NotImplementedException();
        }

        public override void SetupChannels()
        {
            throw new System.NotImplementedException();
        }
    }

}
