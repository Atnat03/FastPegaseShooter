#ifndef GETSCREENSHADOW_INCLUDED
#define GETSCREENSHADOW_INCLUDED

void GetScreenShadow_float(float4 ScreenPos, float2 ShadowFade, float NormalBias, out float Shadow)
{
    #if defined(SHADERGRAPH_PREVIEW)
    Shadow = 1.0;
    #else

    float2 uv = ScreenPos.xy / ScreenPos.w;
    float2 texelSize = 1.0 / _ScreenParams.xy;
    uv = clamp(uv, texelSize * 0.5, 1.0 - texelSize * 0.5);

    float depth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_LinearClamp, uv).r;
    if (depth <= 0.0001)
    {
        Shadow = 1.0;
        return;
    }

    float3 normalWS = SAMPLE_TEXTURE2D_X(_CameraNormalsTexture, sampler_LinearClamp, uv).rgb;
    normalWS = normalize(normalWS * 2.0 - 1.0);

    // Récupération manuelle de la direction de la lumière principale
    // sans passer par GetMainLight() qui nécessite RealtimeLights.hlsl
    float3 lightDir = normalize(_MainLightPosition.xyz);
    float NdotL = dot(normalWS, lightDir) - NormalBias;

    float shadowMask = smoothstep(ShadowFade.x, ShadowFade.y, NdotL);

    float shadowTex = SAMPLE_TEXTURE2D_X(_ScreenSpaceShadowmapTexture, sampler_LinearClamp, uv).r;

    Shadow = shadowTex * shadowMask;
    #endif
}
#endif