Shader "VH/ICT_HyrbridNormalShader_v3" {

Properties {
	_DiffuseIntensity ("Diffuse Intensity", Range (0,1)) = 1.0 // sliders
	_SpecularIntensity ("Specular Intensity", Range (0,1)) = 1.0 // sliders
	_SpecularExponent ("Specular Exponent", Range (0,10)) = 5.0 // sliders
	_AsperityIntensity ("Asperity Intensity", Range (0,1)) = 1.0 // sliders
	_AsperityExponent ("Asperity Exponent", Range (1,8)) = 4.0 // sliders
	_SSSColor ("SSS Color", Color) = (0,0,0,1)
	_Albedo ("Albedo", 2D) = "white" {}	// [NOTE] pack spec into alpha
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
	#pragma surface surf CustomLighting
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
	
	sampler2D _Albedo;
	sampler2D _NormalR;
	sampler2D _NormalG;
	sampler2D _NormalB;
	sampler2D _NormalS;
	uniform float _DiffuseIntensity;
	uniform float _SpecularIntensity;
	uniform float _SpecularExponent;
	uniform float _AsperityIntensity;
	uniform float _AsperityExponent;
	uniform float3 _SSSColor;
	float3 NormalRVec;
	float3 NormalGVec;
	float3 NormalBVec;
	float3 NormalSVec;
	
	struct Input
	{		
		// because all the textures use the same scale and offset's we only need one
		// set of texture coordinates
		float2 uv_Albedo;
	};
	
	//! Diffuse reflection using per-color-channel diffuse normal vectors.
	/*!
	  Diffuse reflection for light direction L and normal direction
	  specifed for each of red, green, and blue channels.
	*/
	float3 diffuseReflection(float3 normal_R, float3 normal_G, float3 normal_B, float3 L, half3 V, float3 albedoD)
	{
		// [NOTE] omitting fresnel transmission term to help approximate SSS and make up for missing specular environment map
		float3 dots = float3(dot(normal_R, L), dot(normal_G, L), dot(normal_B, L));
		return pow(saturate((1.0f - dots) * _SSSColor + dots), _SSSColor * 4.0f + 1.0f) * albedoD;	// [NOTE] spherical cosine lobe helps approximate SSS
	}

	//! Specular reflection using specular normal vector.
	/*!
	  Blinn-Phong with energy-preserving modification. specularExponent is one
	  of the material parameters set before export.
	*/
	float3 specularReflection(float3 N, float3 L, half3 V, float3 albedoS)
	{
		float3 H = normalize(L + V);
		// Asperity lobe
		float asperity = 1.125f * max(0.0f, dot(N, H));
		float asperityStrength = pow(min(1, 1 - dot(N, V)), _AsperityExponent) * _AsperityIntensity;
		// Blinn-Phong lobe
		float n = pow(2, _SpecularExponent);
		//float normalization = (n + 2.0f) * (n + 4.0f) * 0.125f / (n + pow(0.5f, n * 0.5f));
		float normalization = n * 0.125f + 1.0f; // [NOTE] while the above normalization is correct, this one is almost the same
		float fresnel = pow(min(1, 1 - dot(N, V)), 5) * 0.92f + 0.08f;
		float blinnPhong = normalization * pow(max(0.0f, dot(N, H)), n);
		float blinnPhongStrength = min(fresnel * _SpecularIntensity, 1 - asperityStrength);
		return (blinnPhong * blinnPhongStrength + asperity * asperityStrength) * max(0.0f, dot(N, L)) * albedoS;
	}

	half4 LightingCustomLighting (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) 
	{
		float3 diffuseColor = diffuseReflection(NormalRVec, NormalGVec, NormalBVec, lightDir, viewDir, s.Albedo * _DiffuseIntensity);
		float3 specularColor = specularReflection(NormalSVec, lightDir, viewDir, s.Specular);
		return half4((diffuseColor + specularColor) * _LightColor0.rgb * atten * 2.0f, 1);	// [NOTE] atten * 2 is the unity standard definition for light intensity attenuation
	}

	// main function
	void surf (Input IN, inout SurfaceOutput Out)
	{
		NormalRVec = UnpackNormal(tex2D(_NormalR, IN.uv_Albedo));
		NormalGVec = UnpackNormal(tex2D(_NormalG, IN.uv_Albedo));
		NormalBVec = UnpackNormal(tex2D(_NormalB, IN.uv_Albedo));
		NormalSVec = UnpackNormal(tex2D(_NormalS, IN.uv_Albedo));
		Out.Normal = NormalSVec;	// [NOTE] putting ANYTHING here tells Unity to work in tangent coordinates, which is nice
		
		float4 albedo = tex2D (_Albedo, IN.uv_Albedo);
		Out.Albedo = albedo.rgb;
		Out.Specular = pow(albedo.a, 2.2f);	// [NOTE] Unity assumes alpha is linear
	}
ENDCG

} //subshader
Fallback "VertexLit"
} 