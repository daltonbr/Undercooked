using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified Transform.localPosition to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanTransformLocalPosition")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "Transform/Transform.localPosition" + LeanTransition.MethodsMenuSuffix + "(LeanTransformLocalPosition)")]
	public class LeanTransformLocalPosition : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(Transform);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.Position, Data.Duration, Data.Ease);
		}

		public static LeanState Register(Transform target, Vector3 position, float duration, LeanEase ease = LeanEase.Smooth)
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
			public Vector3 Position;

			[Tooltip("The ease method that will be used for the transition.")]
			public LeanEase Ease = LeanEase.Smooth;

			[System.NonSerialized] private Vector3 oldPosition;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.localPosition != Position ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				Position = Target.localPosition;
			}

			public override void BeginWithTarget()
			{
				oldPosition = Target.localPosition;
			}

			public override void UpdateWithTarget(float progress)
			{
				Target.localPosition = Vector3.LerpUnclamped(oldPosition, Position, Smooth(Ease, progress));
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
		public static Transform localPositionTransition(this Transform target, Vector3 position, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanTransformLocalPosition.Register(target, position, duration, ease); return target;
		}
	}
}