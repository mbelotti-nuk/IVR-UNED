using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class colorScale : MonoBehaviour
{
    RawImage m_RawImage;
    //Select a Texture in the Inspector to change to
    RenderTexture outputTexture;
    public ComputeShader shader;
    int kernelHandle;
    int texResolution = 1048;

    void Start()
    {
        //Fetch the RawImage component from the GameObject
        m_RawImage = GetComponent<RawImage>();
        outputTexture = new RenderTexture(texResolution, texResolution, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();
        InitShader();

        m_RawImage.texture = outputTexture;
  

    }

    private void InitShader()
    {
        kernelHandle = shader.FindKernel("CSMain");

        shader.SetTexture(kernelHandle, "Result", outputTexture);

        //shader.SetInt("resolution", texResolution);

        DispatchShader(texResolution / 8, texResolution / 8);
    }

    private void DispatchShader(int x, int y)
    {
        shader.Dispatch(kernelHandle, x, y, 1);
    }
}
