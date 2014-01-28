Shader "VH/ICT_HyrbridNormalShader_v2" {

Properties {
		_DiffuseIntensity ("Diffuse Intensity", Range (0,2)) = 1.0 // sliders
		_SpecularIntensity ("Specular Intensity", Range (0,2)) = 1.0 // sliders
		_SpecularExponent ("Specular Exponent", Range (1,128)) = 16.0 // sliders
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
    	_AlbedoD ("Albedo D", 2D) = "white" {}
		_AlbedoS ("Albedo S", 2D) = "white" {}
		_NormalR ("Normal R", 2D) = "bump" {}
		_NormalG ("Normal G", 2D) = "bump" {}
		_NormalB ("Normal B", 2D) = "bump" {}
		_NormalS ("Normal S", 2D) = "bump" {}
		

}

SubShader {
Tags { "RenderType" = "Opaque" }

CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
	//#pragma surface surf Lambert vertex:VS
	#pragma surface surf CustomLighting vertex:VS
	#pragma target 3.0
	//#pragma debug
	#include "UnityCG.cginc"
	
	//struct SurfaceOutput 
	//{
	//    half3 Albedo;
	//    half3 Normal;
	//    half3 Emission;
	//    half Specular;
	//    half Gloss;
	//    half Alpha;
	//};
	
	sampler2D _AlbedoD;
	sampler2D _AlbedoS;
	sampler2D _NormalR;
	sampler2D _NormalG;
	sampler2D _NormalB;
	sampler2D _NormalS;
	float _DiffuseIntensity;
	float _SpecularIntensity;
	float _SpecularExponent;
	float4x4 TBN;
	float3 NormalRVec;
	float3 NormalGVec;
	float3 NormalBVec;
	float3 NormalSVec;
	
	struct Input
	{
		//float3 worldPos;
		//float3 viewDir;
		
		//float3 normal;
		//float3 tangent;		
		
		// because all the textures use the same scale and offset's we only need one
		// set of texture coordinates
		float2 uv_AlbedoD;
	};
	
	//Input savedInput;
	
	//! Lambertian diffuse reflectance.
	float diffuseLambertian(float3 N, float3 L)
	{
	  return max(0.0f, dot(N, L));
	}
	
	//! Diffuse reflection using per-color-channel diffuse normal vectors.
	/*!
	  Diffuse (lambertian) reflection for light direction L and normal direction
	  specifed for each of red, green, and blue channels.
	*/
	float3 diffuseReflection(float3 normal_R, float3 normal_G, float3 normal_B, float3 L, float3 albedoD)
	{
		return albedoD * float3(diffuseLambertian(normal_R, L),
		                      diffuseLambertian(normal_G, L),
		                      diffuseLambertian(normal_B, L));
	}
	
	//! Blinn-phong specular reflectance, with energy-preserving normalization.
	float specularBlinnPhong(float3 N, float3 H, float exponent)
	{
		float normalization = (exponent + 1.0f) / 6.2831853f;
		return normalization * pow(max(0.0f, dot(N, H)), exponent);
	}
	
	//! Specular reflection using specular normal vector.
	/*!
	  Blinn-Phong with energy-preserving modification. specularExponent is one
	  of the material parameters set before export.
	*/
	float3 specularReflection(float3 normal_S, half3 L, half3 V, float3 albedoS)
	{
		float3 H = normalize(L + V);
		return albedoS * specularBlinnPhong(normal_S, H, _SpecularExponent);
	}

	half4 LightingCustomLighting (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) 
	{
		half3 h = normalize (lightDir + viewDir);
		half diff = max (0, dot (s.Normal, lightDir));
		float nh = max (0, dot (s.Normal, h));
		//float spec = pow (nh, 48.0);
		
		float3 lightDirFloat = float3(lightDir);
		float3 L = mul(TBN, float4(lightDirFloat, 1));
		//lightDir = mul(TBN, lightDirFloat);
		
		half4 c = LightingLambert(s, lightDir, atten);
		float3 colorAsFloat3 = float3(c);
		float3 diffuseColor = diffuseReflection(NormalRVec, NormalGVec, NormalBVec, L, s.Albedo) * colorAsFloat3 * atten;
		
		//return half4(NormalBVec, 1);
		float3 specularColor = specularReflection(NormalSVec, L, viewDir, s.Albedo) * _SpecColor * atten;
		
		float3 holder = _SpecularIntensity * specularColor + _DiffuseIntensity * diffuseColor;
		holder = pow(holder, 1.0f/2.2f);
		
		c = half4(holder, 1.0f);
		return c;
		

		//half3 h = normalize (lightDir + viewDir);
		//half diff = max (0, dot (s.Normal, lightDir));
		
		//float nh = max (0, dot (s.Normal, h));
		//float spec = pow (nh, 48.0);
		
		//half4 c;
		//c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * (atten * 2);
		//c.a = s.Alpha;
		//return c;
	}
		
	
	void VS(inout appdata_full v, out Input IN)
	{
	
		//IN.normal = v.normal;
		//IN.tangent = float3(v.tangent);
		TBN = float4x4(v.tangent, float4(cross(v.normal, v.tangent.xyz), 0), float4(v.normal, 0), float4(0, 0, 0, 1));
		//TBN = float3x3(v.tangent.xyz, float3(cross(v.normal, v.tangent.xyz)), v.normal);
	}
	

	// main function
	void surf (Input IN, inout SurfaceOutput Out)
	{					
		//TBN
		//IN.N = normalize(In.N);
		//IN.tangent -= IN.normal * dot(IN.normal, IN.tangent);
		//IN.tangent = normalize(IN.tangent);
		//IN.B = normalize(cross(In.N, In.T));
		//TBN = float3x3(IN.tangent, cross(IN.normal, IN.tangent), IN.normal);
			
		Out.Albedo = tex2D (_AlbedoD, IN.uv_AlbedoD).rgb;
		//Out.Specular = tex2D (_AlbedoS, IN.uv_AlbedoD).rgb;
		//Out.Normal = UnpackNormal(tex2D (_AlbedoS, IN.uv_AlbedoD));
		
		NormalRVec = UnpackNormal(tex2D(_NormalR, IN.uv_AlbedoD));
		NormalGVec = UnpackNormal(tex2D(_NormalG, IN.uv_AlbedoD));
		NormalBVec = UnpackNormal(tex2D(_NormalB, IN.uv_AlbedoD));
		NormalSVec = UnpackNormal(tex2D(_NormalS, IN.uv_AlbedoD));
	}
ENDCG

} //subshader
Fallback "VertexLit"
} 