Shader "Custom/GLGraphics/Depth Fade"
{
Properties
	{
	_Fade ("3D Fade", Range(0,1)) = 0.1
	[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Integer) = 2
	_StencilRef ("StencilRef", Integer) = 1
	}

SubShader
	{
	LOD 100

	Tags
		{
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"
		}

	Cull [_Cull]
	Lighting Off
	ZWrite Off
	ZTest Off
	Fog { Mode Off }
	Offset 0, 0
	Blend SrcAlpha OneMinusSrcAlpha

	// Pass 0 - Transparent with depth fade
	Pass
		{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 2.0
		#include "UnityCG.cginc"

		struct appdata_t
			{
			float4 vertex : POSITION;
			fixed4 color : COLOR;
			};

		struct v2f
			{
			float4 vertex : SV_POSITION;
			fixed4 color : COLOR;
			float4 projPos : TEXCOORD2;
			};

		v2f vert (appdata_t v)
			{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.projPos = ComputeScreenPos(o.vertex);
			COMPUTE_EYEDEPTH(o.projPos.z);
			o.color = v.color;
			return o;
			}

		UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
		float _Fade;

		fixed4 frag (v2f i) : COLOR
			{
			float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
			float partZ = i.projPos.z;

			float fade = saturate(saturate(1000 * (sceneZ - partZ)) + _Fade);
			i.color.a *= fade;

			return i.color;
			}
		ENDCG
		}

	// Pass 1 - Transparent with depth fade and stencil test.
	// Allows drawing once when Ref matches the value provided externally.
	Pass
		{
		// Stencil allows a single write

		Stencil
			{
			Ref [_StencilRef]
			Comp Equal
			Pass IncrWrap
			}

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		struct appdata_t
			{
			float4 vertex : POSITION;
			fixed4 color : COLOR;
			};

		struct v2f
			{
			float4 vertex : SV_POSITION;
			fixed4 color : COLOR;
			float4 projPos : TEXCOORD2;
			};

		v2f vert (appdata_t v)
			{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.projPos = ComputeScreenPos(o.vertex);
			COMPUTE_EYEDEPTH(o.projPos.z);
			o.color = v.color;
			return o;
			}

		UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
		float _Fade;

		fixed4 frag (v2f i) : COLOR
			{
			float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
			float partZ = i.projPos.z;

			float fade = saturate(saturate(1000 * (sceneZ - partZ)) + _Fade);
			i.color.a *= fade;

			return i.color;
			}
		ENDCG
		}

	// Pass 2 - Write value to stencil buffer
	Pass
		{
		ColorMask 0

		Stencil
			{
			Ref [_StencilRef]
			Comp Always
			Pass Replace
			}
		}
	}
}
