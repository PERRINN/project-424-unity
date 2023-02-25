public interface IPIDInfo
{
	float Error { get; }
	float P { get; }
	float I { get; }
	float D { get; }
	//TODO rename
	float PID { get; }

	//TODO rename
	float MaxForceP { get; }
	//TODO rename
	float MaxForceD { get; }
}
