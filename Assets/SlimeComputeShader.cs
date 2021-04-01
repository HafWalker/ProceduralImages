using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Agent {
    public Vector2 position;
    public float angle;
}

public class SlimeComputeShader : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture outRenderTextrure;
    public RenderTexture outRenderProcessedTextrure;

    public int resolution_width = 256;
    public int resolution_height = 256;

    private int oldResWidth;
    private int oldResHeight;

    public int numAgents = 10;
    private Agent[] agentsData;
    public float moveSpeed;
    public float evaporateSpeed = 1f;

    private ComputeBuffer agentsBuffer;

    // Kernels

    private int kernel_CSMain;
    private int kernel_RandomTest;
    private int kernel_Update;
    private int kernel_ProcessTrailMap;

    // Start is called before the first frame update
    void Start()
    {
        kernel_CSMain = computeShader.FindKernel("CSMain");
        kernel_RandomTest = computeShader.FindKernel("RandomTest");
        kernel_Update = computeShader.FindKernel("Update");
        kernel_ProcessTrailMap = computeShader.FindKernel("ProcessTrailMap");

        if (outRenderTextrure == null) {
            outRenderTextrure = new RenderTexture(resolution_width, resolution_height, 24);
            outRenderTextrure.enableRandomWrite = true;
            outRenderTextrure.Create();
        }

        if (outRenderProcessedTextrure == null) {
            outRenderProcessedTextrure = new RenderTexture(resolution_width, resolution_height, 24);
            outRenderProcessedTextrure.enableRandomWrite = true;
            outRenderProcessedTextrure.Create();
        }

        PopulateAgents();

        // General Sets

        oldResWidth = resolution_width;
        oldResHeight = resolution_height;

        computeShader.SetInt("width", resolution_width);
        computeShader.SetInt("height", resolution_height);
        computeShader.SetFloat("evaporateSpeed", evaporateSpeed);

        // Kernel RandomTest // Use thi to test the Random out
        /*
        computeShader.SetTexture(kernel_RandomTest, "RandomTestTexture", outRenderTextrure);
        computeShader.Dispatch(kernel_RandomTest, resolution_width / 8, resolution_height / 8, 1);
        */

        // Kernel Update

        computeShader.SetInt("numAgents", numAgents);
        computeShader.SetFloat("moveSpeed", moveSpeed);
        computeShader.SetTexture(kernel_Update, "TrailMap", outRenderTextrure);
        //computeShader.Dispatch(kernel_Update, resolution_width / 8, resolution_height / 8, 1);
    }

    private void Update() {
        ComputeStepFrame();
        CheResolutionChange();
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        Graphics.Blit(outRenderTextrure, dest);
    }

    private void ComputeStepFrame() {

        int totalSize = (sizeof(float) * 2) + (sizeof(float));
        agentsBuffer = new ComputeBuffer(agentsData.Length, totalSize);
        agentsBuffer.SetData(agentsData);
        computeShader.SetBuffer(kernel_Update, "agents", agentsBuffer);

        computeShader.SetFloat("newRandomAngle", Random.Range(0, 360));
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.Dispatch(kernel_Update, resolution_width / 8, resolution_height / 8, 1);

        //computeShader.SetTexture(kernel_ProcessTrailMap, "ProcessedTrailMap", outRenderProcessedTextrure);
        //computeShader.Dispatch(kernel_ProcessTrailMap, resolution_width / 8, resolution_height / 8, 1);

        agentsBuffer.GetData(agentsData);

        computeShader.SetTexture(kernel_Update, "TrailMap", outRenderTextrure);
        
        agentsBuffer.Dispose();
    }

    public void PopulateAgents() {
        agentsData = new Agent[numAgents];

        for (int i = 0; i < numAgents; i++) {
            Agent agentData = new Agent();
            agentData.position = new Vector2(resolution_width / 2, resolution_height / 2);
            agentData.angle = Random.Range(0,360);
            agentsData[i] = agentData;
        }
    }

    public void CheResolutionChange() {
        if (resolution_height != oldResHeight || resolution_width != oldResWidth) {
            computeShader.SetInt("width", resolution_width);
            computeShader.SetInt("height", resolution_height);
        }
    }
}
