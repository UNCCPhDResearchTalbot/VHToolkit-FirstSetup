Shader "VH/ICT_EyeShader_with_cubemap" {

Properties {
	_DiffuseIntensity ("Diffuse Intensity", Range (0,1)) = 1.0 // sliders
	_SpecularIntensity ("Specular Intensity", Range (0,1)) = 1.0 // sliders
	_SpecularExponent ("Specular Exponent", Range (0,15)) = 12.0 // sliders
	_IrisIntensity ("Iris Intensity", Range (0,1)) = 1.0 // sliders
	_IrisUVRadius ("Iris UV Radius", Range (0.1,0.9)) = 0.5 // sliders
	_IrisWorldRadius ("Iris World Radius", Range (0.1,0.5)) = 0.3 // sliders
	_IrisWorldDepth ("Iris World Depth", Range (0.01,0.2)) = 0.1 // sliders
	_IOR ("IOR", Range (1,2)) = 1.33 // sliders
	_ReflectionIntensity ("Reflection Intensity", Range (0,10)) = 0 // sliders
	_ReflectionExponent ("Reflection Exponent", Range (1,10)) = 1 // sliders
	_IrisPlane ("Iris Plane", Color) = (0,0,1,0)
	_SSSColor ("SSS Color", Color) = (0,0,0,1)
	_DiffuseAlbedo ("DiffuseAlbedo", 2D) = "white" {}
	_Normal ("Normal", 2D) = "bump" {}
	_Cube ("Reflection Cubemap", Cube) = "_Skybox" { TexGen CubeReflect }
}

SubShader {
Tags { "RenderType" = "Opaque" }

CGPROGRAM
	// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
	#pragma exclude_renderers gles
	#pragma surface surf CustomLighting vertex:vert
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
	
	sampler2D _DiffuseAlbedo;
	sampler2D _Normal;
	samplerCUBE _Cube;
	uniform float _DiffuseIntensity;
	uniform float _SpecularIntensity;
	uniform float _SpecularExponent;
	uniform float _IrisIntensity;
	uniform float _IrisUVRadius;
	uniform float _IrisWorldRadius;
	uniform float _IrisWorldDepth;
	uniform float _IOR;
	uniform float _ReflectionIntensity;
	uniform float _ReflectionExponent;
	uniform float3 _SSSColor;
	uniform float3 _IrisPlane;
	float3 NormalVecD;
	float3 NormalVecS;
	
	struct Input
	{		
		// because all the textures use the same scale and offset's we only need one
		// set of texture coordinates
		float2 uv_DiffuseAlbedo;
		float3 viewDir;
		float3 objectPos;
		float3 objectNormal;
		float4 objectTangent;
		//float3 worldRefl;
		//INTERNAL_DATA
	};
	
	float3 diffuseReflection(float3 N, float3 L, float3 albedoD)
	{
		// [NOTE] omitting fresnel transmission term to help approximate SSS and make up for missing specular environment map
		float d = dot(L, N);
		return pow(saturate((1.0f - d) * _SSSColor + d), _SSSColor * 4.0f + 1.0f) * albedoD;	// [NOTE] spherical cosine lobe helps approximate SSS
	}
	
	float fresnelReflection(float3 N, half3 V)
	{
		float r0 = pow((1 - 1.33) / (1 + 1.33), 2);
		return pow(min(1, 1 - dot(N, V)), 5) * (1 - r0) + r0;
	}

	float specularReflection(float3 N, float3 L, half3 V)
	{
		float3 H = normalize(L + V);
		// Blinn-Phong lobe
		float n = pow(2, _SpecularExponent);
		//float normalization = (n + 2.0f) * (n + 4.0f) * 0.125f / (n + pow(0.5f, n * 0.5f));
		float normalization = n * 0.125f + 1.0f; // [NOTE] while the above normalization is correct, this one is almost the same
		float fresnel = fresnelReflection(N, V);
		return normalization * pow(max(0.0f, dot(N, H)), n) * fresnel * _SpecularIntensity * max(0.0f, dot(N, L));
	}

	half4 LightingCustomLighting (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) 
	{
		float3 diffuseColor = diffuseReflection(NormalVecD, lightDir, s.Albedo);
		float specularColor = specularReflection(NormalVecS, lightDir, viewDir);
		return half4((diffuseColor + specularColor) * _LightColor0.rgb * atten * 2.0f, 1);
	}

	void vert (inout appdata_full v, out Input o)
	{
		o.objectPos = v.vertex.xyz;
		o.objectNormal = v.normal.xyz;
		o.objectTangent = v.tangent;
	}

	// main function
	void surf (Input IN, inout SurfaceOutput Out)
	{
		// compute initial coordinates in iris space
		float2 irisUV = (IN.uv_DiffuseAlbedo * 2 - 1) / _IrisUVRadius;
		float irisR = length(irisUV);
		float irisAlpha = saturate(1.0f - (irisR - 1.0f) * 10.0f);

		// compute tangent <-> object transforms
		float3 binormal = cross(IN.objectNormal, IN.objectTangent.xyz) * IN.objectTangent.w;
		float3x3 object2Tangent = float3x3(IN.objectTangent.xyz, binormal, IN.objectNormal);
		float3x3 tangent2Object = transpose(object2Tangent);

		// perform refraction in object space
		float3 ObjectCameraPos = mul(_World2Object, float4(_WorldSpaceCameraPos, 1)).xyz;
		float3 V = normalize(ObjectCameraPos - IN.objectPos);
		float3 N = mul(tangent2Object, float3(0, 0, 1));

		float3 irisN = _IrisPlane;
		float irisH = (1 - pow(irisR, 2)) * _IrisWorldDepth;	// [FIXME] approximate distance to iris plane
		float3 irisP = -irisN * irisH;
		
		// Snell's law: n1 * sintheta1 = n2 * sintheta2
		float ior = 1.0f / _IOR;
		float cost1 = dot(N, V);
		float sqCost2 = 1.0 - ior*ior*(1.0-cost1*cost1);
		// refracted camera ray direction
		float3 R = -V * ior + N * (ior * cost1 - sqrt(max(0.0, sqCost2)));
		float d = dot(irisP, irisN) / dot(R, irisN);
		float3 RUV = mul(object2Tangent, R * d / _IrisWorldRadius);
		irisUV = irisUV + RUV.xy;
		irisUV /= max(1, length(irisUV));
		irisUV = (irisUV * _IrisUVRadius + 1) / 2;	// convert to texture UV
		float2 eyeUV = lerp(IN.uv_DiffuseAlbedo, irisUV, irisAlpha);

		float3 Normal = UnpackNormal(tex2D(_Normal, eyeUV));
		NormalVecS = normalize(lerp(Normal, float3(0,0,1), irisAlpha));
		NormalVecD = normalize(lerp(float3(0,0,1), Normal, irisAlpha));
		
		float3 ObjectNormalS = mul(tangent2Object, NormalVecS);
		float3 ObjectR = ObjectNormalS * (dot(V, ObjectNormalS) * 2) - V;
		float3 WorldR = mul((float3x3)_Object2World, ObjectR);
		
		// fake HDR fresnel reflection
		float3 reflValue = pow(texCUBE(_Cube, WorldR), 2.2);	// [FIXME] not sure why cube maps don't get gamma corrected by unity
		reflValue = pow(reflValue, _ReflectionExponent) * _ReflectionIntensity;
		Out.Emission = reflValue * fresnelReflection(ObjectNormalS, V);		
		
		Out.Albedo = tex2D(_DiffuseAlbedo, eyeUV).rgb * lerp(_DiffuseIntensity, _IrisIntensity, irisAlpha);
		Out.Normal = NormalVecS;	// [NOTE] putting ANYTHING here tells Unity to work in tangent coordinates, which is nice
	}
ENDCG

} //subshader
Fallback "VertexLit"
}
