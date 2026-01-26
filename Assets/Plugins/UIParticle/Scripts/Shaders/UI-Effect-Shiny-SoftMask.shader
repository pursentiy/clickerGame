Shader "UI/Hidden/UI-Effect-Shiny-SoftMask"
{
    Properties
    {
        [PerRendererData] _MainTex ("Main Texture", 2D) = "white" {}
        _SoftMask("Mask", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        _ParamTex ("Parameter Texture", 2D) = "white" {}
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
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #pragma multi_compile __ UNITY_UI_ALPHACLIP
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            // Подключаем только базовый эффект блеска
            #define UI_SHINY 1
            #include "UI-Effect.cginc" 

            // Описываем структуры, которые раньше были в скрытых инклудах
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Структура v2f теперь включает eParam для Shiny эффекта
            struct v2f_custom
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4 eParam    : TEXCOORD3; // Параметры для ApplyShinyEffect
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _SoftMask;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            // Вершинный шейдер
            v2f_custom vert(appdata_t v)
            {
                v2f_custom o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                
                // Инициализация параметров блеска (из UI-Effect.cginc)
                // Если eParam не инициализировать, блеск не появится
                #ifdef UI_SHINY
                o.eParam = float4(v.texcoord, 0, 0); // Базовая передача координат
                #endif

                return o;
            }

            fixed4 frag(v2f_custom IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                
                // Клиппинг стандартного UI Unity
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

                // Применяем эффект блеска
                color = ApplyShinyEffect(color, IN.eParam);
                
                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif
                
                // Накладываем SoftMask
                half4 mask = tex2D(_SoftMask, IN.texcoord);
                color.a *= mask.a;

                return color;
            }
        ENDCG
        }
    }
}