Shader "Hidden/Render/SplitStereoLeft" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
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

            sampler2D _MainTex;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.x = v.uv.x *0.5f;
                o.uv.y = v.uv.y;
                return o;
            }

            half4 frag (v2f i) : SV_Target {
                float2 uvOffset = float2(0, 0);
                half4 texColor = tex2D(_MainTex, i.uv + uvOffset);
                return texColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
