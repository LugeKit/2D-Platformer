using UnityEditor;
using UnityEngine;

[UnityEditor.CustomPropertyDrawer(typeof(ReadOnly))]
public class ReadOnlyAttributesDrawer : UnityEditor.PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;
    }
}
