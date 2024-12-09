using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;

public class Waves : MonoBehaviour
{
    MeshFilter waterMeshFilter;

    Mesh waterMesh;

    MeshRenderer waterMeshRenderer;

    public WavesVariables Wvariable;

    public WavesAttribute waves;

    List<Vector3> waterVertices;

    List<int> waterTriangles;

    List<Vector2> waterUVS;

    public float frequency = 1, maxHeight = 16, minHeight = 1;

    private void OnValidate()
    {
        waterMesh = new Mesh();
        waterMeshFilter = GetComponent<MeshFilter>();
        waterMeshFilter.mesh = waterMesh;
        waterMeshRenderer = GetComponent<MeshRenderer>();
        GeneratingPlane();
    }

    private void Update()
    {
        float xStep = Wvariable.Dimension.x / Wvariable.Resolution;
        float zStep = Wvariable.Dimension.y / Wvariable.Resolution;
        float y = 0;
        y = FractalBrownianMotion(xStep, zStep);
        for (int i = 0; i < waves.waves.Count; i++) //soon will find a way to set an array of vector4 to apply in shaders so that y can have max waves
        {
            waterMeshRenderer.material.SetInt("_NumberOfWaves", waves.waves.Count);
            waterMeshRenderer.material.SetVectorArray("_Waves", waves.waves);
        }

    }

    void GeneratingPlane()
    {
        waterVertices = new List<Vector3>();
        float xStep = Wvariable.Dimension.x / Wvariable.Resolution;
        float zStep = Wvariable.Dimension.y / Wvariable.Resolution;

        for (int width = 0; width < Wvariable.Resolution + 1; width++)
        {
            for (int depth = 0; depth < Wvariable.Resolution + 1; depth++)
            {
                float y = 0;
                y = FractalBrownianMotion(width, depth);
                waterVertices.Add(new Vector3(depth * xStep, y, width * zStep));
            }
        }

        waterTriangles = new List<int>();
        for (int r = 0; r < Wvariable.Resolution; r++)
        {
            for (int c = 0; c < Wvariable.Resolution; c++)
            {
                int i = (r * Wvariable.Resolution) + r + c;

                waterTriangles.Add(i);
                waterTriangles.Add(i + (Wvariable.Resolution) + 1);
                waterTriangles.Add(i + (Wvariable.Resolution) + 2);

                waterTriangles.Add(i);
                waterTriangles.Add(i + (Wvariable.Resolution) + 2);
                waterTriangles.Add(i + 1);
            }
        }

        waterUVS = new List<Vector2>();
        for (int x = 0; x <= Wvariable.Resolution; x++)
        {
            for (int z = 0; z <= Wvariable.Resolution; z++)
            {
                var vec = new Vector2((x / Wvariable.UVScale) % 2, (z / Wvariable.UVScale) % 2);
                waterUVS.Add(new Vector2(vec.x <= 1 ? vec.x : 2 - vec.x, vec.y <= 1 ? vec.y : 2 - vec.y));
            }
        }

        waterMesh.Clear();
        waterMesh.vertices = waterVertices.ToArray();
        waterMesh.triangles = waterTriangles.ToArray();
        waterMesh.uv = waterUVS.ToArray();
        waterMesh.RecalculateNormals();
        waterMesh.RecalculateBounds();
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
        // public List<float> directionX, directionY;
        // public List<float> wavelength, steepness;
    }
}
