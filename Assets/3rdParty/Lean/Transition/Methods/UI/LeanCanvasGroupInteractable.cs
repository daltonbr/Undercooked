using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified CanvasGroup.interactable to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanCanvasGroupInteractable")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "CanvasGroup/CanvasGroup.interactable" + LeanTransition.MethodsMenuSuffix + "(LeanCanvasGroupInteractable)")]
	public class LeanCanvasGroupInteractable : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(CanvasGroup);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.Interactable, Data.Duration);
		}

		public static LeanState Register(CanvasGroup target, bool interactable, float duration)
		{
			var data = LeanTransition.RegisterWithTarget(State.Pool, duration, target);

			data.Interactable = interactable;

			return data;
		}

		[System.Serializable]
		public class State : LeanStateWithTarget<CanvasGroup>
		{
			[Tooltip("The interactable we will transition to.")]
			public bool Interactable = true;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.interactable != Interactable ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				Interactable = Target.interactable;
			}

			public override void UpdateWithTarget(float progress)
			{
				if (progress == 1.0f)
				{
					Target.interactable = Interactable;
				}
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
		public static CanvasGroup interactableTransition(this CanvasGroup target, bool interactable, float duration)
		{
			Method.LeanCanvasGroupInteractable.Register(target, interactable, duration); return target;
		}
	}
}