Shader "Custom/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ShadowColor ("Inner Shadow Color", Color) = (0,0,0,1)
        _ShadowWidth ("Shadow Width", Range(0, 10)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _ShadowColor;
            float _ShadowWidth;

            v2f vert(appdata_t IN) {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. Sample the center pixel alpha
                fixed4 mainCol = tex2D(_MainTex, IN.uv);
                float centerAlpha = mainCol.a;

                // 2. If the pixel is already transparent, don't draw anything
                if (centerAlpha <= 0.01) discard;

                float2 unit = _MainTex_TexelSize.xy * _ShadowWidth;
                
                // 3. Multi-sampling neighbors
                float avgAlpha = 0;
                avgAlpha += tex2D(_MainTex, IN.uv + float2(0, unit.y)).a;
                avgAlpha += tex2D(_MainTex, IN.uv - float2(0, unit.y)).a;
                avgAlpha += tex2D(_MainTex, IN.uv + float2(unit.x, 0)).a;
                avgAlpha += tex2D(_MainTex, IN.uv - float2(unit.x, 0)).a;

                // Diagonals for smoothness
                float diag = 0.707;
                avgAlpha += tex2D(_MainTex, IN.uv + float2(unit.x, unit.y) * diag).a;
                avgAlpha += tex2D(_MainTex, IN.uv + float2(-unit.x, unit.y) * diag).a;
                avgAlpha += tex2D(_MainTex, IN.uv + float2(unit.x, -unit.y) * diag).a;
                avgAlpha += tex2D(_MainTex, IN.uv + float2(-unit.x, -unit.y) * diag).a;

                // 4. Calculate Inner Shadow
                // If avgAlpha is 8, it's deep inside. If it's less than 8, it's near the edge.
                float innerShadow = 1.0 - (avgAlpha / 8.0);
                
                // Smooth out the shadow
                innerShadow = smoothstep(0, 1, innerShadow);

                // 5. Final Color
                // We keep the shadow color, but mask it by the original shape's alpha
                fixed4 finalColor = _ShadowColor;
                finalColor.a *= innerShadow * centerAlpha;

                return finalColor;
            }
            ENDCG
        }
    }
}