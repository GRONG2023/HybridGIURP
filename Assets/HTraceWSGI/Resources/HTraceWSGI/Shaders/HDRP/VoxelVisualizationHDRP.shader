Shader "Hidden/HTraceWSGI/VoxelVisualizationHDRP"
{
    HLSLINCLUDE

    #pragma vertex FullScreenVert
    #pragma fragment FullScreenFrag

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

    float4x4 _DebugCameraFrustum;
    float4 _DebugCameraFrustumArray[8];

    struct Attributes2
    {
        uint vertexID : SV_VertexID;
    };

    struct Varyings2
    {
        float4 positionCS : SV_POSITION;
        float3 ray         : TEXCOORD0;
    };

    Varyings2 FullScreenVert(Attributes2 input)
    {
        Varyings2 output;

        // URP 全屏三角形顶点
        float4 positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.positionCS = positionCS;

        // 将坐标映射到 0~1
        float2 uv = positionCS.xy * 0.5 + 0.5;

        // 根据 UV 计算索引
        int index = (uv.x / 2.0f) + uv.y;

        output.ray = _DebugCameraFrustumArray[index].xyz;

        return output;
    }

    float4 FullScreenFrag(Varyings2 varyings) : SV_Target
    {
        return float4(varyings.ray, 1);
    }

    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" }
        Pass
        {
            Name "URP Fullscreen Pass"

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
            #pragma vertex FullScreenVert
            #pragma fragment FullScreenFrag
            ENDHLSL
        }
    }
    Fallback Off
}
