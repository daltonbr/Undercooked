using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified Light.range to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanLightRange")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "Light/Light.range" + LeanTransition.MethodsMenuSuffix + "(LeanLightRange)")]
	public class LeanLightRange : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(Light);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.Range, Data.Duration, Data.Ease);
		}

		public static LeanState Register(Light target, float range, float duration, LeanEase ease = LeanEase.Smooth)
		{
			var data = LeanTransition.RegisterWithTarget(State.Pool, duration, target);

			data.Range = range;
			data.Ease  = ease;

			return data;
		}

		[System.Serializable]
		public class State : LeanStateWithTarget<Light>
		{
			[Tooltip("The range we will transition to.")]
			public float Range = 10.0f;

			[Tooltip("The ease method that will be used for the transition.")]
			public LeanEase Ease = LeanEase.Smooth;

			[System.NonSerialized] private float oldRange;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.range != Range ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				Range = Target.range;
			}

			public override void BeginWithTarget()
			{
				oldRange = Target.range;
			}

			public override void UpdateWithTarget(float progress)
			{
				Target.range = Mathf.LerpUnclamped(oldRange, Range, Smooth(Ease, progress));
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
		public static Light rangeTransition(this Light target, float range, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanLightRange.Register(target, range, duration, ease); return target;
		}
	}
}