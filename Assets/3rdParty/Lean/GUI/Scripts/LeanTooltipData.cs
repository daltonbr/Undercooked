using UnityEngine;
using UnityEngine.EventSystems;
using Lean.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Gui
{
	/// <summary>This component allows you to associate text with this GameObject, allowing it to be displayed from a tooltip.</summary>
	[HelpURL(LeanGui.HelpUrlPrefix + "LeanTooltipData")]
	[AddComponentMenu(LeanGui.ComponentMenuPrefix + "Tooltip Data")]
	public class LeanTooltipData : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
		/// <summary>If you want this tooltip to hide when a selectable (e.g. Button) is disabled or non-interactable, then specify it here.</summary>
		public UnityEngine.UI.Selectable Selectable { set { selectable = value; } get { return selectable; } } [SerializeField] private UnityEngine.UI.Selectable selectable;

		/// <summary>This allows you to set the tooltip text string that is associated with this object.</summary>
		public string Text { set { text = value; } get { return text; } } [Multiline] [SerializeField] private string text;

		protected virtual void Update()
		{
			if (LeanTooltip.HoverData == this)
			{
				if (selectable != null)
				{
					LeanTooltip.HoverShow = selectable.enabled == true && selectable.interactable == true;
				}
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			LeanTooltip.HoverPointer = eventData;
			LeanTooltip.HoverData    = this;
			LeanTooltip.HoverShow    = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (LeanTooltip.HoverData == this)
			{
				LeanTooltip.HoverPointer = null;
				LeanTooltip.HoverData    = null;
				LeanTooltip.HoverShow    = false;
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			LeanTooltip.PressPointer = eventData;
			LeanTooltip.PressData    = this;
			LeanTooltip.PressShow    = true;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (LeanTooltip.PressData == this)
			{
				LeanTooltip.PressPointer = null;
				LeanTooltip.PressData    = null;
				LeanTooltip.PressShow    = false;
			}
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Gui
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanTooltipData))]
	public class LeanTooltipData_Editor : LeanInspector<LeanTooltipData>
	{
		protected override void DrawInspector()
		{
			Draw("selectable", "If you want this tooltip to hide when a selectable (e.g. Button) is disabled or non-interactable, then specify it here.");
			Draw("text", "This allows you to set the tooltip text string that is associated with this object.");
		}
	}
}
#endif