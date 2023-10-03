using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfValueGenerator : MonoBehaviour
{
    const int threadGroupSize = 8;
    public ComputeShader gradientShader;
    ComputeBuffer xbuffer;
    ComputeBuffer ybuffer;
    ComputeBuffer zbuffer;
    ComputeBuffer xCbuffer;
    ComputeBuffer yCbuffer;
    ComputeBuffer zCbuffer;
    ComputeBuffer bufferFlux;


    public virtual Vector2[] Generate(ComputeBuffer surfbuffer, List<float> xstep, List<float> ystep, List<float> zstep, List<float> xCenter, List<float> yCenter, List<float> zCenter, float[] flux)
    {
        Vector2[] SurfData = new Vector2[flux.Length * 6];

        int nVoxX = (xstep.Count - 1); // number of voxels on x axis
        int nVoxY = (ystep.Count - 1); // number of voxels on y axis
        int nVoxZ = (zstep.Count - 1); // number of voxels on z axis
        int numThreadsXAxis = Mathf.CeilToInt(nVoxX / (float)threadGroupSize);
        int numThreadsYAxis = Mathf.CeilToInt(nVoxY / (float)threadGroupSize);
        int numThreadsZAxis = Mathf.CeilToInt(nVoxZ / (float)threadGroupSize);

        // Points buffer is populated inside shader with pos (xyz) + density (w).

        // Set paramaters
        gradientShader.SetBuffer(0, "surf", surfbuffer);
        gradientShader.SetInt("nx", nVoxX);
        gradientShader.SetInt("ny", nVoxY);
        gradientShader.SetInt("nz", nVoxZ);

        int stride = 1 * 4; // 1 float (x, y or z) - 4 bytes per float
        xbuffer = new ComputeBuffer(xstep.ToArray().Length, stride);
        xbuffer.SetData(xstep.ToArray());
        gradientShader.SetBuffer(0, "xdiv", xbuffer);

        ybuffer = new ComputeBuffer(ystep.ToArray().Length, stride);
        ybuffer.SetData(ystep.ToArray());
        gradientShader.SetBuffer(0, "ydiv", ybuffer);

        zbuffer = new ComputeBuffer(zstep.ToArray().Length, stride);
        zbuffer.SetData(zstep.ToArray());
        gradientShader.SetBuffer(0, "zdiv", zbuffer);
        //
        xCbuffer = new ComputeBuffer(xCenter.ToArray().Length, stride);
        xCbuffer.SetData(xstep.ToArray());
        gradientShader.SetBuffer(0, "xcen", xCbuffer);

        yCbuffer = new ComputeBuffer(yCenter.ToArray().Length, stride);
        yCbuffer.SetData(ystep.ToArray());
        gradientShader.SetBuffer(0, "ycen", yCbuffer);

        zCbuffer = new ComputeBuffer(zCenter.ToArray().Length, stride);
        zCbuffer.SetData(zstep.ToArray());
        gradientShader.SetBuffer(0, "zcen", zCbuffer);
        //
        bufferFlux = new ComputeBuffer(flux.Length, stride);
        bufferFlux.SetData(flux);
        gradientShader.SetBuffer(0, "Flux", bufferFlux);
        // End setting parameters


        // Dispatch shader
        gradientShader.Dispatch(0, numThreadsXAxis, numThreadsYAxis, numThreadsZAxis);
        surfbuffer.GetData(SurfData);

        xbuffer.Release();
        ybuffer.Release();
        zbuffer.Release();
        xCbuffer.Release();
        yCbuffer.Release();
        zCbuffer.Release();
        bufferFlux.Release();

        // Return voxel data buffer so it can be used to generate mesh
        return SurfData;
    }
}
