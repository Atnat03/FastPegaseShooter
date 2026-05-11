#ifndef GETSCREENSHADOW_INCLUDED
#define GETSCREENSHADOW_INCLUDED

SamplerState sampler_point_clamp;

void GetScreenShadow_float(float4 ScreenPos, float2 ShadowFade, float NormalBias, out float Shadow)
{
#if defined(SHADERGRAPH_PREVIEW)
    Shadow = 1.0;
#else
    float2 uv = ScreenPos.xy / ScreenPos.w;

    #if defined(_ScreenSpaceShadowmapTexture_TexelSize)
        float2 texelSize = _ScreenSpaceShadowmapTexture_TexelSize.xy;
    #else
        float2 texelSize = 1.0 / _ScreenParams.xy;
    #endif

    uv = clamp(uv, texelSize * 0.5, 1.0 - texelSize * 0.5);

    // Exclusion de la skybox
    float depth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_point_clamp, uv).r;
    if (depth <= 0.0001)
    {
        Shadow = 1.0;
        return;
    }

    float3 normalWS = SAMPLE_TEXTURE2D_X(_CameraNormalsTexture,
                                          sampler_CameraNormalsTexture,
                                          uv).rgb;
    normalWS = normalWS * 2.0 - 1.0;

    float3 lightDir = _MainLightPosition.xyz;
    float NdotL = dot(normalWS, lightDir) - NormalBias;
    float hardMask = step(0.0, NdotL);
    float softMask = smoothstep(ShadowFade.x, ShadowFade.y, NdotL);
    float shadowMask = hardMask * softMask;

    Shadow = SAMPLE_TEXTURE2D_X(_ScreenSpaceShadowmapTexture,
                                 sampler_point_clamp,
                                 uv).r * shadowMask;
#endif
}

#endif