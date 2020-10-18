using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified RectTransform.offsetMin to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanRectTransformOffsetMin")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "RectTransform/RectTransform.offsetMin" + LeanTransition.MethodsMenuSuffix + "(LeanRectTransformOffsetMin)")]
	public class LeanRectTransformOffsetMin : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(RectTransform);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.OffsetMin, Data.Duration, Data.Ease);
		}

		public static LeanState Register(RectTransform target, Vector2 offsetMin, float duration, LeanEase ease = LeanEase.Smooth)
		{
			var data = LeanTransition.RegisterWithTarget(State.Pool, duration, target);

			data.OffsetMin = offsetMin;
			data.Ease      = ease;

			return data;
		}

		[System.Serializable]
		public class State : LeanStateWithTarget<RectTransform>
		{
			[Tooltip("The offsetMin we will transition to.")]
			public Vector2 OffsetMin;

			[Tooltip("The ease method that will be used for the transition.")]
			public LeanEase Ease = LeanEase.Smooth;

			[System.NonSerialized] private Vector2 oldOffsetMin;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.offsetMin != OffsetMin ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				OffsetMin = Target.offsetMin;
			}

			public override void BeginWithTarget()
			{
				oldOffsetMin = Target.offsetMin;
			}

			public override void UpdateWithTarget(float progress)
			{
				Target.offsetMin = Vector2.LerpUnclamped(oldOffsetMin, OffsetMin, Smooth(Ease, progress));
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
		public static RectTransform offsetMinTransition(this RectTransform target, Vector2 offsetMin, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanRectTransformOffsetMin.Register(target, offsetMin, duration, ease); return target;
		}
	}
}