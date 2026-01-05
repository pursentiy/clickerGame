Shader "Custom/SpriteInnerShadow_Pro"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _ShadowColor ("Shadow Color", Color) = (0,0,0,1)
        _ShadowWidth ("Shadow Width", Range(0, 30)) = 2
        _Falloff ("Shadow Softness", Range(0.1, 4)) = 1.0

        [Header(Wave Animation)]
        [Toggle] _UseWave ("Enable Wave", Float) = 0
        _WaveSpeed ("Wave Speed", Range(0, 10)) = 3
        _WaveFreq ("Wave Frequency", Range(0, 20)) = 10
        _WaveStrength ("Wave Intensity", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "RenderType"="Transparent" "PreviewType"="Plane"
        }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _USEWAVE_ON
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _ShadowColor;
            float _ShadowWidth;
            float _Falloff;

            float _WaveSpeed;
            float _WaveFreq;
            float _WaveStrength;

            v2f vert(appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                if (col.a <= 0.05) discard;

                // --- 1. RADIAL SAMPLING (Fixes the gaps) ---
                // We use a polar coordinate approach for higher sample density
                float totalAlpha = 0;
                float sampleCount = 0;
                float2 texelSize = _MainTex_TexelSize.xy * _ShadowWidth;

                // Change 'angle += 0.4' to a smaller number like 0.2 for even more density
                for (float angle = 0; angle < 6.28; angle += 0.4)
                {
                    float2 offset = float2(cos(angle), sin(angle)) * _MainTex_TexelSize.xy * _ShadowWidth;
                    totalAlpha += tex2D(_MainTex, i.uv + offset).a;
                }

                float avgAlpha = totalAlpha / sampleCount;
                float shadow = saturate(1.0 - avgAlpha);

                // Control falloff smoothness
                shadow = pow(shadow, _Falloff);

                // --- 2. WAVE LOGIC ---
                #ifdef _USEWAVE_ON
                    // We use the UV coordinates and Time to create a moving wave
                    float wave = sin((i.uv.y + i.uv.x) * _WaveFreq + (_Time.g * _WaveSpeed));
                    // Map wave from (-1, 1) to (0.5, 1.0) so it pulsates rather than disappearing
                    wave = lerp(1.0 - _WaveStrength, 1.0, (wave * 0.5 + 0.5));
                    shadow *= wave;
                #endif

                fixed4 finalCol = _ShadowColor;
                finalCol.a *= shadow * col.a;

                return finalCol;
            }
            ENDCG
        }
    }
}