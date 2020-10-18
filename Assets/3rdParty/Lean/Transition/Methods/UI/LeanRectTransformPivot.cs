using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified RectTransform.pivot to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanRectTransformPivot")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "RectTransform/RectTransform.pivot" + LeanTransition.MethodsMenuSuffix + "(LeanRectTransformPivot)")]
	public class LeanRectTransformPivot : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(RectTransform);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.Pivot, Data.Duration, Data.Ease);
		}

		public static LeanState Register(RectTransform target, Vector2 pivot, float duration, LeanEase ease = LeanEase.Smooth)
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
			public Vector2 Pivot;

			[Tooltip("The ease method that will be used for the transition.")]
			public LeanEase Ease = LeanEase.Smooth;

			[System.NonSerialized] private Vector2 oldPivot;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.pivot != Pivot ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				Pivot = Target.pivot;
			}

			public override void BeginWithTarget()
			{
				oldPivot = Target.pivot;
			}

			public override void UpdateWithTarget(float progress)
			{
				Target.pivot = Vector2.LerpUnclamped(oldPivot, Pivot, Smooth(Ease, progress));
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
		public static RectTransform pivotTransition(this RectTransform target, Vector2 pivot, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanRectTransformPivot.Register(target, pivot, duration, ease); return target;
		}
	}
}