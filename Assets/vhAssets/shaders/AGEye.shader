Shader "AG/Eye/Eye"
{
	Properties 
	{
_Diffuse("_Diffuse", 2D) = "black" {}
_DiffuseBrightness("_DiffuseBrightness", Range(0,2) ) = 1
_InnerReflection("_InnerReflection", Range(20,500) ) = 20
_ReflectionMap("_ReflectionMap", Cube) = "black" {}
_ReflectionFalloff("_ReflectionFalloff", Range(0.5,3) ) = 0.5
_ReflectionStr("_ReflectionStr", Range(0,8) ) = 0.5

	}
	
	SubShader 
	{
		Tags
		{
"Queue"="Geometry"
"IgnoreProjector"="False"
"RenderType"="Opaque"

		}

		
Cull Back
ZWrite On
ZTest LEqual
ColorMask RGBA
Fog{
}


		CGPROGRAM
#pragma surface surf BlinnPhongEditor  vertex:vert
#pragma target 3.0


sampler2D _Diffuse;
float _DiffuseBrightness;
float _InnerReflection;
samplerCUBE _ReflectionMap;
float _ReflectionFalloff;
float _ReflectionStr;

			struct EditorSurfaceOutput {
				half3 Albedo;
				half3 Normal;
				half3 Emission;
				half3 Gloss;
				half Specular;
				half Alpha;
				half4 Custom;
			};
			
			inline half4 LightingBlinnPhongEditor_PrePass (EditorSurfaceOutput s, half4 light)
			{
half3 spec = light.a * s.Gloss;
half4 c;
c.rgb = (s.Albedo * light.rgb + light.rgb * spec);
c.a = s.Alpha;
return c;

			}

			inline half4 LightingBlinnPhongEditor (EditorSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
			{
				half3 h = normalize (lightDir + viewDir);
				
				half diff = max (0, dot ( lightDir, s.Normal ));
				
				float nh = max (0, dot (s.Normal, h));
				float spec = pow (nh, s.Specular*128.0);
				
				half4 res;
				res.rgb = _LightColor0.rgb * diff;
				res.w = spec * Luminance (_LightColor0.rgb);
				res *= atten * 2.0;

				return LightingBlinnPhongEditor_PrePass( s, res );
			}
			
			struct Input {
				float2 uv_Diffuse;
float3 viewDir;
float3 sWorldNormal;

			};

			void vert (inout appdata_full v, out Input o) {
float4 VertexOutputMaster0_0_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_1_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_2_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_3_NoInput = float4(0,0,0,0);

o.uv_Diffuse = float2(0, 0);
o.viewDir = float3(0, 0, 0);
o.sWorldNormal = mul((float3x3)_Object2World, SCALED_NORMAL);

			}
			

			void surf (Input IN, inout EditorSurfaceOutput o) {
				o.Normal = float3(0.0,0.0,1.0);
				o.Alpha = 1.0;
				o.Albedo = 0.0;
				o.Emission = 0.0;
				o.Gloss = 0.0;
				o.Specular = 0.0;
				o.Custom = half4(0.0,0.0,0.0,0.0);
				
float4 Tex2D1=tex2D(_Diffuse,(IN.uv_Diffuse.xyxy).xy);
float4 Multiply1=Tex2D1 * _DiffuseBrightness.xxxx;
float4 Clamp1=clamp(Multiply1,float4( 0.0, 0.0, 0.0, 0.0 ),float4( 1.0, 1.0, 1.0, 1.0 ));
float4 Fresnel1_1_NoInput = float4(0,0,1,1);
float4 Fresnel1=(1.0 - dot( normalize( float4( IN.viewDir.x, IN.viewDir.y,IN.viewDir.z,1.0 ).xyz), normalize( Fresnel1_1_NoInput.xyz ) )).xxxx;
float4 Pow1=pow(Fresnel1,_ReflectionFalloff.xxxx);
float4 TexCUBE0=texCUBE(_ReflectionMap,float4( IN.sWorldNormal.x, IN.sWorldNormal.y,IN.sWorldNormal.z,1.0 ));
float4 Multiply0=Pow1 * TexCUBE0;
float4 Multiply2=Multiply0 * _ReflectionStr.xxxx;
float4 Fresnel0=(1.0 - dot( normalize( float4( IN.viewDir.x, IN.viewDir.y,IN.viewDir.z,1.0 ).xyz), normalize( float4( 0.5,0.5,-0.5,0).xyz ) )).xxxx;
float4 Invert0= float4(1.0, 1.0, 1.0, 1.0) - Fresnel0;
float4 Pow0=pow(Invert0,_InnerReflection.xxxx);
float4 Clamp0=clamp(Pow0,float4( 0.0, 0.0, 0.0, 0.0 ),float4( 1.0, 1.0, 1.0, 1.0 ));
float4 Add0=Multiply2 + Clamp0;
float4 Master0_1_NoInput = float4(0,0,1,1);
float4 Master0_3_NoInput = float4(0,0,0,0);
float4 Master0_4_NoInput = float4(0,0,0,0);
float4 Master0_5_NoInput = float4(1,1,1,1);
float4 Master0_7_NoInput = float4(0,0,0,0);
float4 Master0_6_NoInput = float4(1,1,1,1);
o.Albedo = Clamp1;
o.Emission = Add0;

				o.Normal = normalize(o.Normal);
			}
		ENDCG
	}
	Fallback "Diffuse"
}