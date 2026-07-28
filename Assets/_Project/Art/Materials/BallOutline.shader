Shader "MiniGameBall/BallOutline"
{
    Properties
    {
        [MainColor] _BaseColor("Ball Color", Color) = (1, 1, 1, 1)
        _OutlineColor("Outline Color", Color) = (0.12, 0.28, 0.38, 1)
        _OutlineWidth("Outline Width", Range(0.001, 0.2)) = 0.04
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "UniversalMaterialType" = "Unlit"
            "IgnoreProjector" = "True"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "Unlit"

            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            #ifdef UNITY_DOTS_INSTANCING_ENABLED
                UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                    UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
                    UNITY_DOTS_INSTANCED_PROP(float4, _OutlineColor)
                    UNITY_DOTS_INSTANCED_PROP(float, _OutlineWidth)
                UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

                #define BALL_COLOR UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor)
                #define OUTLINE_COLOR UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _OutlineColor)
                #define OUTLINE_WIDTH UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _OutlineWidth)
            #else
                #define BALL_COLOR _BaseColor
                #define OUTLINE_COLOR _OutlineColor
                #define OUTLINE_WIDTH _OutlineWidth
            #endif

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings Vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float outlineWidth = OUTLINE_WIDTH;
                float3 positionOS = input.positionOS.xyz + normalize(input.normalOS) * outlineWidth;

                Varyings output;
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS);
                output.positionHCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half facing = abs(dot(normalize(input.normalWS),
                    GetWorldSpaceNormalizeViewDir(input.positionWS)));
                half originalRadius = 0.5h;
                half expandedRadius = originalRadius + OUTLINE_WIDTH;
                half innerRadius = originalRadius / expandedRadius;
                half outlineThreshold = sqrt(saturate(1.0h - innerRadius * innerRadius));
                return facing < outlineThreshold ? OUTLINE_COLOR : BALL_COLOR;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
