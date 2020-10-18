using UnityEngine;
using System.Collections.Generic;
using Lean.Common;
#if UNITY_EDITOR
using UnityEditor;

namespace Lean.Transition.Method
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanTime))]
	public class LeanMethod_Inspector : LeanInspector<LeanTime>
	{
		protected override void DrawInspector()
		{
			Draw("Update");
		}
	}
}
#endif

namespace Lean.Transition.Method
{
	/// <summary>This allows you to change where in the game loop transitions after this will update.
	/// NOTE: Once you submit the previous transitions, this will be reset to default.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanTime")]
	[AddComponentMenu(LeanTransition.MethodsMenuPrefix + "Time" + LeanTransition.MethodsMenuSuffix + "(LeanTime)")]
	public class LeanTime : LeanMethod
	{
		public override void Register()
		{
			LeanTransition.CurrentTiming = Update;
		}

		public LeanTiming Update = LeanTiming.Default;
	}
}

namespace Lean.Transition
{
	public static partial class LeanExtensions
	{
		/// <summary>This allows you to change where in the game loop transitions after this will update.
		/// NOTE: Once you submit the previous transitions, this will be reset to default.</summary>
		public static T TimeTransition<T>(this T target, LeanTiming update)
			where T : Component
		{
			LeanTransition.CurrentTiming = update; return target;
		}

		/// <summary>This allows you to change where in the game loop transitions after this will update.
		/// NOTE: Once you submit the previous transitions, this will be reset to default.</summary>
		public static GameObject TimeTransition(this GameObject target, LeanTiming update)
		{
			LeanTransition.CurrentTiming = update; return target;
		}
	}
}