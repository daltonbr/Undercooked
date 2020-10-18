using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified Transform.localPosition.x to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanTransformLocalPositionX")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "Transform/Transform.localPosition.x" + LeanTransition.MethodsMenuSuffix + "(LeanTransformLocalPositionX)")]
	public class LeanTransformLocalPositionX : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(Transform);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.Position, Data.Duration, Data.Ease);
		}

		public static LeanState Register(Transform target, float position, float duration, LeanEase ease = LeanEase.Smooth)
		{
			var data = LeanTransition.RegisterWithTarget(State.Pool, duration, target);

			data.Position = position;
			data.Ease     = ease;

			return data;
		}

		[System.Serializable]
		public class State : LeanStateWithTarget<Transform>
		{
			[Tooltip("The position we will transition to.")]
			public float Position;

			[Tooltip("The ease method that will be used for the transition.")]
			public LeanEase Ease = LeanEase.Smooth;

			[System.NonSerialized] private float oldPosition;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.localPosition.x != Position ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				Position = Target.localPosition.x;
			}

			public override void BeginWithTarget()
			{
				oldPosition = Target.localPosition.x;
			}

			public override void UpdateWithTarget(float progress)
			{
				var localPosition = Target.localPosition;

				localPosition.x = Mathf.LerpUnclamped(oldPosition, Position, Smooth(Ease, progress));

				Target.localPosition = localPosition;
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
		public static Transform localPositionTransition_X(this Transform target, float position, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanTransformLocalPositionX.Register(target, position, duration, ease); return target;
		}
	}
}