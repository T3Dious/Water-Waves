Shader "Custom/WaterWaves"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        CGPROGRAM


            #pragma surface surf Standard fullforwardshadows vertex:vert 
		    #pragma target 3.0


            sampler2D _MainTex;
            struct Input {
                float2 uv_MainTex;
            };
    
            half _Glossiness;
            half _Metallic;
            fixed4 _Color;
			int _NumberOfWaves;
            float4 _Waves[1000];
			float3 ps[1000];

		// float random (float2 st) {
		// 	return fract(sin(dot(st.xy,
		// 						 float2(12.9898,78.233)))*
		// 		43758.5453123);
		// }

		float3 GerstnerWave(float4 wave, float3 p, inout float3 tangent, inout float3 binormal)
		{
			float steepness = wave.z;
			float wavelength = wave.w;
			float k = 2 * UNITY_PI / wavelength;
			float c = sqrt(9.8 / k);
			float2 d = normalize(wave.xz  * wave.xy);
			float f = k * (dot(d, p.xz) - c * _Time.y);
			float a = steepness / k;

			tangent += float3(
				-d.x * d.x * (steepness * sin(f)),
				d.x * (steepness * cos(f)),
				-d.x * d.y * (steepness * sin(f))
			);
			binormal += float3(
				-d.x * d.y * (steepness * sin(f)),
				d.y * (steepness * cos(f)),
				-d.y * d.y * (steepness * sin(f))
			);
			return float3(
				d.x * (a * cos(f)),
				a * sin(f),
				d.y * (a * cos(f))
			);
		}

		void vert(inout appdata_full vertexData)
		{
			float3 gridPoint = vertexData.vertex.xyz;
			float3 tangent = 0;
			float3 binormal = 0;
			float3 p = gridPoint;
			for(int i = 0; i < _NumberOfWaves; i++)
			{
				p += GerstnerWave(_Waves[i], gridPoint, tangent, binormal);
			}
			float3 normal = normalize(cross(binormal, tangent));
			vertexData.vertex.xyz = p;
			vertexData.normal = normal;
		}


        void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
            ENDCG
            
    }

    FallBack "Diffuse"
}
