using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates the stylized balloon mesh used across the wall.
/// </summary>
public static class BalloonMeshGenerator
{
    private static Mesh _sharedMesh;

    public static Mesh GetSharedMesh()
    {
        if (_sharedMesh == null)
        {
            _sharedMesh = Generate();
            _sharedMesh.name = "ProceduralBalloon";
        }

        return _sharedMesh;
    }

    private static Mesh Generate()
    {
        const int segments = 96;
        const int rings = 48;
        const float bodyHeight = 1.0f;
        const float bodyWidth = 0.52f;
        const float tieRadius = 0.06f;
        const float tieLength = 0.12f;

        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();

        vertices.Add(new Vector3(0f, bodyHeight * 0.5f, 0f));
        uvs.Add(new Vector2(0.5f, 1f));

        for (int ring = 1; ring <= rings; ring++)
        {
            float v = ring / (float)(rings + 1);
            float polar = Mathf.Lerp(0.02f, Mathf.PI - 0.08f, v);
            float radius = Mathf.Sin(polar) * bodyWidth;
            float y = Mathf.Cos(polar) * bodyHeight * 0.5f;
            radius *= Mathf.Lerp(0.94f, 1.06f, Mathf.SmoothStep(0f, 1f, 1f - Mathf.Abs(v - 0.5f) * 2f));
            if (v > 0.85f)
            {
                radius *= Mathf.Lerp(1f, 0.25f, Mathf.InverseLerp(0.85f, 1f, v));
            }

            for (int segment = 0; segment < segments; segment++)
            {
                float t = segment / (float)segments * Mathf.PI * 2f;
                vertices.Add(new Vector3(Mathf.Cos(t) * radius, y, Mathf.Sin(t) * radius));
                uvs.Add(new Vector2(segment / (float)segments, 1f - v));
            }
        }

        int bottomRingStart = vertices.Count;
        for (int segment = 0; segment < segments; segment++)
        {
            float t = segment / (float)segments * Mathf.PI * 2f;
            vertices.Add(new Vector3(Mathf.Cos(t) * tieRadius, -bodyHeight * 0.46f, Mathf.Sin(t) * tieRadius));
            uvs.Add(new Vector2(segment / (float)segments, 0.08f));
        }

        int tipIndex = vertices.Count;
        vertices.Add(new Vector3(0f, -bodyHeight * 0.46f - tieLength, 0f));
        uvs.Add(new Vector2(0.5f, 0f));

        for (int segment = 0; segment < segments; segment++)
        {
            int next = segment == segments - 1 ? 0 : segment + 1;
            triangles.Add(0);
            triangles.Add(1 + next);
            triangles.Add(1 + segment);
        }

        for (int ring = 0; ring < rings - 1; ring++)
        {
            int currentStart = 1 + ring * segments;
            int nextStart = currentStart + segments;
            ConnectRingStrip(triangles, currentStart, nextStart, segments);
        }

        int lastSphereRingStart = 1 + (rings - 1) * segments;
        ConnectRingStrip(triangles, lastSphereRingStart, bottomRingStart, segments);

        for (int segment = 0; segment < segments; segment++)
        {
            int next = segment == segments - 1 ? 0 : segment + 1;
            triangles.Add(bottomRingStart + segment);
            triangles.Add(bottomRingStart + next);
            triangles.Add(tipIndex);
        }

        var mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static void ConnectRingStrip(List<int> triangles, int currentStart, int nextStart, int segments)
    {
        for (int segment = 0; segment < segments; segment++)
        {
            int next = segment == segments - 1 ? 0 : segment + 1;
            triangles.Add(currentStart + segment);
            triangles.Add(nextStart + segment);
            triangles.Add(nextStart + next);

            triangles.Add(currentStart + segment);
            triangles.Add(nextStart + next);
            triangles.Add(currentStart + next);
        }
    }
}
