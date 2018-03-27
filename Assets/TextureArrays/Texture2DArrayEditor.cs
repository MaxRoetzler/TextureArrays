/// Date	: 27/03/2018
/// Company	: Fantastic, yes
/// Author	: Maximilian Rötzer
/// License	: This code is licensed under MIT license

using UnityEditorInternal;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (Texture2DArrayData))]
public class Texture2DArrayEditor : Editor
{
	#region Fields
	private bool m_isModified;
	private ReorderableList m_list;
	private Texture2DArrayData m_arrayData;

	private readonly GUIContent [] m_texture2DState = new GUIContent [4]
	{
		new GUIContent ("✓", "Ok"),
		new GUIContent ("✘", "Texture size does not match!"),
		new GUIContent ("✘", "Texture format is wrong!"),
		new GUIContent ("✘", "Mip map count is wrong!"),
	};
	#endregion

	/// <summary>
	/// Setup reorderable list.
	/// </summary>
	private void OnEnable ()
	{
		SerializedProperty serializedProperty = serializedObject.FindProperty ("m_textures");
		m_arrayData = (Texture2DArrayData) target;

		m_list = new ReorderableList (serializedObject, serializedProperty, true, true, true, true)
		{
			drawHeaderCallback = (Rect rect) =>
			{
				EditorGUI.LabelField (rect, new GUIContent ("Texture Array"));
			},

			drawElementCallback = (Rect rect, int index, bool isActive, bool hasFocus) =>
			{
				SerializedProperty property = m_list.serializedProperty.GetArrayElementAtIndex (index);
				Texture2D texture = (Texture2D) property.objectReferenceValue;

				int state = (int) m_arrayData.GetTextureState (texture);

				EditorGUI.BeginChangeCheck ();
				EditorGUI.PropertyField (new Rect (rect.x, rect.y + 2, 36, 16), property, GUIContent.none);
				EditorGUI.LabelField (new Rect (rect.x + 40, rect.y + 2, rect.width - 60, 16), new GUIContent (texture.name));
				EditorGUI.LabelField (new Rect (rect.width, rect.y + 2, 20, 16), m_texture2DState [state]);

				if (EditorGUI.EndChangeCheck ())
				{
					m_isModified = true;
				}
			},

			onAddCallback = (ReorderableList list) =>
			{
				list.serializedProperty.arraySize++;
				m_isModified = true;
			},

			onRemoveCallback = (ReorderableList list) =>
			{
				list.serializedProperty.GetArrayElementAtIndex (list.index).objectReferenceValue = null;
				list.serializedProperty.DeleteArrayElementAtIndex (list.index);
				m_isModified = true;
			},

			onChangedCallback = (ReorderableList list) =>
			{
				m_isModified = true;
			},
		};
	}

	/// <summary>
	/// Draw TextureArrayData inspector.
	/// </summary>
	public override void OnInspectorGUI ()
	{
		serializedObject.Update ();

		SerializedProperty autoSettings = serializedObject.FindProperty ("m_isAutomatic");
		EditorGUILayout.PropertyField (autoSettings, new GUIContent ("Automatic"));

		GUI.enabled = !autoSettings.boolValue;

		EditorGUI.BeginChangeCheck ();
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_width"));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_height"));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_format"));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_aniso"));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_wrapModeU"));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_wrapModeV"));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_mipMapCount"));

		if (EditorGUI.EndChangeCheck ())
		{
			m_isModified = true;
		}

		GUI.enabled = true;
		GUILayout.Space (16);
		m_list.DoLayoutList ();

		serializedObject.ApplyModifiedProperties ();

		if (m_isModified)
		{
			m_arrayData.Rebuild ();
			m_isModified = false;
		}
	}
}