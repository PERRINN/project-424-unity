using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehiclePhysics;
using EdyCommonTools;

namespace Project424
{
#if !VPP_ESSENTIAL

    [RequireComponent(typeof(VPPerformanceDisplay))]
    public class Telemetry_Distance_Channel : MonoBehaviour
    {
        // Member variables
        private Vector3 m_lastPosition;
        public static float m_totalDistance { get; private set; }
        public static float m_lapDistance { get; private set; }

        public Rigidbody Perrinn424;
        public BoxCollider StartLine;

        public enum Charts { TotalDistanceChart };

        // Shows dropdown list in Unity Inspector
        public Charts chart = Charts.TotalDistanceChart;

        // The array of charts
        PerformanceChart[] m_charts = new PerformanceChart[]
        {
        new TotalDistanceChart()
        };

        VPPerformanceDisplay m_perfComponent;

        void OnEnable()
        {   // When enabled, get VPPDisplay and assign member variables
            m_perfComponent = GetComponent<VPPerformanceDisplay>();

            // Initialises the variables
            m_lastPosition = Perrinn424.position;
            m_totalDistance = 0;
            m_lapDistance = 0;
        }

        void FixedUpdate()
        {
            // Calculates the total distance travelled by subtracting lastPosition from current position
            float distance = (Perrinn424.position - m_lastPosition).magnitude;

            // Updates lastPosition to current position and increment totalDistance
            m_lastPosition = Perrinn424.position;
            m_totalDistance += distance;
            m_lapDistance += distance;
            
            

            // Passes the exposed parameters to their corresponding charts
            TotalDistanceChart totalDistanceChart = m_charts[(int)Charts.TotalDistanceChart] as TotalDistanceChart;

            // Applies the selected custom chart
            m_perfComponent.customChart = m_charts[(int)chart];            
        }
    }

    // Total Distance Chart
    public class TotalDistanceChart : PerformanceChart
    {
        // Creates channel for distance travelled
        DataLogger.Channel m_totalDistanceTravelled;

        public override string Title()
        {
            return "Total Distance Travelled";
        }

        public override void Initialize()
        {
            dataLogger.topLimit = 1000f;
            dataLogger.bottomLimit = 0f;
        }

        public override void ResetView()
        {
            dataLogger.rect = new Rect(0.0f, -0.5f, 30.0f, 12.5f);
        }

        public override void SetupChannels()
        {
            m_totalDistanceTravelled = dataLogger.NewChannel("Total Distance");
            m_totalDistanceTravelled.color = GColor.gray;
            m_totalDistanceTravelled.SetOriginAndSpan(3.5f, 2.0f, 1.0f);
            m_totalDistanceTravelled.valueFormat = "0 m";
            m_totalDistanceTravelled.captionPositionY = 1;
        }

        public override void RecordData()
        {
            //passes the distance to the datalogger to write on the chart
            m_totalDistanceTravelled.Write(Telemetry_Distance_Channel.m_totalDistance);
            m_totalDistanceTravelled.SetOriginAndSpan(3.5f, 2.0f, Telemetry_Distance_Channel.m_totalDistance / 3);
        }
    }

#endif
}