using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified Transform.position to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanTransformPosition")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "Transform/Transform.position" + LeanTransition.MethodsMenuSuffix + "(LeanTransformPosition)")]
	public class LeanTransformPosition : LeanMethodWithStateAndTarget
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
					return Target != null && Target.position != Position ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				Position = Target.position;
			}

			public override void BeginWithTarget()
			{
				oldPosition = Target.position;
			}

			public override void UpdateWithTarget(float progress)
			{
				Target.position = Vector3.LerpUnclamped(oldPosition, Position, Smooth(Ease, progress));
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
		public static Transform positionTransition(this Transform target, Vector3 position, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanTransformPosition.Register(target, position, duration, ease); return target;
		}
	}
}