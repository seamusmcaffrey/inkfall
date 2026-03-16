using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates chrome dart body (tapered barrel + fletching fins) and tip meshes.
/// </summary>
public static class DartMeshGenerator
{
    private static Mesh _bodyMesh;
    private static Mesh _tipMesh;

    private const int Segments = 18;
    private const int FinCount = 4;
    private const float BodyLength = 0.54f;
    private const float BodyRadiusFront = 0.045f;
    private const float BodyRadiusBack = 0.065f;
    private const float BarrelRadius = 0.08f;
    private const float BarrelLength = 0.04f;
    private const float TipRadius = 0.08f;
    private const float TipLength = 0.18f;
    private const float FinHeight = 0.12f;
    private const float FinLength = 0.16f;
    private const float FinThickness = 0.006f;

    public static Mesh GetBodyMesh()
    {
        if (_bodyMesh == null)
        {
            _bodyMesh = GenerateBody();
            _bodyMesh.name = "DartBody";
        }

        return _bodyMesh;
    }

    public static Mesh GetTipMesh()
    {
        if (_tipMesh == null)
        {
            _tipMesh = GenerateCone(TipRadius, TipLength, Segments);
            _tipMesh.name = "DartTip";
        }

        return _tipMesh;
    }

    private static Mesh GenerateBody()
    {
        var verts = new List<Vector3>();
        var tris = new List<int>();
        var uvs = new List<Vector2>();

        float halfBody = BodyLength * 0.5f;
        float barrelX = -halfBody;
        float backX = barrelX + BarrelLength;
        float frontX = halfBody;

        GenerateTaperedCylinder(verts, tris, uvs, backX, frontX,
            BodyRadiusBack, BodyRadiusFront);

        int barrelStart = verts.Count;
        GenerateRing(verts, tris, uvs, barrelX, backX,
            BarrelRadius, BodyRadiusBack, barrelStart);

        GenerateFletchingFins(verts, tris, barrelX);

        var mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        return mesh;
    }

    private static void GenerateTaperedCylinder(
        List<Vector3> verts, List<int> tris, List<Vector2> uvs,
        float xBack, float xFront, float radiusBack, float radiusFront)
    {
        for (int side = 0; side < 2; side++)
        {
            float x = side == 0 ? xBack : xFront;
            float r = side == 0 ? radiusBack : radiusFront;
            for (int s = 0; s < Segments; s++)
            {
                float angle = s / (float)Segments * Mathf.PI * 2f;
                verts.Add(new Vector3(x, Mathf.Cos(angle) * r, Mathf.Sin(angle) * r));
                uvs.Add(new Vector2(side, s / (float)Segments));
            }
        }

        for (int s = 0; s < Segments; s++)
        {
            int next = (s + 1) % Segments;
            int a = s;
            int b = next;
            int c = Segments + s;
            int d = Segments + next;
            tris.Add(a); tris.Add(c); tris.Add(d);
            tris.Add(a); tris.Add(d); tris.Add(b);
        }
    }

    private static void GenerateRing(
        List<Vector3> verts, List<int> tris, List<Vector2> uvs,
        float xLeft, float xRight, float outerRadius, float innerRadius, int baseIdx)
    {
        for (int side = 0; side < 2; side++)
        {
            float x = side == 0 ? xLeft : xRight;
            float r = side == 0 ? outerRadius : innerRadius;
            for (int s = 0; s < Segments; s++)
            {
                float angle = s / (float)Segments * Mathf.PI * 2f;
                verts.Add(new Vector3(x, Mathf.Cos(angle) * r, Mathf.Sin(angle) * r));
                uvs.Add(new Vector2(0.5f, s / (float)Segments));
            }
        }

        for (int s = 0; s < Segments; s++)
        {
            int next = (s + 1) % Segments;
            int a = baseIdx + s;
            int b = baseIdx + next;
            int c = baseIdx + Segments + s;
            int d = baseIdx + Segments + next;
            tris.Add(a); tris.Add(c); tris.Add(d);
            tris.Add(a); tris.Add(d); tris.Add(b);
        }
    }

    private static void GenerateFletchingFins(
        List<Vector3> verts, List<int> tris, float backX)
    {
        float finBack = backX;
        float finFront = backX + FinLength;

        for (int fin = 0; fin < FinCount; fin++)
        {
            float angle = fin / (float)FinCount * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            Vector3 innerBack = new Vector3(finBack, cos * BodyRadiusBack, sin * BodyRadiusBack);
            Vector3 innerFront = new Vector3(finFront, cos * BodyRadiusFront, sin * BodyRadiusFront);
            Vector3 outerBack = new Vector3(finBack, cos * (BodyRadiusBack + FinHeight),
                sin * (BodyRadiusBack + FinHeight));

            Vector3 offset = new Vector3(0f, -sin * FinThickness, cos * FinThickness);

            int idx = verts.Count;

            verts.Add(innerBack - offset);
            verts.Add(innerFront - offset);
            verts.Add(outerBack - offset);
            verts.Add(innerBack + offset);
            verts.Add(innerFront + offset);
            verts.Add(outerBack + offset);

            tris.Add(idx);     tris.Add(idx + 2); tris.Add(idx + 1);
            tris.Add(idx + 3); tris.Add(idx + 4); tris.Add(idx + 5);
        }
    }

    private static Mesh GenerateCone(float radius, float length, int segments)
    {
        var verts = new List<Vector3>();
        var uvs = new List<Vector2>();
        var tris = new List<int>();

        verts.Add(new Vector3(length * 0.5f, 0f, 0f));
        uvs.Add(new Vector2(0.5f, 1f));

        for (int s = 0; s < segments; s++)
        {
            float angle = s / (float)segments * Mathf.PI * 2f;
            verts.Add(new Vector3(-length * 0.5f, Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius));
            uvs.Add(new Vector2(s / (float)segments, 0f));
        }

        for (int s = 1; s <= segments; s++)
        {
            int next = s == segments ? 1 : s + 1;
            tris.Add(0); tris.Add(s); tris.Add(next);
        }

        var mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        return mesh;
    }
}
