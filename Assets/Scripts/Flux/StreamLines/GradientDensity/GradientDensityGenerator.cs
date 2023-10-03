using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradientDensityGenerator : MonoBehaviour
{
    const int threadGroupSize = 8;
    public ComputeShader gradientDensityShader;
    ComputeBuffer xbuffer;
    ComputeBuffer ybuffer;
    ComputeBuffer zbuffer;
    ComputeBuffer bufferGrad;


    public virtual ComputeBuffer Generate(ComputeBuffer pointsBuffer, List<float> xstep, List<float> ystep, List<float> zstep, Vector3[] gradient)
    {

        int nVoxX = (xstep.Count - 1); // number of voxels on x axis
        int nVoxY = (ystep.Count - 1); // number of voxels on y axis
        int nVoxZ = (zstep.Count - 1); // number of voxels on z axis
        int numThreadsXAxis = Mathf.CeilToInt(nVoxX / (float)threadGroupSize);
        int numThreadsYAxis = Mathf.CeilToInt(nVoxY / (float)threadGroupSize);
        int numThreadsZAxis = Mathf.CeilToInt(nVoxZ / (float)threadGroupSize);


        // Set paramaters
        gradientDensityShader.SetBuffer(0, "points", pointsBuffer);
        gradientDensityShader.SetInt("nx", nVoxX);
        gradientDensityShader.SetInt("ny", nVoxY);
        gradientDensityShader.SetInt("nz", nVoxZ);

        int stride = 1 * 4; // 1 float (x, y or z) - 4 bytes per float
        xbuffer = new ComputeBuffer(xstep.ToArray().Length, stride);
        xbuffer.SetData(xstep.ToArray());
        gradientDensityShader.SetBuffer(0, "xdiv", xbuffer);

        ybuffer = new ComputeBuffer(ystep.ToArray().Length, stride);
        ybuffer.SetData(ystep.ToArray());
        gradientDensityShader.SetBuffer(0, "ydiv", ybuffer);

        zbuffer = new ComputeBuffer(zstep.ToArray().Length, stride);
        zbuffer.SetData(zstep.ToArray());
        gradientDensityShader.SetBuffer(0, "zdiv", zbuffer);

        bufferGrad = new ComputeBuffer(gradient.Length, stride*3);
        bufferGrad.SetData(gradient);
        gradientDensityShader.SetBuffer(0, "Grad", bufferGrad);
        // End setting parameters


        // Dispatch shader
        gradientDensityShader.Dispatch(0, numThreadsXAxis, numThreadsYAxis, numThreadsZAxis);

        xbuffer.Release();
        ybuffer.Release();
        zbuffer.Release();
        bufferGrad.Release();

        // Return voxel data buffer so it can be used to generate mesh
        return pointsBuffer;
    }
}
