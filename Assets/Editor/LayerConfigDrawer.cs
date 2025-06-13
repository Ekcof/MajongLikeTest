using UnityEditor;
using UnityEngine;
using Majong.Level;

[CustomPropertyDrawer(typeof(LayerConfig))]
public class LayerConfigDrawer : PropertyDrawer
{
	private const float TOGGLE_SIZE = 18f;
	private const float SPACING = 2f;
	private const float INDENT_OFFSET = 15f;
	private float LineHeight => EditorGUIUtility.singleLineHeight;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		var foldRect = new Rect(position.x, position.y, position.width, LineHeight);
		property.isExpanded = EditorGUI.Foldout(foldRect, property.isExpanded, label, true);
		if (!property.isExpanded)
		{
			EditorGUI.EndProperty();
			return;
		}

		int oldIndent = EditorGUI.indentLevel;
		EditorGUI.indentLevel++;
		float indentOffset = oldIndent * INDENT_OFFSET;

		float y = position.y + LineHeight + SPACING;
		float contentWidth = position.width - indentOffset;

		var widthProp = property.FindPropertyRelative("_width");
		var heightProp = property.FindPropertyRelative("_height");
		var slotsProp = property.FindPropertyRelative("_slots");

		var wRect = new Rect(position.x + indentOffset, y, contentWidth, LineHeight);
		EditorGUI.PropertyField(wRect, widthProp);
		y += LineHeight + SPACING;

		var hRect = new Rect(position.x + indentOffset, y, contentWidth, LineHeight);
		EditorGUI.PropertyField(hRect, heightProp);
		y += LineHeight + SPACING * 2;


		// draw buttons
		float halfW = (contentWidth - SPACING) * 0.5f;
		var btnRect1 = new Rect(position.x + indentOffset, y, halfW, LineHeight);
		var btnRect2 = new Rect(position.x + indentOffset + halfW + SPACING, y, halfW, LineHeight);

		if (GUI.Button(btnRect1, "Activate All"))
		{
			for (int i = 0; i < slotsProp.arraySize; i++)
				slotsProp.GetArrayElementAtIndex(i).boolValue = true;
		}
		if (GUI.Button(btnRect2, "Deactivate All"))
		{
			for (int i = 0; i < slotsProp.arraySize; i++)
				slotsProp.GetArrayElementAtIndex(i).boolValue = false;
		}

		y += LineHeight + SPACING * 2;

		int w = Mathf.Max(1, widthProp.intValue);
		int h = Mathf.Max(1, heightProp.intValue);
		if (slotsProp.arraySize != w * h)
			slotsProp.arraySize = w * h;

		// draw grid
		for (int row = h - 1; row >= 0; row--)
		{
			float x = position.x + indentOffset;
			for (int col = 0; col < w; col++)
			{
				int idx = row * w + col;
				var cell = slotsProp.GetArrayElementAtIndex(idx);
				var tRect = new Rect(x, y, TOGGLE_SIZE, TOGGLE_SIZE);
				cell.boolValue = EditorGUI.Toggle(tRect, GUIContent.none, cell.boolValue);
				x += TOGGLE_SIZE + SPACING;
			}
			y += TOGGLE_SIZE + SPACING;
		}

		EditorGUI.indentLevel = oldIndent;
		property.serializedObject.ApplyModifiedProperties();
		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = LineHeight;

		if (property.isExpanded)
		{
			height += LineHeight * 2 + SPACING * 3;

			height += LineHeight + SPACING * 2;

			int rows = Mathf.Max(1, property.FindPropertyRelative("_height").intValue);

			height += rows * (TOGGLE_SIZE + SPACING);
		}

		return height;
	}
}