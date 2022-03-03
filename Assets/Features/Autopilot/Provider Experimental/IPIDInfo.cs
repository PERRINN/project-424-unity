public interface IPIDInfo
{
	float Error { get; }
	float P { get; }
	float I { get; }
	float D { get; }
	float PID { get; }

	float MaxForceP { get; }
	float MaxForceD { get; }
}
