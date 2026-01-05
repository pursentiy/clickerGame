Shader "Custom/SpriteInnerShadow_Pro_Spiral"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _ShadowColor ("Shadow Color", Color) = (0,0,0,1)
        _ShadowWidth ("Shadow Width", Range(0, 30)) = 2
        _Falloff ("Shadow Softness", Range(0.1, 4)) = 1.0

        [Header(Linear Wave)]
        [Toggle(ENABLE_LINEAR)] _UseWave ("Enable Linear Wave", Float) = 0
        _WaveSpeed ("Speed", Range(0, 10)) = 3
        _WaveFreq ("Frequency", Range(0, 20)) = 10
        _WaveStrength ("Intensity", Range(0, 1)) = 0.5

        [Header(Spiral Wave)]
        [Toggle(ENABLE_SPIRAL)] _UseSpiral ("Enable Spiral Wave", Float) = 0
        _SpiralSpeed ("Spiral Speed", Range(-10, 10)) = 3
        _SpiralArms ("Spiral Arms", Range(1, 10)) = 3
        _SpiralTightness ("Spiral Tightness", Range(0, 20)) = 5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "PreviewType"="Plane" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature ENABLE_LINEAR
            #pragma shader_feature ENABLE_SPIRAL
            #include "UnityCG.cginc"

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _ShadowColor;
            float _ShadowWidth, _Falloff;
            float _WaveSpeed, _WaveFreq, _WaveStrength;
            float _SpiralSpeed, _SpiralArms, _SpiralTightness;

            v2f vert(appdata_base v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                if (col.a <= 0.05) discard;

                // --- 1. RADIAL SAMPLING ---
                float totalAlpha = 0;
                float sampleCount = 0;
                float2 texelSize = _MainTex_TexelSize.xy * _ShadowWidth;

                // Increased density to 0.2 to ensure NO GAPS
                // for (float angle = 0; angle < 6.28; angle += 0.2)
                // {
                //     float2 offset = float2(cos(angle), sin(angle)) * texelSize;
                //     totalAlpha += tex2D(_MainTex, i.uv + offset).a;
                //     sampleCount++; // Added increment fix
                // }

                for(float angle = 0; angle < 6.28; angle += 0.4) {
    float2 offset = float2(cos(angle), sin(angle)) * _MainTex_TexelSize.xy * _ShadowWidth;
    totalAlpha += tex2D(_MainTex, i.uv + offset).a;; // Added increment fix
}

                float avgAlpha = totalAlpha / sampleCount;
                float shadow = saturate(1.0 - avgAlpha);
                shadow = pow(shadow, _Falloff);

                // --- 2. LINEAR WAVE LOGIC ---
                #ifdef ENABLE_LINEAR
                    float lWave = sin((i.uv.y + i.uv.x) * _WaveFreq + (_Time.g * _WaveSpeed));
                    lWave = lerp(1.0 - _WaveStrength, 1.0, (lWave * 0.5 + 0.5));
                    shadow *= lWave;
                #endif

                // --- 3. SPIRAL WAVE LOGIC ---
                #ifdef ENABLE_SPIRAL
                    // Get UV centered at (0,0)
                    float2 centeredUV = i.uv - 0.5;
                    // Calculate angle (0 to 2*PI) and distance from center
                    float dist = length(centeredUV);
                    float anglePos = atan2(centeredUV.y, centeredUV.x);
                    
                    // Spiral formula: Angle + (Distance * Tightness) - Time
                    float sWave = sin(anglePos * _SpiralArms + (dist * _SpiralTightness) - (_Time.g * _SpiralSpeed));
                    sWave = saturate(sWave * 0.5 + 0.5);
                    shadow *= sWave;
                #endif

                fixed4 finalCol = _ShadowColor;
                finalCol.a *= shadow * col.a;

                return finalCol;
            }
            ENDCG
        }
    }
}