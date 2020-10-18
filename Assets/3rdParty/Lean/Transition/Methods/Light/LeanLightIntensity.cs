using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified Light.intensity to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanLightIntensity")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "Light/Light.intensity" + LeanTransition.MethodsMenuSuffix + "(LeanLightIntensity)")]
	public class LeanLightIntensity : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(Light);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.Intensity, Data.Duration, Data.Ease);
		}

		public static LeanState Register(Light target, float intensity, float duration, LeanEase ease = LeanEase.Smooth)
		{
			var data = LeanTransition.RegisterWithTarget(State.Pool, duration, target);

			data.Intensity = intensity;
			data.Ease      = ease;

			return data;
		}

		[System.Serializable]
		public class State : LeanStateWithTarget<Light>
		{
			[Tooltip("The intensity we will transition to.")]
			public float Intensity = 1.0f;

			[Tooltip("The ease method that will be used for the transition.")]
			public LeanEase Ease = LeanEase.Smooth;

			[System.NonSerialized] private float oldIntensity;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.intensity != Intensity ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				Intensity = Target.intensity;
			}

			public override void BeginWithTarget()
			{
				oldIntensity = Target.intensity;
			}

			public override void UpdateWithTarget(float progress)
			{
				Target.intensity = Mathf.LerpUnclamped(oldIntensity, Intensity, Smooth(Ease, progress));
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
		public static Light intensityTransition(this Light target, float intensity, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanLightIntensity.Register(target, intensity, duration, ease); return target;
		}
	}
}