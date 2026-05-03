Shader "domeprojection.com/ProjectionMapping"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4x4 _MyWorldToCamera;
            float4x4 _MyCameraProjection;
            float4x4 _MyLocalToWorld;

            fixed2 WorldToScreenPos(fixed4 pos) {
                fixed2 uv = 0;
                fixed4 toCam = mul(_MyWorldToCamera, pos);
                toCam = mul(_MyCameraProjection, fixed4(toCam.xyz, 1));
                toCam /= toCam.w;
                uv.x = (toCam.x + 1) * 0.5;
                uv.y = (toCam.y + 1) * 0.5;
                return uv;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = WorldToScreenPos(mul(_MyLocalToWorld, v.vertex));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
