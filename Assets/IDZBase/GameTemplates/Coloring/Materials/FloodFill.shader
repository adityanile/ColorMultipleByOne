Shader "Template/FloodFill"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.5
        _StartPosition("Start Position", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D      _MainTex;
            float4         _MainTex_ST;
            uniform float4 _MainTex_TexelSize;
            sampler2D      _MaskTex;
            float4         _Color;
            float          _FillAmount;
            float2         _StartPosition;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 mask = tex2D(_MaskTex, i.uv);
                clip(mask.a - 0.5);
                const float2 pos = i.uv - _StartPosition * _MainTex_TexelSize.xy;
                float2       len = length(pos);
                const float  step_val = step(len.x, _FillAmount);
                fixed4       c = tex2D(_MainTex, i.uv);
                if(step_val > 0) c = _Color;
                return c;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}