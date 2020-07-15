
// Standard shader with projector disabled


Shader "Vehicle Physics/Standard Variants/No Projector"
	{
	Properties
		{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB) Mask (A)", 2D) = "white" {}

		[NoScaleOffset] _MetallicGlossMap("Metallic", 2D) = "white" {}
		_MetallicScale ("Metallic scale", Range(0,1)) = 1.0
		_GlossinessScale ("Smoothness scale", Range(0,1)) = 1.0

		[NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" {}
		_BumpScale ("Normal Scale", Float) = 1.0
		}


	SubShader
		{
		Tags { "RenderType"="Opaque" "IgnoreProjector"="True" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _MetallicGlossMap;

		struct Input
			{
			float2 uv_MainTex;
			};

		half _GlossinessScale;
		half _MetallicScale;
		half _BumpScale;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o)
			{
			// Albedo comes from a texture tinted by color

			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			fixed3 albedo = c.rgb;

			o.Albedo = albedo * _Color.rgb;

			// Metallic and smoothness come from slider variables

			fixed4 metallic = tex2D(_MetallicGlossMap, IN.uv_MainTex);

			o.Metallic = _MetallicScale * metallic.r;
			o.Smoothness = _GlossinessScale * metallic.a;
			o.Alpha = c.a * _Color.a;

			// Other

			o.Normal = UnpackScaleNormal (tex2D (_BumpMap, IN.uv_MainTex), _BumpScale);
			}
		ENDCG
		}
	FallBack "Diffuse"
	}


/*
// From http://forum.unity3d.com/threads/extend-standard-shader.297682/

Shader "Custom/Standard Enhanced"
	{
	Properties
		{
		_Albedo ("Albedo (RGB), Alpha (A)", 2D) = "white" {}
		[NoScaleOffset]
		_Metallic ("Metallic (R), Occlusion (G), Emission (B), Smoothness (A)", 2D) = "black" {}
		[NoScaleOffset]
		_Normal ("Normal (RGB)", 2D) = "bump" {}
		}

	SubShader
		{
		Tags
			{
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
			}

		CGINCLUDE
		#define _GLOSSYENV 1
		ENDCG

		CGPROGRAM
		#pragma target 3.0
		#include "UnityPBSLighting.cginc"
		#pragma surface surf Standard
		#pragma exclude_renderers gles

		struct Input
			{
			float2 uv_Albedo;
			};

		sampler2D _Albedo;
		sampler2D _Normal;
		sampler2D _Metallic;

		void surf (Input IN, inout SurfaceOutputStandard o)
			{
			fixed4 albedo = tex2D(_Albedo, IN.uv_Albedo);
			fixed4 metallic = tex2D(_Metallic, IN.uv_Albedo);
			fixed3 normal = UnpackScaleNormal(tex2D(_Normal, IN.uv_Albedo), 1);

			o.Albedo = albedo.rgb;
			o.Alpha = albedo.a;
			o.Normal = normal;
			o.Smoothness = metallic.a;
			o.Occlusion = metallic.g;
			o.Emission = metallic.b;
			o.Metallic = metallic.r;
			}
		ENDCG
		}

	FallBack "Diffuse"
	}
*/
