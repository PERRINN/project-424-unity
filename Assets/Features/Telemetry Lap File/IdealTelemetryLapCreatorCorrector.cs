using UnityEngine;

namespace Perrinn424.TelemetryLapSystem
{
    public class IdealTelemetryLapCreatorCorrector
    {
        private float dt;
        private float time;
        private float distanceOffset;

        private CSVLine temp;
        private CSVLine current;
        private CSVLine previous;
        private CSVLine corrected;

        public int LineSector => temp.Sector;
        public float[] CorrectedValues => corrected.Values;

        public void ReadLine(string line)
        {
            temp.UpdateValues(line);
        }
        public IdealTelemetryLapCreatorCorrector(string [] headers, float dt)
        {
            temp = new CSVLine(headers);
            current = new CSVLine(headers);
            previous = new CSVLine(headers);
            corrected = new CSVLine(headers);

            this.dt = dt;
            time = 0f;
            distanceOffset = 0f;
        }

        public void Correct()
        {
            previous.UpdateValues(current);
            current.UpdateValues(temp);
            corrected.UpdateValues(current);

            corrected.Time = time;
            time += dt;


            float distanceDifference = current.Distance - previous.Distance;
            float speed = current.Speed / 3.6f;
            float distanceDifferenceFromSpeed = speed * dt;
            float offset = distanceDifferenceFromSpeed - distanceDifference;
            float error = offset / (distanceDifferenceFromSpeed);

            if (Mathf.Abs(error) > 0.05f)
            {
                distanceOffset = distanceOffset + offset;
            }

            corrected.Distance = current.Distance + distanceOffset;
        }
    } 
}
