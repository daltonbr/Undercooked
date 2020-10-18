using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified RectTransform.offsetMax to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanRectTransformOffsetMax")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "RectTransform/RectTransform.offsetMax" + LeanTransition.MethodsMenuSuffix + "(LeanRectTransformOffsetMax)")]
	public class LeanRectTransformOffsetMax : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(RectTransform);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.OffsetMax, Data.Duration, Data.Ease);
		}

		public static LeanState Register(RectTransform target, Vector2 offsetMax, float duration, LeanEase ease = LeanEase.Smooth)
		{
			var data = LeanTransition.RegisterWithTarget(State.Pool, duration, target);

			data.OffsetMax = offsetMax;
			data.Ease      = ease;

			return data;
		}

		[System.Serializable]
		public class State : LeanStateWithTarget<RectTransform>
		{
			[Tooltip("The offsetMax we will transition to.")]
			public Vector2 OffsetMax;

			[Tooltip("The ease method that will be used for the transition.")]
			public LeanEase Ease = LeanEase.Smooth;

			[System.NonSerialized] private Vector2 oldOffsetMax;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.offsetMax != OffsetMax ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				OffsetMax = Target.offsetMax;
			}

			public override void BeginWithTarget()
			{
				oldOffsetMax = Target.offsetMax;
			}

			public override void UpdateWithTarget(float progress)
			{
				Target.offsetMax = Vector2.LerpUnclamped(oldOffsetMax, OffsetMax, Smooth(Ease, progress));
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
		public static RectTransform offsetMaxTransition(this RectTransform target, Vector2 offsetMax, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanRectTransformOffsetMax.Register(target, offsetMax, duration, ease); return target;
		}
	}
}