using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel<Ttype>
{
    #region Fields
    Vector3 center;
    Vector3 dimensions;
    Ttype flux;
    List<float> Verteces;
    #endregion

    #region Constructor
    public Voxel(Vector3 center, Vector3 dimensions)
    {
        this.dimensions = dimensions;
        this.center = center;
    }
    #endregion

    #region Properties
    public Ttype Flux
    {
        get{ return flux; }
        set{ flux = value; }
    }

    public Vector3 Center
        { get{ return center; } }
    public Vector3 Dimensions
        { get{ return dimensions; } }


    // an image of the index of the verteces can be found at
    // http://paulbourke.net/geometry/polygonise/
    public List<float> SetVertexFlux
    {
        get { return Verteces; }
        set { Verteces = value; }
    }

    #endregion

}
