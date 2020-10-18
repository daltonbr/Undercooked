using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified <b>Transform.position.y</b> to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanTransformPositionY")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "Transform/Transform.position.y" + LeanTransition.MethodsMenuSuffix + "(LeanTransformPositionY)")]
	public class LeanTransformPositionY : LeanMethodWithStateAndTarget
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
					return Target != null && Target.position.y != Position ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				Position = Target.position.y;
			}

			public override void BeginWithTarget()
			{
				oldPosition = Target.position.y;
			}

			public override void UpdateWithTarget(float progress)
			{
				var position = Target.position;

				position.y = Mathf.LerpUnclamped(oldPosition, Position, Smooth(Ease, progress));

				Target.position = position;
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
		public static Transform positionTransition_Y(this Transform target, float position, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanTransformPositionY.Register(target, position, duration, ease); return target;
		}
	}
}