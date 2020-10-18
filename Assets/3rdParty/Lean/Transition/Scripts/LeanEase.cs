namespace Lean.Transition
{
	/// <summary>This enum allows you to pick the ease type used by most transition methods.</summary>
	public enum LeanEase
	{
		Linear,
		Smooth     = 100,
		Accelerate = 200,
		Decelerate = 250,
		Elastic    = 300,
		Back       = 400,
		Bounce     = 500
	}
}