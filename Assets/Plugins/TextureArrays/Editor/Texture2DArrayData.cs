/// Date	: 09/06/2018
/// Company	: Fantastic, yes
/// Author	: Maximilian Rötzler
/// License	: This code is licensed under MIT license

using UnityEngine;
using UnityEditor;

public class Texture2DArrayData : ScriptableObject
{
	#region Fields
	[SerializeField]
	private int m_aniso;
	[SerializeField]
	private int m_width;
	[SerializeField]
	private int m_height;
	[SerializeField]
	private int m_mipMapCount;
	[SerializeField]
	private bool m_isAutomatic;
	[SerializeField]
	private TextureFormat m_format;
	[SerializeField]
	private Texture2D [] m_textures;
	[SerializeField]
	private TextureWrapMode m_wrapModeU;
	[SerializeField]
	private TextureWrapMode m_wrapModeV;
	[SerializeField]
	private Texture2DArray m_texture2DArray;
	#endregion

	/// <summary>
	/// The state of a texture within the array.
	/// </summary>
	public enum Texture2DState
	{
		Match = 0,		// Texture matches
		Size = 1,		// Size mismatch
		Format = 2,		// Format mismatch
		Mipmaps = 3,	// Mipmap count mismatch
		Missing = 4,	// Texture reference is missing
	}

	/// <summary>
	/// Updates the Texture2DArray asset.
	/// </summary>
	public void Rebuild ()
	{
		if (Validate ())
		{
			if (m_texture2DArray != null)
			{
				DestroyImmediate (m_texture2DArray, true);
				m_texture2DArray = null;
			}

			m_texture2DArray = new Texture2DArray (m_width, m_height, m_textures.Length, m_format, true);

			for (int i = 0; i < m_textures.Length; i++)
			{
				for (int m = 0; m < m_textures [i].mipmapCount; m++)
				{
					Graphics.CopyTexture (m_textures [i], 0, m, m_texture2DArray, i, m);
				}
			}

			m_texture2DArray.name = name;
			m_texture2DArray.anisoLevel = m_aniso;
			m_texture2DArray.wrapModeU = m_wrapModeU;
			m_texture2DArray.wrapModeV = m_wrapModeV;

			m_texture2DArray.Apply (false, true);

			AssetDatabase.AddObjectToAsset (m_texture2DArray, this);
			AssetDatabase.SaveAssets ();
		}
	}

	/// <summary>
	/// Get the TextureArrayState of the specified texture.
	/// </summary>
	/// <param name="texture">The texture to check.</param>
	/// <returns>The TextureArrayState.</returns>
	public Texture2DState GetTextureState (Texture2D texture)
	{
		if (texture == null)
		{
			return Texture2DState.Missing;
		}

		if (texture.width != m_width || texture.height != m_height)
		{
			return Texture2DState.Size;
		}

		if (texture.format != m_format)
		{
			return Texture2DState.Format;
		}

		if (texture.mipmapCount != m_mipMapCount)
		{
			return Texture2DState.Mipmaps;
		}

		return Texture2DState.Match;
	}

	/// <summary>
	/// Validate if all textures match match the Texture2DArray parameters.
	/// </summary>
	/// <returns>True if all textures match the Texture2DArray parameters, otherwise false.</returns>
	public bool Validate ()
	{
		ApplySettingsFromTexture (m_textures [0]);

		foreach (Texture2D texture in m_textures)
		{
			if (texture == null || texture.width != m_width || texture.height != m_height || texture.format != m_format || texture.mipmapCount != m_mipMapCount)
			{
				return false;
			}
		}

		return true;
	}

	private void ApplySettingsFromTexture (Texture2D template)
	{
		if (template != null && m_isAutomatic)
		{
			m_width = template.width;
			m_height = template.height;
			m_format = template.format;
			m_aniso = template.anisoLevel;
			m_wrapModeU = template.wrapModeU;
			m_wrapModeV = template.wrapModeV;
			m_mipMapCount = template.mipmapCount;
		}
	}

	/// <summary>
	/// Initialize Texture2DArray using supplied texture array.
	/// </summary>
	/// <param name="textures">Array of Texture2Ds.</param>
	private void Initialize (Texture2D [] textures)
	{
		m_isAutomatic = true;
		m_textures = textures;

		ApplySettingsFromTexture (textures [0]);
	}

	#region Create Asset Menu
	[MenuItem ("Assets/Create/Texture 2D Array", false, 303)]
	private static void TextureArrayItem ()
	{
		Texture2DArrayData texture2DArray = CreateInstance<Texture2DArrayData> ();
		texture2DArray.Initialize (Selection.GetFiltered<Texture2D> (SelectionMode.TopLevel));

		string assetPath = AssetDatabase.GetAssetPath (texture2DArray.m_textures [0]);
		assetPath = assetPath.Remove (assetPath.LastIndexOf ('/')) + "/Texture2DArray.asset";

		AssetDatabase.CreateAsset (texture2DArray, assetPath);
		AssetDatabase.SaveAssets ();

		Selection.activeObject = texture2DArray;
		texture2DArray.Rebuild ();
	}

	[MenuItem ("Assets/Create/Texture 2D Array", true)]
	private static bool TextureArrayItemValidation ()
	{
		return Selection.GetFiltered <Texture2D> (SelectionMode.TopLevel).Length > 0;
	}
	#endregion
}