using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Lean.Common;
using Lean.Transition;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Gui
{
	/// <summary>This component allows you to display a tooltip as long as the mouse is hovering over the current UI element, or a finger is on top.
	/// Tooltips will display for any raycastable UI element that has the <b>LeanTooltipData</b> component.</summary>
	[RequireComponent(typeof(RectTransform))]
	[HelpURL(LeanGui.HelpUrlPrefix + "LeanTooltip")]
	[AddComponentMenu(LeanGui.ComponentMenuPrefix + "Tooltip")]
	public class LeanTooltip : MonoBehaviour
	{
		[System.Serializable] public class UnityEventString : UnityEvent<string> {}

		public static PointerEventData HoverPointer;
		public static LeanTooltipData  HoverData;
		public static bool             HoverShow;

		public static PointerEventData PressPointer;
		public static LeanTooltipData  PressData;
		public static bool             PressShow;

		public enum BoundaryType
		{
			None,
			Pivot,
			Position
		}

		public enum ActivationType
		{
			HoverOrPress,
			Hover,
			Press
		}

		/// <summary>This allows you to control how the tooltip will behave when it goes outside the screen bounds.</summary>
		public BoundaryType Boundary { set { boundary = value; } get { return boundary; } } [SerializeField] private BoundaryType boundary;

		/// <summary>This allows you to control when the tooltip will appear.
		/// HoverOrPress = When the mouse is hovering, or when the mouse/finger is pressing.
		/// Hover = Only when the mouse is hovering.
		/// Press = Only when the mouse/finger is pressing.</summary>
		public ActivationType Activation { set { activation = value; } get { return activation; } } [SerializeField] private ActivationType activation;

		/// <summary>This allows you to delay how quickly the tooltip will appear or switch.</summary>
		public float ShowDelay { set { showDelay = value; } get { return showDelay; } } [SerializeField] private float showDelay;

		/// <summary>This allows you to perform a transition when this tooltip appears.
		/// You can create a new transition GameObject by right clicking the transition name, and selecting <b>Create</b>.
		/// For example, the <b>LeanGraphicColor (Graphic.color Transition)</b> component can be used to change the color.
		/// NOTE: Any transitions you perform here should be reverted in the <b>Hide Transitions</b> setting using a matching transition component.</summary>
		public LeanPlayer ShowTransitions { get { if (showTransitions == null) showTransitions = new LeanPlayer(); return showTransitions; } } [SerializeField] private LeanPlayer showTransitions;

		/// <summary>This allows you to perform a transition when this tooltip hides.
		/// You can create a new transition GameObject by right clicking the transition name, and selecting <b>Create</b>.
		/// For example, the <b>LeanGraphicColor (Graphic.color Transition)</b> component can be used to change the color.</summary>
		public LeanPlayer HideTransitions { get { if (hideTransitions == null) hideTransitions = new LeanPlayer(); return hideTransitions; } } [SerializeField] private LeanPlayer hideTransitions;

		/// <summary>This allows you to perform an action when this tooltip appears.</summary>
		public UnityEventString OnShow { get { if (onShow == null) onShow = new UnityEventString(); return onShow; } } [SerializeField] private UnityEventString onShow;

		/// <summary>This allows you to perform an action when this tooltip hides.</summary>
		public UnityEvent OnHide { get { if (onHide == null) onHide = new UnityEvent(); return onHide; } } [SerializeField] private UnityEvent onHide;

		[System.NonSerialized]
		private LeanTooltipData tooltip;

		[System.NonSerialized]
		private RectTransform cachedRectTransform;

		[System.NonSerialized]
		private bool cachedRectTransformSet;

		[System.NonSerialized]
		private float currentDelay;

		[System.NonSerialized]
		private bool shown;

		private static Vector3[] corners = new Vector3[4];

		protected virtual void Update()
		{
			if (cachedRectTransformSet == false)
			{
				cachedRectTransform    = GetComponent<RectTransform>();
				cachedRectTransformSet = true;
			}

			var finalData  = default(LeanTooltipData);
			var finalPoint = default(Vector2);

			switch (activation)
			{
				case ActivationType.HoverOrPress:
				{
					if (HoverShow == true)
					{
						finalData  = HoverData;
						finalPoint = HoverPointer.position;
					}
				}
				break;

				case ActivationType.Hover:
				{
					if (HoverShow == true && PressShow == false)
					{
						finalData  = HoverData;
						finalPoint = HoverPointer.position;
					}
				}
				break;

				case ActivationType.Press:
				{
					if (PressShow == true && HoverShow == true && HoverData == PressData)
					{
						finalData  = PressData;
						finalPoint = PressPointer.position;
					}
				}
				break;
			}

			if (tooltip != finalData)
			{
				currentDelay  = 0.0f;
				tooltip       = finalData;
				shown         = false;

				Hide();
			}

			if (tooltip != null)
			{
				currentDelay += Time.unscaledDeltaTime;

				if (currentDelay >= showDelay)
				{
					if (shown == false)
					{
						Show();
					}

					cachedRectTransform.position = finalPoint;
				}
			}

			if (boundary != BoundaryType.None)
			{
				cachedRectTransform.GetWorldCorners(corners);

				var min = Vector2.Min(corners[0], Vector2.Min(corners[1], Vector2.Min(corners[2], corners[3])));
				var max = Vector2.Max(corners[0], Vector2.Max(corners[1], Vector2.Max(corners[2], corners[3])));

				if (boundary == BoundaryType.Pivot)
				{
					var pivot = cachedRectTransform.pivot;

					if (min.x < 0.0f) pivot.x = 0.0f; else if (max.x > Screen.width ) pivot.x = 1.0f;
					if (min.y < 0.0f) pivot.y = 0.0f; else if (max.y > Screen.height) pivot.y = 1.0f;

					cachedRectTransform.pivot = pivot;
				}

				if (boundary == BoundaryType.Position)
				{
					var position = cachedRectTransform.position;

					if (min.x < 0.0f) position.x -= min.x; else if (max.x > Screen.width ) position.x -= max.x - Screen.width;
					if (min.y < 0.0f) position.y -= min.y; else if (max.y > Screen.height) position.y -= max.y - Screen.height;

					cachedRectTransform.position = position;
				}
			}
		}

		private void Show()
		{
			shown = true;

			if (showTransitions != null)
			{
				showTransitions.Begin();
			}

			if (onShow != null)
			{
				onShow.Invoke(tooltip.Text);
			}
		}

		private void Hide()
		{
			if (hideTransitions != null)
			{
				hideTransitions.Begin();
			}

			if (onHide != null)
			{
				onHide.Invoke();
			}
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Gui
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanTooltip))]
	public class LeanTooltip_Editor : LeanInspector<LeanTooltip>
	{
		protected override void DrawInspector()
		{
			Draw("boundary", "This allows you to control how the tooltip will behave when it goes outside the screen bounds.");
			Draw("activation", "This allows you to control when the tooltip will appear.\nHoverOrPress = When the mouse is hovering, or when the mouse/finger is pressing.\nHover = Only when the mouse is hovering.\nPress = Only when the mouse/finger is pressing.");
			Draw("showDelay", "This allows you to delay how quickly the tooltip will appear or switch.");

			EditorGUILayout.Separator();

			Draw("showTransitions", "This allows you to perform a transition when this tooltip appears. You can create a new transition GameObject by right clicking the transition name, and selecting Create. For example, the <b>LeanGraphicColor (Graphic.color Transition)</b> component can be used to change the color.\n\nNOTE: Any transitions you perform here should be reverted in the Hide Transitions setting using a matching transition component.");
			Draw("hideTransitions", "This allows you to perform a transition when this tooltip hides. You can create a new transition GameObject by right clicking the transition name, and selecting Create. For example, the <b>LeanGraphicColor (Graphic.color Transition)</b> component can be used to change the color.");

			EditorGUILayout.Separator();

			Draw("onShow");
			Draw("onHide");
		}
	}
}
#endif