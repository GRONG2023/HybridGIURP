using HTraceWSGI.Scripts.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update

    public ComputeShader computeShader;

    private void OnEnable()
    {
        Shader.EnableKeyword("DeltaForce");
    }

    private void OnDisable()
    {
        Shader.DisableKeyword("DeltaForce");
    }
    void Start()
    {
        ComputeShader computeShader2 = HExtensions.LoadComputeShader("HDepthPyramid");
        if (computeShader == null)
        {
            Debug.LogError("computeShader == null");
        }
        if (computeShader2 == null)
        {
            Debug.LogError("computeShader2 == null");
        }
        else
        {
            Debug.LogError("computeShader2 £¡£¡£¡£¡£¡£¡£¡£¡£¡£¡£¡£¡£¡£¡£¡£¡£¡£¡");
        }
    }

    // Update is called once per frame
    void Update()
    {
        Shader.EnableKeyword("DeltaForce");
    }
}
