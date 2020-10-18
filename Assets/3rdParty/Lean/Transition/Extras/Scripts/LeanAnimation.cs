using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Transition.Extras
{
	/// <summary>This component allows you to manually begin transitions from UI button events and other sources.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanAnimation")]
	[AddComponentMenu(LeanTransition.ComponentMenuPrefix + "Lean Animation")]
	public class LeanAnimation : MonoBehaviour
	{
		/// <summary>This stores the <b>Transform</b>s containing all the transitions that will be performed.</summary>
		public LeanPlayer Transitions { get { if (transitions == null) transitions = new LeanPlayer(); return transitions; } } [SerializeField] [UnityEngine.Serialization.FormerlySerializedAs("Transitions")] private LeanPlayer transitions;

		/// <summary>This method will execute all transitions on the <b>Transform</b> specified in the <b>Transitions</b> setting.</summary>
		[ContextMenu("Begin Transitions")]
		public void BeginTransitions()
		{
			if (transitions != null)
			{
				transitions.Begin();
			}
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Transition.Extras
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanAnimation))]
	public class LeanAnimation_Inspector : Lean.Common.LeanInspector<LeanAnimation>
	{
		protected override void DrawInspector()
		{
			Draw("transitions", "This stores the Transforms containing all the transitions that will be performed.");
		}
	}
}
#endif