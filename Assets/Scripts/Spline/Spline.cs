using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utilities
{
    public static class Spline
    {
        public static float[] GenerateSamples(Vector3[] points)
        {
            Vector3 prevPoint = points[0];
            Vector3 pt;
            float total = 0;

            List<float> samples = new List<float>(10) { 0 };
            float step = 1.0f / 10.0f;
            for (float f = step; f < 1.0f; f += step)
            {
                pt = GetPoint(points, f);
                total += (pt - prevPoint).magnitude;
                samples.Add(total);

                prevPoint = pt;
            }

            pt = GetPoint(points, 1);
            samples.Add(total + (pt - prevPoint).magnitude);

            return samples.ToArray();
        }
        
        public static Vector3 GetPoint (Vector3[] points, float t)
        {
            float omt = 1f - t;
            float omt2 = omt * omt;
            float t2 = t * t;
            return points[0] * (omt2 * omt) + points[1] * (3f * omt2 * t ) + points[2] * (3f * omt * t2) + points[3] * (t2 * t);
        }

        public static Vector3 GetTangent (Vector3[] points, float t)
        {
            float omt = 1f - t;
            float omt2 = omt * omt;
            float t2 = t * t;
            Vector3 tangent = points[0] * (-omt2) + points[1] * (3 * omt2 - 2 * omt) + points[2] * (-3 * t2 + 2 * t) + points[3] * t2;
            return tangent.normalized;
        }

        public static Vector3 GetNormal (Vector3[] points, float t, Vector3 up)
        {
            Vector3 tangent = GetTangent(points, t);
            Vector3 binormal = Vector3.Cross(up, tangent).normalized;
            return Vector3.Cross(tangent, binormal);
        }

        public static Vector2 CalculateLineNormal(Vector2 a, Vector2 b)
        {
            return new Vector2(-(b.y - a.y), b.x - a.x);
        }

        public static Quaternion GetOrientation (Vector3[] points, float t, Vector3 up)
        {
            return Quaternion.LookRotation(GetTangent(points, t), GetNormal(points, t, up));
        }

        public static void Extrude(Mesh mesh, Shape shape, Point[] path)
        {
            int shapeVertexCount = shape.vertices.Length;
            int segmentCount = path.Length - 1;
            int edgeLoopCount = path.Length;
            int vertexCount = shapeVertexCount * edgeLoopCount;
            int triangleCount = shape.lines.Length * segmentCount * 2;
            int triangleIndexCount = triangleCount * 3;

            int[] triangleIndices = new int[triangleIndexCount];
            Vector3[] vertices = new Vector3[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            Vector2[] uvValues = new Vector2[vertexCount];

            for (int i = 0; i < path.Length; ++i)
            {
                int offset = i * shapeVertexCount;
                for (int j = 0; j < shapeVertexCount; ++j)
                {
                    int id = offset + j;
                    vertices[id] = path[i].LocalToWorld(shape.vertices[j]);
                    normals[id] = path[i].LocalToWorldDirection(shape.normals[j]);
                    uvValues[id] = new Vector2(shape.uValues[j], path[i].vValue);
                }
            }

            int ti = 0;
            for (int i = 0; i < segmentCount; i++)
            {
                int offset = i * shapeVertexCount;
                for (int l = 0; l < shape.lines.Length; l += 2)
                {
                    int a = offset + shape.lines[l];
                    int b = offset + shape.lines[l] + shapeVertexCount;
                    int c = offset + shape.lines[l + 1] + shapeVertexCount;
                    int d = offset + shape.lines[l + 1];
                    triangleIndices[ti++] = a;
                    triangleIndices[ti++] = b;
                    triangleIndices[ti++] = c;
                    triangleIndices[ti++] = c;
                    triangleIndices[ti++] = d;
                    triangleIndices[ti++] = a;
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangleIndices;
            mesh.normals = normals;
            mesh.uv = uvValues;
        }
    }
}
