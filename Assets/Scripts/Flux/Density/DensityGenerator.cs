using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DensityGenerator : MonoBehaviour
{
    const int threadGroupSize = 8;
    public ComputeShader densityShader;
    ComputeBuffer xbuffer;
    ComputeBuffer ybuffer;
    ComputeBuffer zbuffer;
    ComputeBuffer bufferFlux;


    public virtual ComputeBuffer Generate(ComputeBuffer pointsBuffer, List<float> xstep, List<float> ystep, List<float> zstep, float[] flux)
    {

        int nVoxX = (xstep.Count - 1); // number of voxels on x axis
        int nVoxY = (ystep.Count - 1); // number of voxels on y axis
        int nVoxZ = (zstep.Count - 1); // number of voxels on z axis
        int numThreadsXAxis = Mathf.CeilToInt( nVoxX / (float)threadGroupSize);
        int numThreadsYAxis = Mathf.CeilToInt( nVoxY / (float)threadGroupSize);
        int numThreadsZAxis = Mathf.CeilToInt( nVoxZ / (float)threadGroupSize);

        // Points buffer is populated inside shader with pos (xyz) + density (w).

        // Set paramaters
        densityShader.SetBuffer(0, "points", pointsBuffer);
        densityShader.SetInt("nx", nVoxX);
        densityShader.SetInt("ny", nVoxY);
        densityShader.SetInt("nz", nVoxZ);

        int stride = 1 * 4; // 1 float (x, y or z) - 4 bytes per float
        xbuffer = new ComputeBuffer(xstep.ToArray().Length, stride);
        xbuffer.SetData(xstep.ToArray());
        densityShader.SetBuffer(0, "xdiv", xbuffer);

        ybuffer = new ComputeBuffer(ystep.ToArray().Length, stride);
        ybuffer.SetData(ystep.ToArray());
        densityShader.SetBuffer(0, "ydiv", ybuffer);

        zbuffer = new ComputeBuffer(zstep.ToArray().Length, stride);
        zbuffer.SetData(zstep.ToArray());
        densityShader.SetBuffer(0, "zdiv", zbuffer);

        bufferFlux = new ComputeBuffer(flux.Length, stride);
        bufferFlux.SetData(flux);
        densityShader.SetBuffer(0, "Flux", bufferFlux);
        // End setting parameters


        // Dispatch shader
        densityShader.Dispatch(0, numThreadsXAxis, numThreadsYAxis, numThreadsZAxis);

        xbuffer.Release();
        ybuffer.Release();
        zbuffer.Release();
        bufferFlux.Release();

        // Return voxel data buffer so it can be used to generate mesh
        return pointsBuffer;
    }
}
