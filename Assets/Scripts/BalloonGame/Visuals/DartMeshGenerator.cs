using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates the chrome dart body and tip meshes.
/// </summary>
public static class DartMeshGenerator
{
    private static Mesh _bodyMesh;
    private static Mesh _tipMesh;

    public static Mesh GetBodyMesh()
    {
        if (_bodyMesh == null)
        {
            _bodyMesh = GenerateCylinder(0.06f, 0.54f, 18);
            _bodyMesh.name = "DartBody";
        }

        return _bodyMesh;
    }

    public static Mesh GetTipMesh()
    {
        if (_tipMesh == null)
        {
            _tipMesh = GenerateCone(0.08f, 0.18f, 18);
            _tipMesh.name = "DartTip";
        }

        return _tipMesh;
    }

    private static Mesh GenerateCylinder(float radius, float length, int segments)
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        for (int side = 0; side < 2; side++)
        {
            float x = side == 0 ? -length * 0.5f : length * 0.5f;
            for (int segment = 0; segment < segments; segment++)
            {
                float angle = segment / (float)segments * Mathf.PI * 2f;
                vertices.Add(new Vector3(x, Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
                uvs.Add(new Vector2(side, segment / (float)segments));
            }
        }

        for (int segment = 0; segment < segments; segment++)
        {
            int next = segment == segments - 1 ? 0 : segment + 1;
            int a = segment;
            int b = next;
            int c = segments + segment;
            int d = segments + next;
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(d);
            triangles.Add(a);
            triangles.Add(d);
            triangles.Add(b);
        }

        Mesh mesh = new();
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        return mesh;
    }

    private static Mesh GenerateCone(float radius, float length, int segments)
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        vertices.Add(new Vector3(length * 0.5f, 0f, 0f));

        for (int segment = 0; segment < segments; segment++)
        {
            float angle = segment / (float)segments * Mathf.PI * 2f;
            vertices.Add(new Vector3(-length * 0.5f, Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
        }

        for (int segment = 1; segment <= segments; segment++)
        {
            int next = segment == segments ? 1 : segment + 1;
            triangles.Add(0);
            triangles.Add(segment);
            triangles.Add(next);
        }

        Mesh mesh = new();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        return mesh;
    }
}
