using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified RectTransform.anchoredPosition.x to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanRectTransformAnchoredPositionX")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "RectTransform/RectTransform.anchoredPosition.x" + LeanTransition.MethodsMenuSuffix + "(LeanRectTransformAnchoredPositionX)")]
	public class LeanRectTransformAnchoredPositionX : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(RectTransform);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.Position, Data.Duration, Data.Ease);
		}

		public static LeanState Register(RectTransform target, float position, float duration, LeanEase ease = LeanEase.Smooth)
		{
			var data = LeanTransition.RegisterWithTarget(State.Pool, duration, target);

			data.Position = position;
			data.Ease     = ease;

			return data;
		}

		[System.Serializable]
		public class State : LeanStateWithTarget<RectTransform>
		{
			[Tooltip("The position we will transition to.")]
			public float Position;

			[Tooltip("The ease method that will be used for the transition.")]
			public LeanEase Ease = LeanEase.Smooth;

			[System.NonSerialized] private float oldPosition;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.anchoredPosition.x != Position ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				Position = Target.anchoredPosition.x;
			}

			public override void BeginWithTarget()
			{
				oldPosition = Target.anchoredPosition.x;
			}

			public override void UpdateWithTarget(float progress)
			{
				var anchoredPosition = Target.anchoredPosition;

				anchoredPosition.x = Mathf.LerpUnclamped(oldPosition, Position, Smooth(Ease, progress));

				Target.anchoredPosition = anchoredPosition;
			}

			public static Stack<State> Pool = new Stack<State>(); public override void Despawn() { Pool.Push(this); }
		}

		public State Data;
	}
}

namespace Lean.Transition
{
	public static partial class LeanExtensions
	{
		public static RectTransform anchoredPositionTransition_X(this RectTransform target, float position, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanRectTransformAnchoredPositionX.Register(target, position, duration, ease); return target;
		}
	}
}