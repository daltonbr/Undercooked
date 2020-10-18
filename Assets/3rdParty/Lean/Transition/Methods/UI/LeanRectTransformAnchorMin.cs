using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified RectTransform.anchorMin to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanRectTransformAnchorMin")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "RectTransform/RectTransform.anchorMin" + LeanTransition.MethodsMenuSuffix + "(LeanRectTransformAnchorMin)")]
	public class LeanRectTransformAnchorMin : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(RectTransform);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.AnchorMin, Data.Duration, Data.Ease);
		}

		public static LeanState Register(RectTransform target, Vector2 anchorMin, float duration, LeanEase ease = LeanEase.Smooth)
		{
			var data = LeanTransition.RegisterWithTarget(State.Pool, duration, target);

			data.AnchorMin = anchorMin;
			data.Ease      = ease;

			return data;
		}

		[System.Serializable]
		public class State : LeanStateWithTarget<RectTransform>
		{
			[Tooltip("The anchorMin we will transition to.")]
			public Vector2 AnchorMin;

			[Tooltip("The ease method that will be used for the transition.")]
			public LeanEase Ease = LeanEase.Smooth;

			[System.NonSerialized] private Vector2 oldAnchorMin;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.anchorMin != AnchorMin ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				AnchorMin = Target.anchorMin;
			}

			public override void BeginWithTarget()
			{
				oldAnchorMin = Target.anchorMin;
			}

			public override void UpdateWithTarget(float progress)
			{
				Target.anchorMin = Vector2.LerpUnclamped(oldAnchorMin, AnchorMin, Smooth(Ease, progress));
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
		public static RectTransform anchorMinTransition(this RectTransform target, Vector2 anchorMin, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanRectTransformAnchorMin.Register(target, anchorMin, duration, ease); return target;
		}
	}
}