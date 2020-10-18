using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified RectTransform.anchorMax to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanRectTransformAnchorMax")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "RectTransform/RectTransform.anchorMax" + LeanTransition.MethodsMenuSuffix + "(LeanRectTransformAnchorMax)")]
	public class LeanRectTransformAnchorMax : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(RectTransform);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.AnchorMax, Data.Duration, Data.Ease);
		}

		public static LeanState Register(RectTransform target, Vector2 anchorMax, float duration, LeanEase ease = LeanEase.Smooth)
		{
			var data = LeanTransition.RegisterWithTarget(State.Pool, duration, target);

			data.AnchorMax = anchorMax;
			data.Ease      = ease;

			return data;
		}

		[System.Serializable]
		public class State : LeanStateWithTarget<RectTransform>
		{
			[Tooltip("The anchorMax we will transition to.")]
			public Vector2 AnchorMax;

			[Tooltip("The ease method that will be used for the transition.")]
			public LeanEase Ease = LeanEase.Smooth;

			[System.NonSerialized] private Vector2 oldAnchorMax;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.anchorMax != AnchorMax ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				AnchorMax = Target.anchorMax;
			}

			public override void BeginWithTarget()
			{
				oldAnchorMax = Target.anchorMax;
			}

			public override void UpdateWithTarget(float progress)
			{
				Target.anchorMax = Vector2.LerpUnclamped(oldAnchorMax, AnchorMax, Smooth(Ease, progress));
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
		public static RectTransform anchorMaxTransition(this RectTransform target, Vector2 anchorMax, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanRectTransformAnchorMax.Register(target, anchorMax, duration, ease); return target;
		}
	}
}