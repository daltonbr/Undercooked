using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified RectTransform.sizeDelta to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanRectTransformSizeDelta")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "RectTransform/RectTransform.sizeDelta" + LeanTransition.MethodsMenuSuffix + "(LeanRectTransformSizeDelta)")]
	public class LeanRectTransformSizeDelta : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(RectTransform);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.SizeDelta, Data.Duration, Data.Ease);
		}

		public static LeanState Register(RectTransform target, Vector2 sizeDelta, float duration, LeanEase ease = LeanEase.Smooth)
		{
			var data = LeanTransition.RegisterWithTarget(State.Pool, duration, target);

			data.SizeDelta = sizeDelta;
			data.Ease      = ease;

			return data;
		}

		[System.Serializable]
		public class State : LeanStateWithTarget<RectTransform>
		{
			[Tooltip("The sizeDelta we will transition to.")]
			public Vector2 SizeDelta;

			[Tooltip("The ease method that will be used for the transition.")]
			public LeanEase Ease = LeanEase.Smooth;

			[System.NonSerialized] private Vector2 oldSizeDelta;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.sizeDelta != SizeDelta ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				SizeDelta = Target.sizeDelta;
			}

			public override void BeginWithTarget()
			{
				oldSizeDelta = Target.sizeDelta;
			}

			public override void UpdateWithTarget(float progress)
			{
				Target.sizeDelta = Vector2.LerpUnclamped(oldSizeDelta, SizeDelta, Smooth(Ease, progress));
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
		public static RectTransform sizeDeltaTransition(this RectTransform target, Vector2 sizeDelta, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanRectTransformSizeDelta.Register(target, sizeDelta, duration, ease); return target;
		}
	}
}