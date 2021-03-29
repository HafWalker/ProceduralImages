using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeComputeShader : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture outRenderTextrure;

    // Start is called before the first frame update
    void Start()
    {
        if (outRenderTextrure == null) {
            outRenderTextrure = new RenderTexture(256,256,24);
            outRenderTextrure.enableRandomWrite = true;
            outRenderTextrure.Create();
        }

        computeShader.SetTexture(0, "Result", outRenderTextrure);
        computeShader.Dispatch(0, 256 / 8, 256 / 8, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
