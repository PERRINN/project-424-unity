﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Vehicle Physics/Tire Particles Alpha with Shadows"
{
Properties
	{
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
	_ShadowRange ("Shadow Range", Range(0.0, 1.0)) = 0.49
	_ShadowBoost ("Shadow Boost", Range(0.0, 10.0)) = 1
	}

SubShader
	{
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	AlphaTest Greater .01
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off

	Pass
		{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile_particles
		#pragma multi_compile_fog

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		fixed4 _TintColor;

		struct appdata_t
			{
			float4 vertex : POSITION;
			fixed4 color : COLOR;
			float2 texcoord : TEXCOORD0;
			};

		struct v2f
			{
			float4 vertex : SV_POSITION;
			fixed4 color : COLOR;
			float2 texcoord : TEXCOORD0;
			UNITY_FOG_COORDS(1)
			#ifdef SOFTPARTICLES_ON
			float4 projPos : TEXCOORD2;
			#endif
			};

		float4 _MainTex_ST;

		v2f vert (appdata_t v)
			{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			#ifdef SOFTPARTICLES_ON
			o.projPos = ComputeScreenPos (o.vertex);
			COMPUTE_EYEDEPTH(o.projPos.z);
			#endif
			o.color = v.color;
			o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
			UNITY_TRANSFER_FOG(o,o.vertex);
			return o;
			}

		UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
		float _InvFade;

		fixed4 frag (v2f i) : SV_Target
			{
			#ifdef SOFTPARTICLES_ON
			float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
			float partZ = i.projPos.z;
			float fade = saturate (_InvFade * (sceneZ-partZ));
			i.color.a *= fade;
			#endif

			fixed4 col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
			UNITY_APPLY_FOG(i.fogCoord, col);
			return col;
			}
		ENDCG
		}

	//  Shadow rendering pass
	Pass
		{
		Name "ShadowCaster"
		Tags { "LightMode" = "ShadowCaster" }

		ZWrite On ZTest LEqual

		CGPROGRAM
		#pragma target 3.0
		// TEMPORARY: GLES2.0 temporarily disabled to prevent errors spam on devices without textureCubeLodEXT
		#pragma exclude_renderers gles

		// -------------------------------------

		#define _ALPHABLEND_ON 1

		#pragma vertex vertShadowCaster
		#pragma fragment fragShadowCaster

		#include "VPParticleShadows.cginc"
		ENDCG
		}

	}
}
