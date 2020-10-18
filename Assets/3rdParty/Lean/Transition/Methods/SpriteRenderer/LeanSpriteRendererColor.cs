using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified <b>SpriteRenderer.color</b> to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanSpriteRendererColor")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "SpriteRenderer/SpriteRenderer.color" + LeanTransition.MethodsMenuSuffix + "(LeanSpriteRendererColor)")]
	public class LeanSpriteRendererColor : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(SpriteRenderer);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.Color, Data.Duration, Data.Ease);
		}

		public static LeanState Register(SpriteRenderer target, Color color, float duration, LeanEase ease = LeanEase.Smooth)
		{
			var data = LeanTransition.RegisterWithTarget(State.Pool, duration, target);

			data.Color = color;
			data.Ease  = ease;

			return data;
		}

		[System.Serializable]
		public class State : LeanStateWithTarget<SpriteRenderer>
		{
			[Tooltip("The color we will transition to.")]
			public Color Color = Color.white;

			[Tooltip("The ease method that will be used for the transition.")]
			public LeanEase Ease  = LeanEase.Smooth;

			[System.NonSerialized] private Color oldColor;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.color != Color ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				Color = Target.color;
			}

			public override void BeginWithTarget()
			{
				oldColor = Target.color;
			}

			public override void UpdateWithTarget(float progress)
			{
				Target.color = UnityEngine.Color.LerpUnclamped(oldColor, Color, Smooth(Ease, progress));
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
		public static SpriteRenderer colorTransition(this SpriteRenderer target, Color color, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanSpriteRendererColor.Register(target, color, duration, ease); return target;
		}
	}
}