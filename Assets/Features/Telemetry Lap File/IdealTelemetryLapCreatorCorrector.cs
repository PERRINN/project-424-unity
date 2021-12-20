using UnityEngine;

namespace Perrinn424.TelemetryLapSystem
{
    public class IdealTelemetryLapCreatorCorrector
    {
        private float dt;
        private float time;
        //private float distance;
        private float distanceOffset;
        public IdealTelemetryLapCreatorCorrector(float dt)
        {
            this.dt = dt;
            time = 0f;
            //distance = 0f;
            distanceOffset = 0f;
        }
        public void Correct(CSVLine current, CSVLine previous)
        {
            current.Time = time;
            time += dt;
            return;

            if (current.Distance < 0f)
                return;

            float error = ((current.Distance - previous.Distance) - (current.Speed*dt))/ (current.Speed * dt);

            if (Mathf.Abs(error) > 0.05f)
            {
                distanceOffset = distanceOffset + (current.Speed * dt - (current.Distance - previous.Distance));
            }

            //distance = distance + current.Speed;
            current.Distance = current.Distance + distanceOffset;
        }

        public void Correct(CSVLine corrected, CSVLine current, CSVLine previous)
        {
            corrected.UpdateValues(current);

            corrected.Time = time;
            time += dt;

            if (!previous.HasValues)
                return;

            float distanceDifference = current.Distance - previous.Distance;
            float distanceDifferenceFromSpeed = current.Speed * dt;
            float offset = distanceDifferenceFromSpeed - distanceDifference;
            float error = offset / (distanceDifferenceFromSpeed);

            if (Mathf.Abs(error) > 0.05f)
            {
                distanceOffset = distanceOffset + offset;
            }


            //distance = distance + current.Speed;
            corrected.Distance = current.Distance + distanceOffset;
        }
    } 
}
