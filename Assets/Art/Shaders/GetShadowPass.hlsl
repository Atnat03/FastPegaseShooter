#ifndef SHADOW_UTILS_INCLUDED
#define SHADOW_UTILS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

void GetMainLightShadow_float(float3 PositionWS, out float Shadow)
{
    float4 shadowCoord = TransformWorldToShadowCoord(PositionWS);
    Shadow = MainLightRealtimeShadow(shadowCoord);
}

#endif