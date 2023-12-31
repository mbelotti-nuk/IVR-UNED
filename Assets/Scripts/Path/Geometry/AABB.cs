﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{

    public class AABB : Shape {

        public Vector3 Min { get { return min; } }
        public Vector3 Max { get { return max; } }
        public Vector3 Center { get { return (min + max) * 0.5f; } }

        public Vector3 Extent { get { return (max - min); } }

        protected Vector3 min, max;

        public Vector3Int structure { get; set; }

        public AABB(Vector3 min, Vector3 max) : base()
        {
            this.min = Vector3.Min(min, max);
            this.max = Vector3.Max(min, max);
        }

        public override void DrawGizmos(bool intersected)
        {
            Gizmos.color = intersected ? Color.green : Color.white;
            var center = (Min + Max) * 0.5f;
            Gizmos.DrawWireCube(center, Max - Min);
        }

    }

}
