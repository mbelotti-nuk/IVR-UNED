using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

namespace Geometry
{
    public class Grid 
    {

        List<float> xstep;
        List<float[]> ystep;
        List<float> zstep;

        int xDiv;
        int yDiv;
        int zDiv;

        float xMin, yMin, zMin; 
        float xMax, yMax, zMax; 

        ///public AABB[] cubeMesh;
        public AABB[] navigationAABB { get; set; }

        public Grid()
        {


            xstep = new List<float>();
            ystep = new List<float[]>();
            zstep = new List<float>();


        }

        public AABB[] buildGrid(float dim, Vector3 minExt, Vector3 maxExt, float[] validHeights)
        {
            //float dim = 0.2f;
            xMin = minExt.x; zMin = maxExt.x;
            xMax = maxExt.x; zMax = maxExt.z;


            float deltaX = maxExt.x - minExt.x,
                    deltaZ = maxExt.z - minExt.z;

            xDiv = (int)Math.Ceiling(deltaX / dim); zDiv = (int)Math.Ceiling(deltaZ / dim); yDiv = validHeights.Length;

            xstep = new List<float>();
            ystep = new List<float[]>();
            zstep = new List<float>();

            xstep.Capacity = xDiv + 1;
            ystep.Capacity = 2 * yDiv;
            zstep.Capacity = zDiv + 1;


            AABB[] cubeMesh = new AABB[xDiv * yDiv * zDiv];

            int index = 0;


            #region Make steps
            xstep.Add(xMin);
            for (int i = 1; i <= xDiv; i++) { xstep.Add(minExt.x + i * (deltaX / xDiv)); }


            for (int j = 0; j < validHeights.Length; j++) { ystep.Add(new float[] { validHeights[j] - dim, validHeights[j] + dim }); }


            zstep.Add(zMin);
            for (int k = 1; k <= zDiv; k++) { zstep.Add(minExt.z + k * (deltaZ / zDiv)); }


            #endregion

            for (int i = 0; i < xDiv; i++)
            {
                for (int k = 0; k < zDiv; k++)
                {
                    for (int j = 0; j < validHeights.Length; j++)
                    {
                        //float xmin = minExt.x + i * (deltaX / xDiv),
                        //        xmax = minExt.x + (i + 1) * (deltaX / xDiv);

                        //float zmin = minExt.z + k * (deltaZ / zDiv),
                        //        zmax = minExt.z + (k + 1) * (deltaZ / zDiv);

                        float xmin = minExt.x + i * dim,
                                xmax = minExt.x + (i + 1) * dim;

                        float zmin = minExt.z + k * dim,
                                zmax = minExt.z + (k + 1) * dim;


                        float ymin = validHeights[j] - dim, ymax = validHeights[j] + dim;

                        AABB voxel = new AABB(new Vector3(xmin, ymin, zmin), new Vector3(xmax, ymax, zmax));
                        voxel.structure = new Vector3Int(i, j, k);
                        cubeMesh[index] = voxel;
                        index++;
                    }
                }
            }
            return cubeMesh;

        }

        public int IndexOf(float item, int axis)
        {
            // DEFINE AXIS
            // RETURN IN CASE OUTSIDE MESHBOUNDS

            List<float> step = new List<float>();
            if (axis == 1)
            {
                step = xstep;
                if (item > xMax)
                {
                    return -1;
                }
            }
            else if (axis == 2)
            {

                    return lookIntoHeights(item);
                
            }
            else if (axis == 3)
            {
                step = zstep;
                if (item > zMax)
                {
                    return -1;
                }
            }
            //

            int lowerBound = 0;
            int upperBound = step.Count - 1;
            int location = -1;

            while ((location == -1) && (lowerBound <= upperBound))
            {
                // find the middle
                // int always convert to lower integer if float
                int middleLocation = lowerBound + (upperBound - lowerBound) / 2;
                float leftSideValue = step[middleLocation];
                float rightSideValue = step[middleLocation + 1];


                // check for match
                if ((item >= leftSideValue) && (item <= rightSideValue))
                {
                    location = middleLocation;
                }
                else
                {
                    // split data set to search appropriate side
                    if (item <= leftSideValue)
                    {
                        upperBound = middleLocation - 1;
                    }
                    else  //  which means (item > rightSideValue)
                    {
                        lowerBound = middleLocation + 1;
                    }
                }
            }
            return location;
        }


        int lookIntoHeights(float item)
        {
            for (int j = 0; j < ystep.Count; j++)
            {
                float[] bounds = ystep[j];

                if (bounds[0] > bounds[1]) { float temp = bounds[1]; bounds[1] = bounds[0]; bounds[0] = temp; }

                if(item > bounds[0] && item < bounds[1] )
                {
                    return j;
                } 
            }
            return -1;
        }

    }
}
