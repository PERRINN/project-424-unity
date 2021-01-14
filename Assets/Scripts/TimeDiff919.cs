namespace Perrinn424
{
    public class TimeDiff919
    {
        private TimeReference volkswagen;
        private TimeReference porsche;

        public TimeDiff919()
        {
            porsche = TimeReferenceHelper.CreatePorsche();
            volkswagen = TimeReferenceHelper.CreateVolkswagen();
        }

        public void Update(float currentTime, float currentDistance)
        {
            porsche.LapDiff(currentTime, currentDistance);
            volkswagen.LapDiff(currentTime, currentDistance);
        }
    }
}
