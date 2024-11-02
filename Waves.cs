using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;

public class Waves : MonoBehaviour
{
    MeshFilter meshFilter;

    Mesh mesh;

    MeshRenderer meshRenderer;

    public WavesVariables Wvariable;

    public WavesAttribute waves;

    List<Vector3> vertices;

    List<int> triangles;

    List<Vector2> uvs;

    public float frequency = 1, maxHeight = 16, minHeight = 1;

    private void OnValidate()
    {
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        meshRenderer = GetComponent<MeshRenderer>();
        GeneratingPlane();
    }

    private void Update()
    {
        float xStep = Wvariable.Dimension.x / Wvariable.Resolution;
        float zStep = Wvariable.Dimension.y / Wvariable.Resolution;
        float y = 0;
        y = FractalBrownianMotion(xStep, zStep);
        // for (int i = 0; i < waves.waves.Count; i++) soon will find a way to set an array of vector4 to apply in shaders so that y can have max waves
        // {
        //     meshRenderer.material.SetVectorArray("_WaveA", waves.waves);
        // }

    }

    void GeneratingPlane()
    {
        vertices = new List<Vector3>();
        float xStep = Wvariable.Dimension.x / Wvariable.Resolution;
        float zStep = Wvariable.Dimension.y / Wvariable.Resolution;

        for (int width = 0; width < Wvariable.Resolution + 1; width++)
        {
            for (int depth = 0; depth < Wvariable.Resolution + 1; depth++)
            {
                float y = 0;
                y = FractalBrownianMotion(width, depth);
                vertices.Add(new Vector3(depth * xStep, y, width * zStep));
            }
        }

        triangles = new List<int>();
        for (int r = 0; r < Wvariable.Resolution; r++)
        {
            for (int c = 0; c < Wvariable.Resolution; c++)
            {
                int i = (r * Wvariable.Resolution) + r + c;

                triangles.Add(i);
                triangles.Add(i + (Wvariable.Resolution) + 1);
                triangles.Add(i + (Wvariable.Resolution) + 2);

                triangles.Add(i);
                triangles.Add(i + (Wvariable.Resolution) + 2);
                triangles.Add(i + 1);
            }
        }

        uvs = new List<Vector2>();
        for (int x = 0; x <= Wvariable.Resolution; x++)
        {
            for (int z = 0; z <= Wvariable.Resolution; z++)
            {
                var vec = new Vector2((x / Wvariable.UVScale) % 2, (z / Wvariable.UVScale) % 2);
                uvs.Add(new Vector2(vec.x <= 1 ? vec.x : 2 - vec.x, vec.y <= 1 ? vec.y : 2 - vec.y));
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    //void FFT()
    //{
    //    var noise = Mathf.PerlinNoise(x, z);
    //    var waves = 0f;
    //    float t = speed * (2 / w);
    //    for (int i = 0; i < octaves; i++)
    //    {
    //        waves += w * ai * noise * Mathf.Pow(Mathf.Cos(x * w + Time.time * t), 2) + Mathf.Pow(Mathf.Sin(x * w + Time.time * t), 2);
    //    }
    //}

    public float FractalBrownianMotion(float x, float y)
    {
        float amplitude = maxHeight;
        float elevation = 0;
        var t_frequency = frequency;
        var t_amplitude = amplitude;
        for (int o = 0; o < Wvariable.Octaves; o++)
        {
            var sampleX = x * t_frequency;
            var sampleZ = y * t_frequency;
            elevation += Mathf.PerlinNoise(sampleX * t_frequency, sampleZ * t_frequency) * t_amplitude;
            t_frequency *= Wvariable.lacuranity;
            t_amplitude *= Wvariable.persistence;
        }
        elevation *= math.clamp(elevation, -maxHeight, minHeight);
        return elevation;
    }

    [System.Serializable]
    public struct WavesVariables
    {
        public Vector2 Dimension;
        [Range(10, 255)] public int Resolution;
        public int Octaves;
        public float UVScale;
        public float lacuranity, persistence;
    }

    [System.Serializable]
    public struct WavesAttribute
    {
        public List<Vector4> waves;
    }
}
