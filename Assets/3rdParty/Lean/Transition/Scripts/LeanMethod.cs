using UnityEngine;
using Lean.Common;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Transition
{
	/// <summary>This is the base class for all transition methods.</summary>
	public abstract class LeanMethod : MonoBehaviour
	{
		public abstract void Register();

		[ContextMenu("Begin This Transition")]
		public void BeginThisTransition()
		{
			LeanTransition.RequireSubmitted();

			LeanTransition.CurrentAliases.Clear();

			Register();

			LeanTransition.Submit();
		}

		[ContextMenu("Begin All Transitions")]
		public void BeginAllTransitions()
		{
			LeanTransition.CurrentAliases.Clear();

			LeanTransition.BeginAllTransitions(transform);
		}

		/// <summary>This will take the input linear 0..1 value, and return a transformed version based on the specified easing function.</summary>
		public static float Smooth(LeanEase ease, float progress)
		{
			switch (ease)
			{
				case LeanEase.Smooth:
				{
					progress = progress * progress * (3.0f - 2.0f * progress);
				}
				break;

				case LeanEase.Accelerate:
				{
					progress *= progress;
				}
				break;

				case LeanEase.Decelerate:
				{
					progress = 1.0f - progress;
					progress *= progress;
					progress = 1.0f - progress;
				}
				break;

				case LeanEase.Elastic:
				{
					var angle   = progress * Mathf.PI * 4.0f;
					var weightA = 1.0f - Mathf.Pow(progress, 0.125f);
					var weightB = 1.0f - Mathf.Pow(1.0f - progress, 8.0f);

					progress = Mathf.LerpUnclamped(0.0f, 1.0f - Mathf.Cos(angle) * weightA, weightB);
				}
				break;

				case LeanEase.Back:
				{
					progress = 1.0f - progress;
					progress = progress * progress * progress - progress * Mathf.Sin(progress * Mathf.PI);
					progress = 1.0f - progress;
				}
				break;

				case LeanEase.Bounce:
				{
					if (progress < (4f/11f))
					{
						progress = (121f/16f) * progress * progress;
					}
					else if (progress < (8f/11f))
					{
						progress = (121f/16f) * (progress - (6f/11f)) * (progress - (6f/11f)) + 0.75f;
					}
					else if (progress < (10f/11f))
					{
						progress = (121f/16f) * (progress - (9f/11f)) * (progress - (9f/11f)) + (15f/16f);
					}
					else
					{
						progress = (121f/16f) * (progress - (21f/22f)) * (progress - (21f/22f)) + (63f/64f);
					}
				}
				break;
			}

			return progress;
		}
	}

	public abstract class LeanMethodWithState : LeanMethod
	{
		/// <summary>Each time this transition method registers a new state, it will be stored here.</summary>
		public LeanState PreviousState;
	}

	public abstract class LeanMethodWithStateAndTarget : LeanMethodWithState
	{
		public abstract System.Type GetTargetType();

		[UnityEngine.Serialization.FormerlySerializedAs("Data.TargetAlias")]
		public string Alias;

		/// <summary>This allows you to get the current <b>Target</b> value, or an alised override.</summary>
		public T GetAliasedTarget<T>(T current)
			where T : Object
		{
			if (string.IsNullOrEmpty(Alias) == false)
			{
				var target = default(Object);

				if (LeanTransition.CurrentAliases.TryGetValue(Alias, out target) == true)
				{
					if (target is T)
					{
						return (T)target;
					}
					else if (target is GameObject)
					{
						var gameObject = (GameObject)target;

						return gameObject.GetComponent(typeof(T)) as T;
					}
				}
			}

			return current;
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Transition
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanMethod), true)]
	public class LeanMethod_Inspector : LeanInspector<LeanMethod>
	{
		private bool expandAlias;

		private static List<LeanMethod> tempMethods = new List<LeanMethod>();

		private int Order(LeanMethod a)
		{
			a.GetComponents(tempMethods); return tempMethods.IndexOf(a);
		}

		private void DrawTargetAlias(SerializedProperty sTarget, SerializedProperty sAlias, System.Reflection.FieldInfo data)
		{
			EditorGUILayout.Separator();

			var rect  = Reserve();
			var rectF = rect; rectF.xMin += EditorGUIUtility.labelWidth - 50; rectF.width = 48;
			var rectM = rect; rectM.xMin += EditorGUIUtility.labelWidth;
			var rectL = rectM; rectL.xMax -= rectL.width / 2;
			var rectR = rectM; rectR.xMin += rectR.width / 2;

			if (data != null)
			{
				var state = data.GetValue(tgt) as LeanState;

				if (state != null && state.CanFill > -1)
				{
					var rectC = rectM; rectC.width = 28; rectC.x -= 30;

					EditorGUI.BeginDisabledGroup(state.CanFill == 0);
						if (GUI.Button(rectC, new GUIContent("fill", "Copy the current value from the Target into this component?"), EditorStyles.miniButton) == true)
						{
							state.Fill();
						}
					EditorGUI.EndDisabledGroup();
				}
			}

			var label = new GUIContent("Target", "This is the target of the transition. For most transition methods this will be the component that will be modified.");

			if (string.IsNullOrEmpty(sAlias.stringValue) == false)
			{
				expandAlias = true;
			}

			if (string.IsNullOrEmpty(sAlias.stringValue) == true)
			{
				DrawExpand(ref expandAlias, rect);
			}

			if (expandAlias == true)
			{
				label.text += " : Alias";
			}

			EditorGUI.LabelField(rect, label);

			BeginError(sTarget.objectReferenceValue == null && string.IsNullOrEmpty(sAlias.stringValue) == true);
				if (expandAlias == true)
				{
					EditorGUI.PropertyField(rectL, sTarget, GUIContent.none);
					EditorGUI.PropertyField(rectR, sAlias, GUIContent.none);
				}
				else
				{
					EditorGUI.PropertyField(rectM, sTarget, GUIContent.none);
				}
			EndError();

			if (string.IsNullOrEmpty(sAlias.stringValue) == false)
			{
				var methodST      = (LeanMethodWithStateAndTarget)tgt;
				var expectedType  = methodST.GetTargetType();
				var expectedAlias = sAlias.stringValue;

				foreach (var method in tgt.GetComponents<LeanMethodWithStateAndTarget>())
				{
					var methodTargetType = method.GetTargetType();

					if (methodTargetType != expectedType)
					{
						if (method.Alias == expectedAlias)
						{
							if (methodTargetType.IsSubclassOf(typeof(Component)) == true && expectedType.IsSubclassOf(typeof(Component)) == true)
							{
								continue;
							}

							EditorGUILayout.HelpBox("This alias is used by multiple transitions. This only works if they all transition the same type (e.g. Transform.localPosition & Transform.localScale both transition Transform).", MessageType.Error);

							break;
						}
					}
				}
			}
		}

		private void DrawAutoFill(System.Reflection.FieldInfo data)
		{
			var state = data.GetValue(tgt) as LeanState;

			if (state != null && state.CanFill > -1)
			{
				var rect = Reserve();

				EditorGUI.BeginDisabledGroup(state.CanFill == 0);
					if (GUI.Button(rect, new GUIContent("auto fill", "Copy the current value from the scene into this component?"), EditorStyles.miniButton) == true)
					{
						state.Fill();
					}
				EditorGUI.EndDisabledGroup();
			}
		}

		protected override void DrawInspector()
		{
			var dataProperty = serializedObject.FindProperty("Data");

			if (dataProperty != null)
			{
				var sTarget = serializedObject.FindProperty("Data.Target");
				var sAlias  = serializedObject.FindProperty("Alias");
				var data    = tgt.GetType().GetField("Data");

				Draw("Data.Duration", "The transition will complete after this many seconds.");

				if (sTarget != null && sAlias != null)
				{
					DrawTargetAlias(sTarget, sAlias, data);
				}

				dataProperty.NextVisible(true);

				while (true)
				{
					if (dataProperty.name != "Duration" && dataProperty.name != "Target" && dataProperty.name != "TargetAlias")
					{
						if (dataProperty.propertyType == SerializedPropertyType.Quaternion)
						{
							EditorGUI.BeginChangeCheck();
							EditorGUI.showMixedValue = dataProperty.hasMultipleDifferentValues;

							var eulerAngles = EditorGUILayout.Vector3Field(new GUIContent(dataProperty.displayName, dataProperty.tooltip), dataProperty.quaternionValue.eulerAngles);

							EditorGUI.showMixedValue = false;

							if (EditorGUI.EndChangeCheck() == true)
							{
								dataProperty.quaternionValue = Quaternion.Euler(eulerAngles);
							}
						}
						else
						{
							EditorGUILayout.PropertyField(dataProperty);
						}
					}

					if (dataProperty.NextVisible(false) == false)
					{
						break;
					}
				}

				if (sTarget == null && data != null)
				{
					DrawAutoFill(data);
				}
			}
			else
			{
				var property = serializedObject.GetIterator(); property.NextVisible(true);

				while (property.NextVisible(false) == true)
				{
					if (property.name == "Target")
					{
						BeginError(property.objectReferenceValue == null);
							EditorGUILayout.PropertyField(property);
						EndError();
					}
					else
					{
						EditorGUILayout.PropertyField(property);
					}
				}
			}
		}
	}
}
#endif