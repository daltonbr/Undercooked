using UnityEngine;
using Lean.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Gui
{
	/// <summary>This component will automatically constrain the current <b>RectTransform.anchoredPosition</b> to the specified range.</summary>
	[ExecuteInEditMode]
	[HelpURL(LeanGui.HelpUrlPrefix + "LeanConstrainAnchoredPosition")]
	[AddComponentMenu(LeanGui.ComponentMenuPrefix + "Constrain AnchoredPosition")]
	public class LeanConstrainAnchoredPosition : MonoBehaviour
	{
		/// <summary>Constrain horizontally?</summary>
		public bool Horizontal { set { horizontal = value; } get { return horizontal; } } [SerializeField] private bool horizontal;

		/// <summary>The minimum value.</summary>
		public float HorizontalMin { set { horizontalMin = value; } get { return horizontalMin; } } [SerializeField] private float horizontalMin = -100.0f;

		/// <summary>The maximum value.</summary>
		public float HorizontalMax { set { horizontalMax = value; } get { return horizontalMax; } } [SerializeField] private float horizontalMax = 100.0f;

		/// <summary>Constrain vertically?</summary>
		public bool Vertical { set { vertical = value; } get { return vertical; } } [SerializeField] private bool vertical;

		/// <summary>The minimum value.</summary>
		public float VerticalMin { set { verticalMin = value; } get { return verticalMin; } } [SerializeField] private float verticalMin = -100.0f;

		/// <summary>The maximum value.</summary>
		public float VerticalMax { set { verticalMax = value; } get { return verticalMax; } } [SerializeField] private float verticalMax = 100.0f;

		[System.NonSerialized]
		private RectTransform cachedRectTransform;

		protected virtual void OnEnable()
		{
			cachedRectTransform = GetComponent<RectTransform>();
		}

		protected virtual void LateUpdate()
		{
			var anchoredPosition = cachedRectTransform.anchoredPosition;
			var rect             = cachedRectTransform.rect;

			if (horizontal == true)
			{
				anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, horizontalMin, horizontalMax);
			}

			if (vertical == true)
			{
				anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, verticalMin, verticalMax);
			}

			cachedRectTransform.anchoredPosition = anchoredPosition;
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Gui
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanConstrainAnchoredPosition))]
	public class LeanConstrainAnchoredPosition_Editor : LeanInspector<LeanConstrainAnchoredPosition>
	{
		protected override void DrawInspector()
		{
			Draw("horizontal", "Constrain horizontally?");

			if (Any(t => t.Horizontal == true))
			{
				EditorGUI.indentLevel++;
					Draw("horizontalMin", "The minimum value.", "Min");
					Draw("horizontalMax", "The maximum value.", "Max");
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.Separator();

			Draw("vertical", "Constrain vertically?");

			if (Any(t => t.Vertical == true))
			{
				EditorGUI.indentLevel++;
					Draw("verticalMin", "The minimum value.", "Min");
					Draw("verticalMax", "The maximum value.", "Max");
				EditorGUI.indentLevel--;
			}
		}
	}
}
#endif