using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Transition.Extras
{
	/// <summary>This component executes the specified transitions at regular intervals.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanAnimationOnce")]
	[AddComponentMenu(LeanTransition.ComponentMenuPrefix + "Lean Animation Once")]
	public class LeanAnimationOnce : LeanAnimation
	{
		/// <summary>When this reaches 0, the transitions will begin.</summary>
		public float RemainingTime { set { remainingTime = value; } get { return remainingTime; } } [SerializeField] [UnityEngine.Serialization.FormerlySerializedAs("RemainingTime")] protected float remainingTime = 1.0f;

		/// <summary>The event will execute when the transitions begin.</summary>
		public UnityEvent OnAnimation { get { if (onAnimation == null) onAnimation = new UnityEvent(); return onAnimation; } } [SerializeField] [UnityEngine.Serialization.FormerlySerializedAs("OnAnimation")] protected UnityEvent onAnimation;

		protected virtual void Start()
		{
			if (remainingTime <= 0.0f)
			{
				BeginTransitionsAndEvent();
			}
		}

		protected virtual void Update()
		{
			if (remainingTime > 0.0f)
			{
				remainingTime -= Time.deltaTime;

				if (remainingTime <= 0.0f)
				{
					BeginTransitionsAndEvent();
				}
			}
		}

		private void BeginTransitionsAndEvent()
		{
			remainingTime = 0.0f;

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
	[CustomEditor(typeof(LeanAnimationOnce))]
	public class LeanAnimationOnce_Inspector : Lean.Common.LeanInspector<LeanAnimationOnce>
	{
		protected override void DrawInspector()
		{
			Draw("remainingTime", "When this reaches 0, the transitions will begin.");

			EditorGUILayout.Separator();

			Draw("transitions", "This stores the Transforms containing all the transitions that will be performed.");

			EditorGUILayout.Separator();

			Draw("onAnimation");
		}
	}
}
#endif