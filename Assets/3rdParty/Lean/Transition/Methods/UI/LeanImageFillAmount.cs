using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified Image.fillAmount to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanImageFillAmount")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "Image/Image.fillAmount" + LeanTransition.MethodsMenuSuffix + "(LeanImageFillAmount)")]
	public class LeanImageFillAmount : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(Image);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.FillAmount, Data.Duration, Data.Ease);
		}

		public static LeanState Register(Image target, float fillAmount, float duration, LeanEase ease = LeanEase.Smooth)
		{
			var data = LeanTransition.RegisterWithTarget(State.Pool, duration, target);

			data.FillAmount = fillAmount;
			data.Ease       = ease;

			return data;
		}

		[System.Serializable]
		public class State : LeanStateWithTarget<Image>
		{
			[Tooltip("The fillAmount we will transition to.")]
			public float FillAmount;

			[Tooltip("The ease method that will be used for the transition.")]
			public LeanEase Ease = LeanEase.Smooth;

			[System.NonSerialized] private float oldFillAmount;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.fillAmount != FillAmount ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				FillAmount = Target.fillAmount;
			}

			public override void BeginWithTarget()
			{
				oldFillAmount = Target.fillAmount;
			}

			public override void UpdateWithTarget(float progress)
			{
				Target.fillAmount = Mathf.LerpUnclamped(oldFillAmount, FillAmount, Smooth(Ease, progress));
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
		public static Image fillAmountTransition(this Image target, float fillAmount, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanImageFillAmount.Register(target, fillAmount, duration, ease); return target;
		}
	}
}