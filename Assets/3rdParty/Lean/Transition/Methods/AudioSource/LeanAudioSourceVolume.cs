using UnityEngine;
using System.Collections.Generic;

namespace Lean.Transition.Method
{
	/// <summary>This component allows you to transition the specified AudioSource.volume to the target value.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanCanvasGroupAlpha")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "AudioSource/AudioSource.volume" + LeanTransition.MethodsMenuSuffix + "(LeanAudioSourceVolume)")]
	public class LeanAudioSourceVolume : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(AudioSource);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.Volume, Data.Duration, Data.Ease);
		}

		public static LeanState Register(AudioSource target, float volume, float duration, LeanEase ease = LeanEase.Smooth)
		{
			var data = LeanTransition.RegisterWithTarget(State.Pool, duration, target);

			data.Volume = volume;
			data.Ease   = ease;

			return data;
		}

		[System.Serializable]
		public class State : LeanStateWithTarget<AudioSource>
		{
			[Tooltip("The volume we will transition to.")]
			[Range(0.0f, 1.0f)]
			public float Volume = 1.0f;

			[Tooltip("The ease method that will be used for the transition.")]
			public LeanEase Ease  = LeanEase.Smooth;

			[System.NonSerialized] private float oldVolume;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.volume != Volume ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				Volume = Target.volume;
			}

			public override void BeginWithTarget()
			{
				oldVolume = Target.volume;
			}

			public override void UpdateWithTarget(float progress)
			{
				Target.volume = Mathf.LerpUnclamped(oldVolume, Volume, Smooth(Ease, progress));
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
		public static AudioSource volumeTransition(this AudioSource target, float volume, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanAudioSourceVolume.Register(target, volume, duration, ease); return target;
		}
	}
}