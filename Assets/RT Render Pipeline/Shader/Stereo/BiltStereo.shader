Shader "Hidden/Render/CombineTextures" {
    Properties {
        _LeftTex ("Left Texture", 2D) = "white" {}
        _RightTex ("Right Texture", 2D) = "white" {}
    }

    SubShader {
        Pass {
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

            sampler2D _LeftTex;
            sampler2D _RightTex;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.x = v.uv.x*2;
                o.uv.y = v.uv.y;
                return o;
            }

            half4 frag (v2f i) : SV_Target {
                half4 leftColor = tex2D(_LeftTex, i.uv);
                half4 rightColor = tex2D(_RightTex, i.uv - float2(1, 0));
                half4 combinedColor = half4(0, 0, 0, 0);
                if (i.uv.x < 1) {
                    combinedColor = leftColor;
                } else {
                    combinedColor = rightColor;
                }
                return combinedColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}