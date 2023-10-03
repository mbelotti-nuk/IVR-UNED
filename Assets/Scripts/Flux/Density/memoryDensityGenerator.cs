using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class memoryDensityGenerator : MonoBehaviour
{
    const int threadGroupSize = 8;
    public ComputeShader memoryDensityShader;
    ComputeBuffer xbuffer;
    ComputeBuffer ybuffer;
    ComputeBuffer zbuffer;
    ComputeBuffer bufferFlux;


    public virtual void Generate( ref ComputeBuffer pointsBuffer, ref ComputeBuffer valueBuffer, List<float> xstep, List<float> ystep, List<float> zstep, float[] flux)
    {

        int nVoxX = (xstep.Count - 1); // number of voxels on x axis
        int nVoxY = (ystep.Count - 1); // number of voxels on y axis
        int nVoxZ = (zstep.Count - 1); // number of voxels on z axis
        int numThreadsXAxis = Mathf.CeilToInt(nVoxX / (float)threadGroupSize);
        int numThreadsYAxis = Mathf.CeilToInt(nVoxY / (float)threadGroupSize);
        int numThreadsZAxis = Mathf.CeilToInt(nVoxZ / (float)threadGroupSize);

        // Points buffer is populated inside shader with pos (xyz) + density (w).

        // Set paramaters
        memoryDensityShader.SetBuffer(0, "points", pointsBuffer);
        memoryDensityShader.SetBuffer(0, "value", valueBuffer);
        memoryDensityShader.SetInt("nx", nVoxX);
        memoryDensityShader.SetInt("ny", nVoxY);
        memoryDensityShader.SetInt("nz", nVoxZ);

        int stride = 1 * 4; // 1 float (x, y or z) - 4 bytes per float
        xbuffer = new ComputeBuffer(xstep.ToArray().Length, stride);
        xbuffer.SetData(xstep.ToArray());
        memoryDensityShader.SetBuffer(0, "xdiv", xbuffer);

        ybuffer = new ComputeBuffer(ystep.ToArray().Length, stride);
        ybuffer.SetData(ystep.ToArray());
        memoryDensityShader.SetBuffer(0, "ydiv", ybuffer);

        zbuffer = new ComputeBuffer(zstep.ToArray().Length, stride);
        zbuffer.SetData(zstep.ToArray());
        memoryDensityShader.SetBuffer(0, "zdiv", zbuffer);

        bufferFlux = new ComputeBuffer(flux.Length, stride);
        bufferFlux.SetData(flux);
        memoryDensityShader.SetBuffer(0, "Flux", bufferFlux);
        // End setting parameters


        // Dispatch shader
        memoryDensityShader.Dispatch(0, numThreadsXAxis, numThreadsYAxis, numThreadsZAxis);

        xbuffer.Release();
        ybuffer.Release();
        zbuffer.Release();
        bufferFlux.Release();
;
    }
}
