Shader "Example/Sample2DArrayTexture"
{
	Properties
	{
		_MyArr("Tex", 2DArray) = "" {}
	_SliceRange("Slices", Range(1,24)) = 0
		_UVScale("UVScale", Float) = 1.0
	}
		SubShader
	{
		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.5

#include "UnityCG.cginc"

		struct v2f
	{
		float3 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
	};

	float _SliceRange;
	float _UVScale;

	v2f vert(float4 vertex : POSITION)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(vertex);
		o.uv.xy = (vertex.xy + 0.5) * _UVScale;
		o.uv.z = (vertex.z + 0.5) * _SliceRange;
		return o;
	}

	UNITY_DECLARE_TEX2DARRAY(_MyArr);

	half4 frag(v2f i) : SV_Target
	{
		return UNITY_SAMPLE_TEX2DARRAY(_MyArr, i.uv);
	}
		ENDCG
	}
	}
}