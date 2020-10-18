using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified RectTransform.pivot.y to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanRectTransformPivotY")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "RectTransform/RectTransform.pivot.y" + LeanTransition.MethodsMenuSuffix + "(LeanRectTransformPivotY)")]
	public class LeanRectTransformPivotY : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(RectTransform);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.Pivot, Data.Duration, Data.Ease);
		}

		public static LeanState Register(RectTransform target, float pivot, float duration, LeanEase ease = LeanEase.Smooth)
		{
			var data = LeanTransition.RegisterWithTarget(State.Pool, duration, target);

			data.Pivot = pivot;
			data.Ease  = ease;

			return data;
		}

		[System.Serializable]
		public class State : LeanStateWithTarget<RectTransform>
		{
			[Tooltip("The pivot we will transition to.")]
			public float Pivot;

			[Tooltip("The ease method that will be used for the transition.")]
			public LeanEase Ease = LeanEase.Smooth;

			[System.NonSerialized] private float oldPivot;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.pivot.y != Pivot ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				Pivot = Target.pivot.y;
			}

			public override void BeginWithTarget()
			{
				oldPivot = Target.pivot.y;
			}

			public override void UpdateWithTarget(float progress)
			{
				var pivot = Target.pivot;

				pivot.y = Mathf.LerpUnclamped(oldPivot, Pivot, Smooth(Ease, progress));

				Target.pivot = pivot;
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
		public static RectTransform pivotTransition_Y(this RectTransform target, float pivot, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanRectTransformPivotY.Register(target, pivot, duration, ease); return target;
		}
	}
}