using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified CanvasGroup.blocksRaycasts to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanCanvasGroupBlocksRaycasts")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "CanvasGroup/CanvasGroup.blocksRaycasts" + LeanTransition.MethodsMenuSuffix + "(LeanCanvasGroupBlocksRaycasts)")]
	public class LeanCanvasGroupBlocksRaycasts : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(CanvasGroup);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.BlocksRaycasts, Data.Duration);
		}

		public static LeanState Register(CanvasGroup target, bool blocksRaycasts, float duration)
		{
			var data = LeanTransition.RegisterWithTarget(State.Pool, duration, target);

			data.BlocksRaycasts = blocksRaycasts;

			return data;
		}

		[System.Serializable]
		public class State : LeanStateWithTarget<CanvasGroup>
		{
			[Tooltip("The blocksRaycasts we will transition to.")]
			public bool BlocksRaycasts = true;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.blocksRaycasts != BlocksRaycasts ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				BlocksRaycasts = Target.blocksRaycasts;
			}

			public override void BeginWithTarget()
			{
			}

			public override void UpdateWithTarget(float progress)
			{
				if (progress == 1.0f)
				{
					Target.blocksRaycasts = BlocksRaycasts;
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
		public static CanvasGroup blocksRaycastsTransition(this CanvasGroup target, bool blocksRaycasts, float duration)
		{
			Method.LeanCanvasGroupBlocksRaycasts.Register(target, blocksRaycasts, duration); return target;
		}
	}
}