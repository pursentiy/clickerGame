Shader "Custom/SpriteFill_Pro_Spiral_Colored"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _ShadowColor ("Shadow Color", Color) = (0,0,0,1)
        _ShadowWidth ("Shadow Width", Range(0, 30)) = 2
        _Falloff ("Shadow Softness", Range(0.1, 4)) = 1.0

        [Header(Base Settings)]
        _PaleIntensity ("Pale Intensity (0-1)", Range(0, 1)) = 0.5
        _BaseAlpha ("Base Image Alpha", Range(0, 1)) = 1.0

        [Header(Animation Settings)]
        _FillAlpha ("Global Alpha Multiplier", Range(0, 1)) = 1.0
        
        [Header(Linear Wave)]
        [Toggle(ENABLE_LINEAR)] _UseWave ("Enable Linear Wave", Float) = 0
        _LinearColor ("Linear Wave Color", Color) = (1,1,1,1)
        _WaveSpeed ("Speed", Range(0, 10)) = 3
        _WaveFreq ("Frequency", Range(0, 20)) = 10
        _WaveStrength ("Intensity", Range(0, 1)) = 0.5

        [Header(Spiral Wave)]
        [Toggle(ENABLE_SPIRAL)] _UseSpiral ("Enable Spiral Wave", Float) = 0
        _SpiralColor ("Spiral Wave Color", Color) = (1,1,1,1)
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
            fixed4 _ShadowColor, _LinearColor, _SpiralColor;
            float _ShadowWidth, _Falloff, _FillAlpha, _PaleIntensity, _BaseAlpha;
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
                fixed4 mainTex = tex2D(_MainTex, i.uv);
                if (mainTex.a <= 0.05) discard;

                // --- 1. PALE & BASE ALPHA LOGIC ---
                float luminance = dot(mainTex.rgb, float3(0.2126, 0.7152, 0.0722));
                float3 grayscale = float3(luminance, luminance, luminance);
                mainTex.rgb = lerp(mainTex.rgb, grayscale, _PaleIntensity);
                
                // Set the starting alpha based on the property
                float currentAlpha = mainTex.a * _BaseAlpha;

                // --- 2. SHADOW LOGIC ---
                float totalAlpha = 0;
                int sampleCount = 0;
                float2 baseOffset = _MainTex_TexelSize.xy * _ShadowWidth;

                for (float angle = 0; angle < 6.28; angle += 0.4)
                {
                    float2 offset = float2(cos(angle), sin(angle)) * baseOffset;
                    totalAlpha += tex2D(_MainTex, i.uv + offset).a;
                    sampleCount++; 
                }

                float avgAlpha = totalAlpha / max(sampleCount, 1);
                float shadowMask = pow(saturate(1.0 - avgAlpha), _Falloff);

                // --- 3. ANIMATION & COLOR LOGIC ---
                float3 effectColorMix = mainTex.rgb;
                float animationAlphaBonus = 0.0;

                #ifdef ENABLE_LINEAR
                    float lWave = sin((i.uv.y + i.uv.x) * _WaveFreq + (_Time.g * _WaveSpeed));
                    float lIntensity = lWave * 0.5 + 0.5;
                    
                    effectColorMix = lerp(effectColorMix, _LinearColor.rgb, lIntensity * _WaveStrength);
                    // Additive alpha logic: waves can make the image more opaque
                    animationAlphaBonus = max(animationAlphaBonus, lIntensity * _WaveStrength);
                #endif

                #ifdef ENABLE_SPIRAL
                    float2 centeredUV = i.uv - 0.5;
                    float dist = length(centeredUV);
                    float anglePos = atan2(centeredUV.y, centeredUV.x);
                    
                    float sWave = sin(anglePos * _SpiralArms + (dist * _SpiralTightness) - (_Time.g * _SpiralSpeed));
                    float sIntensity = saturate(sWave * 0.5 + 0.5);
                    
                    effectColorMix = lerp(effectColorMix, _SpiralColor.rgb, sIntensity);
                    // Additive alpha logic
                    animationAlphaBonus = max(animationAlphaBonus, sIntensity);
                #endif

                // Combine base alpha with the animation "glow"
                // This ensures the image is visible where the base alpha says OR where waves are
                float finalAlpha = saturate(currentAlpha + animationAlphaBonus);

                // --- 4. FINAL COMPOSITION ---
                fixed4 finalCol;
                finalCol.rgb = lerp(effectColorMix, _ShadowColor.rgb, shadowMask);
                finalCol.a = finalAlpha * _FillAlpha;

                return finalCol;
            }
            ENDCG
        }
    }
}