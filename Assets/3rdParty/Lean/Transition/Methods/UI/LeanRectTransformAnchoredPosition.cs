using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified RectTransform.anchoredPosition to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanRectTransformAnchoredPosition")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "RectTransform/RectTransform.anchoredPosition" + LeanTransition.MethodsMenuSuffix + "(LeanRectTransformAnchoredPosition)")]
	public class LeanRectTransformAnchoredPosition : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(RectTransform);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.Position, Data.Duration, Data.Ease);
		}

		public static LeanState Register(RectTransform target, Vector2 position, float duration, LeanEase ease = LeanEase.Smooth)
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
			public Vector2 Position;

			[Tooltip("The ease method that will be used for the transition.")]
			public LeanEase Ease = LeanEase.Smooth;

			[System.NonSerialized] private Vector2 oldPosition;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.anchoredPosition != Position ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				Position = Target.anchoredPosition;
			}

			public override void BeginWithTarget()
			{
				oldPosition = Target.anchoredPosition;
			}

			public override void UpdateWithTarget(float progress)
			{
				Target.anchoredPosition = Vector2.LerpUnclamped(oldPosition, Position, Smooth(Ease, progress));
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
		public static RectTransform anchoredPositionTransition(this RectTransform target, Vector2 position, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanRectTransformAnchoredPosition.Register(target, position, duration, ease); return target;
		}
	}
}