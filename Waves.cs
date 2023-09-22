using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class Waves : MonoBehaviour
{
    MeshFilter meshFilter;

    Mesh mesh;

    public WavesVariables Wvariable;

    List<Vector3> vertices;

    List<int> triangles;

    List<Vector2> uvs;

    public float frequency = 1, maxHeight = 16, minHeight = 1;
    public Wave[] wave;
    public int numerator0, numerator1;
    public float w, ai, speed, esculator, xOffset, zOffset;
    public float a, b;
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        GeneratingPlane();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }


    void GeneratingPlane()
    {
        vertices = new List<Vector3>();
        float xStep = Wvariable.Dimension.x / Wvariable.Resolution;
        float zStep = Wvariable.Dimension.y / Wvariable.Resolution;

        for (int z = 0; z < Wvariable.Resolution + 1; z++)
        {
            for (int x = 0; x < Wvariable.Resolution + 1; x++)
            {
                var y = 0f;

                //To get waves for the water use the below
                var noise = 0f;//to storing the noise value
                float K = 2 * math.PI / w;//angular frequency
                for (int i = 0; i < Wvariable.Octaves; i++)
                {
                    noise = FractalBrownianMotion(x * xOffset + Time.time, z * zOffset + Time.time) * esculator;//noise value to see details in the water waves 
                    float Yx = w * ai * Mathf.Sin(x * w * math.PI + Time.time * K) + Mathf.Sin(x * w * math.PI + Time.time * K) * noise,
                        Yz = w * ai * Mathf.Sin(z * w * math.PI + Time.time * K) - Mathf.Sin(z * w * math.PI + Time.time * K) * noise;
                    float X = a + (math.exp(K * z) / K) * math.sin(K * (a + speed * Time.time));
                    float Z = b - (math.exp(K * z) / K) * math.cos(K * (a + speed * Time.time));

                    Vector2 Y0 = new Vector2(X, Z) * noise * frequency, Y1 = new Vector2(X, Z) * noise * frequency,
                        Y2 = new Vector2(X, Z) * noise * frequency, Y3 = new Vector2(X, Z) * noise * frequency;
                    Vector2 Y = Y0 + Y1 + Y2 + Y3;
                    float3 gridPoint = new float3(x, Y.magnitude, z);
                    float3 tangent = new float3(1, 0, 0);
                    float3 binormal = new float3(0, 0, 1);
                    float3 p = gridPoint;
                    for (int o = 0; o < wave.Length; o++)
                    {
                        p += GerstnerWave(wave[o].WaveA, gridPoint, tangent, binormal) * noise;
                        p += GerstnerWave(wave[o].WaveB, gridPoint, tangent, binormal) * noise;
                        p += GerstnerWave(wave[o].WaveC, gridPoint, tangent, binormal) * noise;
                    }
                    y += Y.y;
                }
                
                vertices.Add(new Vector3(x, y, z));
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
        elevation = math.clamp(elevation, -maxHeight, minHeight);
        return elevation;
    }

    float3 GerstnerWave(float4 wave, float3 p, float3 tangent, float3 binormal)
    {
        float steepness = wave.z;
        float wavelength = wave.w;
        float k = 2 * math.PI / wavelength;
        float c = math.sqrt(9.8f / k);
        float2 d = math.normalize(wave.xy);
        float f = k * (math.dot(d, p.xz) - c * Time.time);
        float a = steepness / k;

        tangent += new float3(-d.x * d.y * (steepness * math.sin(f)),
            d.y * (steepness * math.cos(f)),
            -d.y * d.y * (steepness * math.sin(f)));

        return new float3(d.x * (a * math.cos(f)), a * math.sin(f), d.y * (a * math.cos(f)));
    }

    [System.Serializable]
    public struct WavesVariables
    {
        public Vector2 Dimension;
        [Range(10, 300)] public int Resolution;
        public int Octaves;
        public float UVScale;
        public float lacuranity, persistence;
    }

    [System.Serializable]
    public struct Wave
    {
        public float4 WaveA, WaveB, WaveC;
    }
}
