Shader "Vehicle Physics/Tire Marks"
{

Properties
	{
	_Color ("Color", Color) = (1,1,1,1)
	_MainTex ("Albedo (RGB)", 2D) = "white" {}
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
	half _Glossiness;
	half _Metallic;


	struct Input
		{
		float2 uv_MainTex;
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

		o.Metallic = _Metallic;
		o.Smoothness = _Glossiness;

		o.Alpha = c.a * IN.vertexColor.a;
		}

	ENDCG
	}
}
