using UnityEngine;
using UnityEngine.EventSystems;
using Lean.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Gui
{
	/// <summary>This component allows you to change the size of the Target RectTransform based on the size of this one.
	/// This is very useful for text that needs to be inside a parent container, but you don't know how big that container should be.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(RectTransform))]
	[HelpURL(LeanGui.HelpUrlPrefix + "LeanSizer")]
	[AddComponentMenu(LeanGui.ComponentMenuPrefix + "Sizer")]
	public class LeanSizer : UIBehaviour
	{
		/// <summary>The RectTransform whose size we want to modify.</summary>
		public RectTransform Target { set { if (target != value) { target = value; UpdateSize(); } } get { return target; } } [SerializeField] private RectTransform target;

		/// <summary>Factor in scale when reading the size of the target?</summary>
		public bool Scale { set { if (scale != value) { scale = value; UpdateSize(); } } get { return scale; } } [SerializeField] private bool scale;

		/// <summary>Match the scale horizontally?</summary>
		public bool Horizontal { set { if (horizontal != value) { horizontal = value; UpdateSize(); } } get { return horizontal; } } [SerializeField] private bool horizontal;

		public float HorizontalPadding { set { if (horizontalPadding != value) { horizontalPadding = value; UpdateSize(); } } get { return horizontalPadding; } } [SerializeField] private float horizontalPadding;

		/// <summary>Match the scale vertically?</summary>
		public bool Vertical { set { if (vertical != value) { vertical = value; UpdateSize(); } } get { return vertical; } } [SerializeField] private bool vertical;

		public float VerticalPadding { set { if (verticalPadding != value) { verticalPadding = value; UpdateSize(); } } get { return verticalPadding; } } [SerializeField] private float verticalPadding;

		[System.NonSerialized]
		private RectTransform cachedRectTransform;

		[System.NonSerialized]
		private bool cachedRectTransformSet;

		[ContextMenu("Update Size")]
		public void UpdateSize()
		{
			if (Target != null)
			{
				if (cachedRectTransformSet == false)
				{
					cachedRectTransform    = GetComponent<RectTransform>();
					cachedRectTransformSet = true;
				}

				var targetSize = Target.sizeDelta;
				var scale      = cachedRectTransform.localScale;
				var sizeDelta  = cachedRectTransform.sizeDelta;

				if (Horizontal == true)
				{
					targetSize.x = sizeDelta.x + HorizontalPadding;

					if (Scale == true)
					{
						targetSize.x *= scale.x;
					}
				}

				if (Vertical == true)
				{
					targetSize.y = sizeDelta.y + VerticalPadding;

					if (Scale == true)
					{
						targetSize.y *= scale.y;
					}
				}

				Target.sizeDelta = targetSize;
			}
		}
#if UNITY_EDITOR
		protected override void OnValidate()
		{
			UpdateSize();
		}
#endif
		protected override void OnRectTransformDimensionsChange()
		{
			UpdateSize();
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Gui
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanSizer))]
	public class LeanSizer_Editor : LeanInspector<LeanSizer>
	{
		protected override void DrawInspector()
		{
			BeginError(Any(t => t.Target == null));
				Draw("target", "The RectTransform whose size we want to modify.");
			EndError();
			Draw("scale", "Factor in scale when reading the size of the target?");

			EditorGUILayout.Separator();

			Draw("horizontal", "Match the scale horizontally?");

			if (Any(t => t.Horizontal == true))
			{
				EditorGUI.indentLevel++;
					Draw("horizontalPadding", "", "Padding");
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.Separator();

			Draw("vertical", "Match the scale vertically?");

			if (Any(t => t.Vertical == true))
			{
				EditorGUI.indentLevel++;
					Draw("verticalPadding", "", "Padding");
				EditorGUI.indentLevel--;
			}
		}
	}
}
#endif