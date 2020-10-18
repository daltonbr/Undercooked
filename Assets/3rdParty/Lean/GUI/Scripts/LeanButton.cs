using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Lean.Common;
using Lean.Transition;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Gui
{
	/// <summary>This component provides an alternative to Unity's UI button, allowing you to easily add custom transitions, as well as add an OnDown event.</summary>
	[HelpURL(LeanGui.HelpUrlPrefix + "LeanButton")]
	[AddComponentMenu(LeanGui.ComponentMenuPrefix + "Button")]
	public class LeanButton : Selectable, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, ISubmitHandler
	{
		/// <summary>If you enable this then OnDown + DownTransition be invoked once for each finger that begins touching the button.
		/// If you disable this then OnDown + DownTransition will only be invoked for the first finger that begins touching the button.</summary>
		public bool MultiDown { set { multiDown = value; } get { return multiDown; } } [SerializeField] private bool multiDown;

		/// <summary>If your finger presses down on the button and drags more than this many pixels, then selection will be cancelled.
		/// -1 = Unlimited drag distance.
		/// 0 = Until the finger exits the button graphic.</summary>
		public float DragThreshold { set { dragThreshold = value; } get { return dragThreshold; } } [SerializeField] private float dragThreshold = 10.0f;

		/// <summary>This allows you to perform a transition when there are no longer any fingers touching the button.
		/// You can create a new transition GameObject by right clicking the transition name, and selecting <b>Create</b>.
		/// For example, the <b>LeanGraphicColor (Graphic.color Transition)</b> component can be used to change the color back to normal.</summary>
		public LeanPlayer NormalTransitions { get { if (normalTransitions == null) normalTransitions = new LeanPlayer(); return normalTransitions; } } [SerializeField] private LeanPlayer normalTransitions;

		/// <summary>This allows you to perform a transition when a finger begins touching the button.
		/// You can create a new transition GameObject by right clicking the transition name, and selecting <b>Create</b>.
		/// For example, the <b>LeanGraphicColor (Graphic.color Transition)</b> component can be used to change the color.
		/// NOTE: Any transitions you perform here must be reverted in the <b>Normal Transitions</b> setting using a matching transition component.</summary>
		public LeanPlayer DownTransitions { get { if (downTransitions == null) downTransitions = new LeanPlayer(); return downTransitions; } } [SerializeField] private LeanPlayer downTransitions;

		/// <summary>This allows you to perform a transition when you click or tap on this button.
		/// You can create a new transition GameObject by right clicking the transition name, and selecting <b>Create</b>.
		/// For example, the <b>Play Sound Transition (LeanPlaySound)</b> component can be used to play a click sound.</summary>
		public LeanPlayer ClickTransitions { get { if (clickTransitions == null) clickTransitions = new LeanPlayer(); return clickTransitions; } } [SerializeField] private LeanPlayer clickTransitions;

		/// <summary>This allows you to perform an action when a finger begins touching the button.</summary>
		public UnityEvent OnDown { get { if (onDown == null) onDown = new UnityEvent(); return onDown; } } [SerializeField] private UnityEvent onDown;

		/// <summary>This allows you to perform an action when you click or tap on this button.</summary>
		public UnityEvent OnClick { get { if (onClick == null) onClick = new UnityEvent(); return onClick; } } [SerializeField] private UnityEvent onClick;

		[System.NonSerialized] private Vector2 totalDelta;

		/// <summary>Track the currently down pointers so we can toggle the transition.</summary>
		private List<int> downPointers = new List<int>();

		[System.NonSerialized]
		private ScrollRect parentScrollRect;

		public override void OnPointerDown(PointerEventData eventData)
		{
			base.OnPointerDown(eventData);

			if (IsInteractable() == true && eventData.button == PointerEventData.InputButton.Left)
			{
				totalDelta = Vector2.zero;

				downPointers.Add(eventData.pointerId);

				if (multiDown == true || downPointers.Count == 1)
				{
					if (downTransitions != null)
					{
						downTransitions.Begin();
					}

					if (onDown != null)
					{
						onDown.Invoke();
					}
				}
			}
		}

		public override void OnPointerUp(PointerEventData eventData)
		{
			base.OnPointerUp(eventData);

			if (downPointers.Remove(eventData.pointerId) == true)
			{
				TryNormal();
				DoClick();
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			parentScrollRect = GetComponentInParent<ScrollRect>();

			if (parentScrollRect != null)
			{
				parentScrollRect.OnBeginDrag(eventData);
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (downPointers.Contains(eventData.pointerId) == true)
			{
				totalDelta += eventData.delta;

				if (dragThreshold > 0.0f && totalDelta.magnitude > dragThreshold)
				{
					downPointers.Remove(eventData.pointerId);

					TryNormal();
				}
			}

			if (parentScrollRect != null)
			{
				parentScrollRect.OnDrag(eventData);
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (parentScrollRect != null)
			{
				parentScrollRect.OnEndDrag(eventData);
			}
		}

		public void OnSubmit(BaseEventData eventData)
		{
			if (enabled == true)
			{
				DoClick();
			}
		}

		public override void OnPointerExit(PointerEventData eventData)
		{
			base.OnPointerExit(eventData);

			if (dragThreshold == 0.0f)
			{
				downPointers.Remove(eventData.pointerId);

				TryNormal();
			}
		}
#if UNITY_EDITOR
		protected override void Reset()
		{
			base.Reset();

			transition = Selectable.Transition.None;
		}
#endif
		private void TryNormal()
		{
			if (downPointers.Count == 0)
			{
				if (normalTransitions != null)
				{
					normalTransitions.Begin();
				}
			}
		}

		private void DoClick()
		{
			if (clickTransitions != null)
			{
				clickTransitions.Begin();
			}

			if (onClick != null)
			{
				onClick.Invoke();
			}
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Gui
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanButton))]
	public class LeanButton_Inspector : LeanInspector<LeanButton>
	{
		protected override void DrawInspector()
		{
			Draw("m_Interactable");
			Draw("m_Transition");
			Draw("m_Navigation");

			EditorGUILayout.Separator();

			Draw("multiDown", "If you press multiple fingers on this button at the same time, should OnDown and DownTransition be invoked multiple times?");
			Draw("dragThreshold", "If your finger presses down on the button and drags more than this many pixels, then selection will be cancelled.\n\n-1 = Unlimited drag distance.\n\n0 = Until the finger exits the button graphic.");

			EditorGUILayout.Separator();

			Draw("normalTransitions", "This allows you to perform a transition when there are no longer any fingers touching the button. You can create a new transition GameObject by right clicking the transition name, and selecting Create. For example, the LeanGraphicColor (Graphic.color Transition) component can be used to change the color back to normal.");
			Draw("downTransitions", "This allows you to perform a transition when a finger begins touching the button. You can create a new transition GameObject by right clicking the transition name, and selecting Create. For example, the LeanGraphicColor (Graphic.color Transition) component can be used to change the color.\n\nNOTE: Any transitions you perform here must be reverted in the Normal Transitions setting using a matching transition component.");
			Draw("clickTransitions", "This event will be invoked when a pointer clicks on this button.");

			EditorGUILayout.Separator();

			Draw("onDown");
			Draw("onClick");
		}
	}
}
#endif