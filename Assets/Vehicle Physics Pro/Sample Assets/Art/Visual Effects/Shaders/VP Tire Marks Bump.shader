Shader "Vehicle Physics/Tire Marks Bump"
{

Properties
	{
	_Color ("Color", Color) = (1,1,1,1)
	_MainTex ("Albedo (RGB)", 2D) = "white" {}
	_BumpMap ("Normal Map", 2D) = "bump" {}
	_BumpScale ("Normal Scale", Float) = 1.0
	_Glossiness ("Smoothness", Range(0,1)) = 0.5
	_Metallic ("Metallic", Range(0,1)) = 0.0
	}

SubShader
	{
	Tags { "RenderType"="Opaque" "IgnoreProjector"="True" "Queue"="Geometry+1" }
	LOD 200

	CGPROGRAM
	// WARNING!! (Unity 5.1)
	//		Shader receives or not shadows depending on the Rendering Mode from the material
	//		(in the Standard Shader inspector)
	//
	// I.e: Selecting the Standard shader, then changing Rendering Mode to Fade, and selecting
	// back this shader, then shadows won't be collected by objects using this material.

	#pragma surface surf Standard fullforwardshadows decal:blend vertex:vert
	#pragma target 3.0


	fixed4 _Color;
	sampler2D _MainTex;
	sampler2D _BumpMap;
	half _Glossiness;
	half _Metallic;
	half _BumpScale;


	struct Input
		{
		float2 uv_MainTex;
		float2 uv_BumpMap;
		fixed4 vertexColor;
		};


	void vert (inout appdata_full v, out Input o)
		{
		UNITY_INITIALIZE_OUTPUT(Input, o);
		o.vertexColor = v.color;
		}


	void surf (Input IN, inout SurfaceOutputStandard o)
		{
		// Regular standard shader implementation, but alpha is read from the vertex color

		fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
		o.Albedo = c.rgb;
		o.Normal = UnpackScaleNormal (tex2D (_BumpMap, IN.uv_BumpMap), _BumpScale);

		o.Metallic = _Metallic;
		o.Smoothness = _Glossiness;

		o.Alpha = c.a * IN.vertexColor.a;
		}

	ENDCG
	}
}
