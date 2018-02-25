using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utilities
{
    public struct Point
    {
        public Vector3 position;
        public Quaternion rotation;
        public float vValue;

        public Point(Vector3 position, Quaternion rotation, float vCoordinate)
        {
            this.position = position;
            this.rotation = rotation;
            this.vValue = vCoordinate;
        }

        public Vector3 LocalToWorld (Vector3 point)
        {
            return position + rotation * point;
        }

        public Vector3 WorldToLocal(Vector3 point)
        {
            return Quaternion.Inverse(rotation) * (point - position);
        }

        public Vector3 LocalToWorldDirection(Vector3 direction)
        {
            return rotation * direction;
        }
    }
}
