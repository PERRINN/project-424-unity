namespace Perrinn424.TelemetryLapSystem
{
    public class IdealTelemetryLapCreatorCorrector
    {
        private float dt;
        private float time;
        private float distance;
        public IdealTelemetryLapCreatorCorrector(float dt)
        {
            this.dt = dt;
            time = 0f;
            distance = 0f;
        }
        public void Correct(CSVLine line)
        {
            line.Time = time;
            time += dt;

            distance = distance + line.Speed;
            line.Distance = distance;
        }
    } 
}
