Shader "Custom/GLGraphics/Standard"
{
Properties
	{
	_StencilRef ("StencilRef", Integer) = 1
	[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Integer) = 2
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

	// Pass 0 - regular transparent pass with vertex colors
	Pass
		{
		BindChannels
			{
			Bind "vertex", vertex
			Bind "color", color
			}
		}

	// Pass 1 - transparent pass with vertex colors and stencil test
	Pass
		{
		Stencil
			{
			Ref [_StencilRef]
			Comp Equal
			Pass IncrWrap
			}
		BindChannels
			{
			Bind "vertex", vertex
			Bind "color", color
			}
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
