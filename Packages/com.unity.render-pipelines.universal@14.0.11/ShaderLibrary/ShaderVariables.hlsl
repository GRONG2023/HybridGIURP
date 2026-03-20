// UNITY_SHADER_NO_UPGRADE

#ifndef UNITY_SHADER_VARIABLES_INCLUDED
#define UNITY_SHADER_VARIABLES_INCLUDED

float GetCurrentExposureMultiplier()
{
    return 0.00651;
    //return LOAD_TEXTURE2D(_ExposureTexture, int2(0, 0)).x;
}

float GetInverseCurrentExposureMultiplier()
{
    float exposure = GetCurrentExposureMultiplier();
    return rcp(exposure + (exposure == 0.0)); // zero-div guard
}


float GetPreviousExposureMultiplier()
{
    return 0.00651;
    //return LOAD_TEXTURE2D(_PrevExposureTexture, int2(0, 0)).x;
    // _ProbeExposureScale is a scale used to perform range compression to avoid saturation of the content of the probes. It is 1.0 if we are not rendering probes.
    //return LOAD_TEXTURE2D(_PrevExposureTexture, int2(0, 0)).x * _ProbeExposureScale;

}

float GetInversePreviousExposureMultiplier()
{
    float exposure = GetPreviousExposureMultiplier();
    return rcp(exposure + (exposure == 0.0)); // zero-div guard
}

#endif // UNITY_SHADER_VARIABLES_INCLUDED
