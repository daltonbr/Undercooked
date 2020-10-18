using UnityEngine;
using Lean.Transition;

public class RepeatFromCode : MonoBehaviour
{
	void Start()
	{
		InvokeRepeating("UpAndDown", 0.0f, 2.0f); // Call every 2 seconds
	}

	void UpAndDown()
	{
		transform.
			localPositionTransition(Vector3.up, 1). // Move up over 1 second
			JoinTransition(). // Connect together
			localPositionTransition(Vector3.down, 1); // Move down over 1 second
	}
}