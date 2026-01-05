Shader "Custom/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 10)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // Automatically filled by Unity: (1/width, 1/height, width, height)
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. Sample the center pixel
                fixed4 c = tex2D(_MainTex, IN.texcoord);

                // 2. Clear the interior (like the first image)
                // We use a smoothstep here to ensure the transition from inside to outside is clean
                float mask = smoothstep(0.1, 0.2, c.a);

                float2 uv = IN.texcoord;
                float2 unit = _MainTex_TexelSize.xy * _OutlineWidth;

                // 3. Multi-sampling for smoothness (Gaussian-like Blur)
                // We average the alpha of 8 surrounding points
                fixed totalAlpha = 0;

                // Cardinal directions
                totalAlpha += tex2D(_MainTex, uv + float2(0, unit.y)).a;
                totalAlpha += tex2D(_MainTex, uv - float2(0, unit.y)).a;
                totalAlpha += tex2D(_MainTex, uv + float2(unit.x, 0)).a;
                totalAlpha += tex2D(_MainTex, uv - float2(unit.x, 0)).a;

                // Diagonals (multiplied by 0.707 for distance correction)
                float diag = 0.707;
                totalAlpha += tex2D(_MainTex, uv + float2(unit.x, unit.y) * diag).a;
                totalAlpha += tex2D(_MainTex, uv + float2(-unit.x, unit.y) * diag).a;
                totalAlpha += tex2D(_MainTex, uv + float2(unit.x, -unit.y) * diag).a;
                totalAlpha += tex2D(_MainTex, uv + float2(-unit.x, -unit.y) * diag).a;

                // 4. Calculate final alpha
                // We divide by a value (like 4 or 8) to control thickness/softness
                float outlineAlpha = saturate(totalAlpha / 4.0);

                // 5. Subtract the original shape so only the outer blurred edge remains
                outlineAlpha -= mask;

                // Apply the outline color with the calculated smooth alpha
                fixed4 finalColor = _OutlineColor;
                finalColor.a *= saturate(outlineAlpha);

                return finalColor;
            }
            ENDCG
        }
    }
}