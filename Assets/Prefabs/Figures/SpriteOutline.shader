Shader "Custom/SpriteInnerShadow_Smooth"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ShadowColor ("Inner Shadow Color", Color) = (0,0,0,1)
        _ShadowWidth ("Shadow Width", Range(0, 20)) = 1
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
                fixed4 mainCol = tex2D(_MainTex, IN.uv);
                
                // If current pixel is transparent, there's no "inner" to draw on
                if (mainCol.a <= 0.05) discard;

                float2 texelSize = _MainTex_TexelSize.xy;
                float totalAlpha = 0;
                float samples = 0;

                // Increase the 'step' density to remove gaps
                // We sample in a small grid around the pixel
                for (float x = -1; x <= 1; x += 0.5)
                {
                    for (float y = -1; y <= 1; y += 0.5)
                    {
                        // Skip the center pixel
                        if (x == 0 && y == 0) continue;

                        float2 offset = float2(x, y) * _ShadowWidth * texelSize;
                        totalAlpha += tex2D(_MainTex, IN.uv + offset).a;
                        samples++;
                    }
                }

                // Calculate how much "emptiness" is around this opaque pixel
                float averageAlpha = totalAlpha / samples;
                
                // Invert it: 1.0 means it's an edge (lots of transparency nearby)
                // 0.0 means it's deep inside the shape
                float innerShadow = saturate(1.0 - averageAlpha);

                // Make the falloff smoother
                innerShadow = pow(innerShadow, 0.5); 

                fixed4 finalColor = _ShadowColor;
                finalColor.a *= innerShadow * mainCol.a;

                return finalColor;
            }
            ENDCG
        }
    }
}