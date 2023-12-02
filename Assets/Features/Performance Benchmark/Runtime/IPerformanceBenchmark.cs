namespace Perrinn424
{
    public interface IPerformanceBenchmark
    {
        float Time { get; } //[s]
        float TimeDiff { get; } //[s]
        float Speed { get; } //[m/s]
        float TraveledDistance { get; } //[m]
    } 
}
