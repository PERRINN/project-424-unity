namespace Perrinn424
{
    public class TimeDiff919
    {
        private TimeReference porsche;
        private TimeReference volkswagen;

        public float PorscheDiff { get; private set; }
        public float VolkswagenDiff { get; private set; }

        public TimeDiff919()
        {
            porsche = TimeReferenceHelper.CreatePorsche();
            volkswagen = TimeReferenceHelper.CreateVolkswagen();
        }

        public void Update(float currentTime, float currentDistance)
        {
            PorscheDiff = porsche.LapDiff(currentTime, currentDistance);
            VolkswagenDiff = volkswagen.LapDiff(currentTime, currentDistance);
        }
    }
}
