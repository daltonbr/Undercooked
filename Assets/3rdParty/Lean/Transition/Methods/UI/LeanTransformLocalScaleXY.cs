using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified Transform.localScale.xy to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanTransformLocalScaleXY")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "Transform/Transform.localScale.xy" + LeanTransition.MethodsMenuSuffix + "(LeanTransformLocalScaleXY)")]
	public class LeanTransformLocalScaleXY : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(Transform);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.Scale, Data.Duration, Data.Ease);
		}

		public static LeanState Register(Transform target, Vector2 scale, float duration, LeanEase ease = LeanEase.Smooth)
		{
			var data = LeanTransition.RegisterWithTarget(State.Pool, duration, target);

			data.Scale = scale;
			data.Ease  = ease;

			return data;
		}

		[System.Serializable]
		public class State : LeanStateWithTarget<Transform>
		{
			[Tooltip("The scale we will transition to.")]
			public Vector2 Scale = Vector2.one;

			[Tooltip("The ease method that will be used for the transition.")]
			public LeanEase Ease = LeanEase.Smooth;

			[System.NonSerialized] public Vector2 OldScale;

			public override int CanFill
			{
				get
				{
					return Target != null && (Target.localScale.x != Scale.x || Target.localScale.y != Scale.y) ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				Scale = Target.localScale;
			}

			public override void BeginWithTarget()
			{
				OldScale = Target.localScale;
			}

			public override void UpdateWithTarget(float progress)
			{
				var localScale = Target.localScale;
				var smooth     = Smooth(Ease, progress);

				localScale.x = Mathf.LerpUnclamped(OldScale.x, Scale.x, smooth);
				localScale.y = Mathf.LerpUnclamped(OldScale.y, Scale.y, smooth);

				Target.localScale = localScale;
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
		public static Transform localScaleTransition_XY(this Transform target, Vector2 scale, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanTransformLocalScale.Register(target, scale, duration, ease); return target;
		}
	}
}