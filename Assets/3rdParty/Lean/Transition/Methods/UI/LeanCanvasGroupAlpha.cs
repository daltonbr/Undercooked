using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified CanvasGroup.alpha to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanCanvasGroupAlpha")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "CanvasGroup/CanvasGroup.alpha" + LeanTransition.MethodsMenuSuffix + "(LeanCanvasGroupAlpha)")]
	public class LeanCanvasGroupAlpha : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(CanvasGroup);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.Alpha, Data.Duration, Data.Ease);
		}

		public static LeanState Register(CanvasGroup target, float alpha, float duration, LeanEase ease = LeanEase.Smooth)
		{
			var data = LeanTransition.RegisterWithTarget(State.Pool, duration, target);

			data.Alpha = alpha;
			data.Ease  = ease;

			return data;
		}

		[System.Serializable]
		public class State : LeanStateWithTarget<CanvasGroup>
		{
			[Tooltip("The alpha we will transition to.")]
			[Range(0.0f, 1.0f)]
			public float Alpha = 1.0f;

			[Tooltip("The ease method that will be used for the transition.")]
			public LeanEase Ease  = LeanEase.Smooth;

			[System.NonSerialized] private float oldAlpha;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.alpha != Alpha ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				Alpha = Target.alpha;
			}

			public override void BeginWithTarget()
			{
				oldAlpha = Target.alpha;
			}

			public override void UpdateWithTarget(float progress)
			{
				Target.alpha = Mathf.LerpUnclamped(oldAlpha, Alpha, Smooth(Ease, progress));
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
		public static CanvasGroup alphaTransition(this CanvasGroup target, float alpha, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanCanvasGroupAlpha.Register(target, alpha, duration, ease); return target;
		}
	}
}