Shader "domeprojection.com/Correction"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlendTexture ("Texture", 2D) = "white" {}
        _BlackLevelTexture("Texture", 2D) = "black" {}
        _Gamma("Gamma", Float) = 2.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uvDefault : TEXCOORD1;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uvDefault : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _BlendTexture;
            sampler2D _BlackLevelTexture;
            float _Gamma = 2.2;
            float _Params[8] = { 0, 1, 1, 1, 1, 1, 1, 1 };

            float4x4 _WorldToCameraMatrix;
            float4x4 _CameraProjectionMatrix;
            float4x4 _LocalToWorldMatrix;

            float4 _MainTex_ST;

            float2 WorldToScreenPos(float4 pos)
            {
                float2 uv = 0;
                float4 toCam = mul(_WorldToCameraMatrix, pos);
                toCam = mul(_CameraProjectionMatrix, float4(toCam.xyz, 1));
                toCam /= toCam.w;
                uv.x = (toCam.x + 1) * 0.5;
                uv.y = (toCam.y + 1) * 0.5;
                return uv;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = mul(UNITY_MATRIX_P, float4(v.vertex));
                if (_Params[0] > 0.0)
                {
                    o.uv = WorldToScreenPos(mul(_LocalToWorldMatrix, v.color));
                }
                else
                {
                    o.uv = v.uv;
                }
                o.uvDefault = v.uvDefault;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 color4 = tex2D(_MainTex, lerp(i.uvDefault, i.uv, _Params[1]));
                if (color4.a < _Params[4]) discard;
                float3 color = clamp(color4.rgb, 0.0, 1.0);
                float3 blend = lerp(float3(1.0, 1.0, 1.0), tex2D(_BlendTexture, i.uvDefault).rgb, _Params[2]);
                float3 blc   = pow(tex2D(_BlackLevelTexture, i.uvDefault).rgb * _Params[3], float3(_Gamma, _Gamma, _Gamma));
                
                color *= blend;
                color = pow(pow(color, float3(_Gamma, _Gamma, _Gamma)) * (1.0 - blc) + blc, float3(1.0 / _Gamma, 1.0 / _Gamma, 1.0 / _Gamma));

                return float4(color, 1.0);
            }

            ENDCG
        }
    }
}
