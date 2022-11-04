Shader "Unlit/fade"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_FadeSizePX ("Fade Size PX", Float) = 0.25
		_FadeSizePY ("Fade Size PY", Float) = 0.25
		_FadeSizeNX ("Fade Size NX", Float) = 0.25
		_FadeSizeNY ("Fade Size NY", Float) = 0.25
		_DebugDraw ("Debug Mode", Int) = 0
		_DebugMag ("Debug Magnitude", Float) = 0.25
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

			float _FadeSizePX;
			float _FadeSizePY;
			float _FadeSizeNX;
			float _FadeSizeNY;
			int _DebugDraw;
			float _DebugMag;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float brightnessX = clamp(min(i.uv.x / _FadeSizeNX, (1 - i.uv.x) / _FadeSizePX), 0.0, 1.0);
				float brightnessY = clamp(min(i.uv.y / _FadeSizeNY, (1 - i.uv.y) / _FadeSizePY), 0.0, 1.0);
				float brightness = brightnessX * brightnessY;

				if (_DebugDraw != 0) {
					float debugVal = min(brightnessX, brightnessY);
					if (debugVal <= _DebugMag) {
						return fixed4(0, 1, 0, 1);
					}
					else if (debugVal >= (0.5 - _DebugMag / 2.0) && debugVal <= (0.5 + _DebugMag / 2.0)) {
						return fixed4(1, 0, 0, 1);
					}
					else if (brightness >= (1 - _DebugMag) && debugVal < 1) {
						return fixed4(0, 0, 1, 1);
					}
				}
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * brightness;
				//brightness = _FadeSizeNX;
				//col = fixed4(brightness, brightness, brightness, 1.0);
				// apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
