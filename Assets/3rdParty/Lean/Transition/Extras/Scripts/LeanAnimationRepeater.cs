using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Transition.Extras
{
	/// <summary>This component executes the specified transitions at regular intervals.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanAnimationRepeater")]
	[AddComponentMenu(LeanTransition.ComponentMenuPrefix + "Lean Animation Repeater")]
	public class LeanAnimationRepeater : LeanAnimationOnce
	{
		/// <summary>The time in seconds between each animation.</summary>
		public float TimeInterval { set { timeInterval = value; } get { return timeInterval; } } [SerializeField] [UnityEngine.Serialization.FormerlySerializedAs("TimeInterval")] private float timeInterval = 3.0f;

		protected override void Start()
		{
			if (remainingTime <= 0.0f)
			{
				BeginTransitionsAndEvent();
			}
		}

		protected override void Update()
		{
			remainingTime -= Time.deltaTime;

			if (remainingTime <= 0.0f)
			{
				BeginTransitionsAndEvent();
			}
		}

		private void BeginTransitionsAndEvent()
		{
			remainingTime = timeInterval + remainingTime % timeInterval;

			BeginTransitions();

			if (onAnimation != null)
			{
				onAnimation.Invoke();
			}
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Transition.Extras
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanAnimationRepeater))]
	public class LeanAnimationRepeater_Inspector : Lean.Common.LeanInspector<LeanAnimationRepeater>
	{
		protected override void DrawInspector()
		{
			Draw("remainingTime", "When this reaches 0, the transitions will begin.");
			Draw("timeInterval", "The time in seconds between each animation.");

			EditorGUILayout.Separator();

			Draw("transitions", "This stores the Transforms containing all the transitions that will be performed.");

			EditorGUILayout.Separator();

			Draw("onAnimation");
		}
	}
}
#endif