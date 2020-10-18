using UnityEngine;
using Lean.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Gui
{
	/// <summary>This component will automatically snap <b>RectTransform.anchoredPosition</b> to the specified interval.</summary>
	[ExecuteInEditMode]
	[HelpURL(LeanGui.HelpUrlPrefix + "LeanSnap")]
	[AddComponentMenu(LeanGui.ComponentMenuPrefix + "Snap")]
	public class LeanSnap : MonoBehaviour
	{
		/// <summary>To prevent UI element dragging from conflicting with snapping, you can specify the drag component here.</summary>
		public LeanDrag DisableWith { set { disableWith = value; } get { return disableWith; } } [SerializeField] private LeanDrag disableWith;

		/// <summary>Snap horizontally?</summary>
		public bool Horizontal { set { horizontal = value; } get { return horizontal; } } [SerializeField] private bool horizontal;

		/// <summary>The snap points will be offset by this many pixels.</summary>
		public float HorizontalOffset { set { horizontalOffset = value; } get { return horizontalOffset; } } [SerializeField] private float horizontalOffset;

		/// <summary>The spacing between each snap point in pixels.</summary>
		public float HorizontalInterval { set { horizontalInterval = value; } get { return horizontalInterval; } } [SerializeField] private float horizontalInterval = 10.0f;

		/// <summary>The snap speed.
		/// -1 = Instant.
		/// 1 = Slow.
		/// 10 = Fast.</summary>
		public float HorizontalSpeed { set { horizontalSpeed = value; } get { return horizontalSpeed; } } [SerializeField] private float horizontalSpeed = -1.0f;

		/// <summary>Snap vertically?</summary>
		public bool Vertical { set { vertical = value; } get { return vertical; } } [SerializeField] private bool vertical;

		/// <summary>The snap points will be offset by this many pixels.</summary>
		public float VerticalOffset { set { verticalOffset = value; } get { return verticalOffset; } } [SerializeField] private float verticalOffset;

		/// <summary>The spacing between each snap point in pixels.</summary>
		public float VerticalInterval { set { verticalInterval = value; } get { return verticalInterval; } } [SerializeField] private float verticalInterval = 10.0f;

		/// <summary>The snap speed.
		/// -1 = Instant.
		/// 1 = Slow.
		/// 10 = Fast.</summary>
		public float VerticalSpeed { set { verticalSpeed = value; } get { return verticalSpeed; } } [SerializeField] private float verticalSpeed = -1.0f;

		[System.NonSerialized]
		private RectTransform cachedRectTransform;

		protected virtual void OnEnable()
		{
			cachedRectTransform = GetComponent<RectTransform>();
		}

		protected virtual void LateUpdate()
		{
			if (disableWith != null && disableWith.Dragging == true)
			{
				return;
			}

			var anchoredPosition = cachedRectTransform.anchoredPosition;
			var rect             = cachedRectTransform.rect;

			if (horizontal == true && horizontalInterval != 0.0f)
			{
				var target = Mathf.Round((anchoredPosition.x - horizontalOffset) / horizontalInterval) * horizontalInterval + horizontalOffset;
				var factor = LeanHelper.DampenFactor(horizontalSpeed, Time.deltaTime);

				anchoredPosition.x = Mathf.Lerp(anchoredPosition.x, target, factor);
			}

			if (vertical == true && verticalInterval != 0.0f)
			{
				var target = Mathf.Round((anchoredPosition.y - verticalOffset) / verticalInterval) * verticalInterval + verticalOffset;
				var factor = LeanHelper.DampenFactor(verticalSpeed, Time.deltaTime);

				anchoredPosition.y = Mathf.Lerp(anchoredPosition.y, target, factor);
			}

			cachedRectTransform.anchoredPosition = anchoredPosition;
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Gui
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanSnap))]
	public class LeanSnap_Editor : LeanInspector<LeanSnap>
	{
		protected override void DrawInspector()
		{
			Draw("horizontal", "Snap horizontally?");

			if (Any(t => t.Horizontal == true))
			{
				EditorGUI.indentLevel++;
					Draw("horizontalOffset", "The snap points will be offset by this many pixels.", "Offset");
					BeginError(Any(t => t.HorizontalInterval == 0.0f));
						Draw("horizontalInterval", "The spacing between each snap point in pixels.", "Interval");
					EndError();
					Draw("horizontalSpeed", "The snap speed.\n\n-1 = Instant.\n\n1 = Slow.\n\n10 = Fast.", "Speed");
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.Separator();

			Draw("vertical", "Snap vertically?");

			if (Any(t => t.Vertical == true))
			{
				EditorGUI.indentLevel++;
					Draw("verticalOffset", "The snap points will be offset by this many pixels.", "Offset");
					BeginError(Any(t => t.VerticalInterval == 0.0f));
						Draw("verticalInterval", "The spacing between each snap point in pixels.", "Interval");
					EndError();
					Draw("verticalSpeed", "The snap speed.\n\n-1 = Instant.\n\n1 = Slow.\n\n10 = Fast.", "Speed");
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.Separator();

			Draw("disableWith", "To prevent UI element dragging from conflicting with snapping, you can specify the drag component here.");
		}
	}
}
#endif