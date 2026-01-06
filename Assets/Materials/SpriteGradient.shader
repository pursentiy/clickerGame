Shader "Custom/SpriteGradient"
{
Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _TopColor ("Top Color", Color) = (1,1,1,1)
        _BottomColor ("Bottom Color", Color) = (0,0,0,1)
        _Pivot ("Gradient Position", Range(0, 1)) = 0.5
        _Opacity ("Opacity", Range(0,1)) = 1.0
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

        Cull Off Lighting Off ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _TopColor;
            fixed4 _BottomColor;
            float _Pivot;
            float _Opacity;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, IN.texcoord);
                
                // Logic for Position Shifting:
                // We use smoothstep to remap the UV.y based on our Pivot.
                // This creates a sharper or softer transition at a specific point.
                float gradPoint = smoothstep(0, _Pivot * 2, IN.texcoord.y);
                
                // If you want a harder "edge" shift, use this line instead:
                // float gradPoint = saturate((IN.texcoord.y - _Pivot) / (1.0 - 0.95) + 0.5);

                fixed4 gradColor = lerp(_BottomColor, _TopColor, gradPoint);
                
                fixed4 res = texColor * gradColor * IN.color;
                res.a *= _Opacity;
                res.rgb *= res.a;
                return res;
            }
            ENDCG
        }
    }
}