Shader "AG/Eye/Eye Shell"
{
	Properties 
	{
_Distortion("_Distortion", Range(0,0.5) ) = 0.1
_DetailNormal("_DetailNormal", 2D) = "black" {}
_PrimaryHighlight_Pos("_PrimaryHighlight_Pos", Vector) = (0,0,0,0)
_PrimaryHighlight_Falloff("_PrimaryHighlight_Falloff", Range(60,2000) ) = 400
_DiffusedHighlight_Pos("_DiffusedHighlight_Pos", Vector) = (0,0,0,0)
_DiffusedHighlight_Str("_DiffusedHighlight_Str", Range(80,3000) ) = 1650

	}
	
	SubShader 
	{
		Tags
		{
"Queue"="Transparent"
"IgnoreProjector"="False"
"RenderType"="Opaque"

		}
GrabPass { }
		
Cull Back
ZWrite Off
ZTest LEqual
ColorMask RGBA
Fog{
}


		CGPROGRAM
#pragma surface surf BlinnPhongEditor  vertex:vert
#pragma target 3.0


float _Distortion;
sampler2D _DetailNormal;
float4 _PrimaryHighlight_Pos;
float _PrimaryHighlight_Falloff;
float4 _DiffusedHighlight_Pos;
float _DiffusedHighlight_Str;
sampler2D _GrabTexture;

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
				float4 screenPos;
float2 uv_DetailNormal;
float3 sWorldNormal;

			};

			void vert (inout appdata_full v, out Input o) {
float4 VertexOutputMaster0_0_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_1_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_2_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_3_NoInput = float4(0,0,0,0);

o.screenPos = float4(0, 0, 0, 0);
o.uv_DetailNormal = float2(0, 0);
o.sWorldNormal = mul((float3x3)_Object2World, SCALED_NORMAL);

			}
			

			void surf (Input IN, inout EditorSurfaceOutput o) {
				o.Normal = float3(0.0,0.0,1.0);
				o.Alpha = 1.0;
				o.Albedo = 0.0;
				o.Emission = 0.0;
				o.Gloss = 0.0;
				o.Specular = 0.0;
				o.Custom = 0.0;
				
float4 Tex2D1=tex2D(_DetailNormal,(IN.uv_DetailNormal.xyxy).xy);
float4 UnpackNormal0=float4(UnpackNormal(Tex2D1).xyz, 1.0);
float4 Multiply1=UnpackNormal0 * _Distortion.xxxx;
float4 Add0=((IN.screenPos.xy/IN.screenPos.w).xyxy) + Multiply1;
float4 Tex2D0=tex2D(_GrabTexture,Add0.xy);
float4 Fresnel1=(1.0 - dot( normalize( float4( IN.sWorldNormal.x, IN.sWorldNormal.y,IN.sWorldNormal.z,1.0 ).xyz), normalize( _PrimaryHighlight_Pos.xyz ) )).xxxx;
float4 Invert1= float4(1.0, 1.0, 1.0, 1.0) - Fresnel1;
float4 Pow0=pow(Invert1,_PrimaryHighlight_Falloff.xxxx);
float4 Clamp1=clamp(Pow0,float4( 0.0, 0.0, 0.0, 0.0 ),float4( 1.0, 1.0, 1.0, 1.0 ));
float4 Multiply3=UnpackNormal0 * _DiffusedHighlight_Pos;
float4 Fresnel0=(1.0 - dot( normalize( float4( IN.sWorldNormal.x, IN.sWorldNormal.y,IN.sWorldNormal.z,1.0 ).xyz), normalize( Multiply3.xyz ) )).xxxx;
float4 Invert0= float4(1.0, 1.0, 1.0, 1.0) - Fresnel0;
float4 Pow1=pow(Invert0,_DiffusedHighlight_Str.xxxx);
float4 Clamp0=clamp(Pow1,float4( 0.0, 0.0, 0.0, 0.0 ),float4( 1.0, 1.0, 1.0, 1.0 ));
float4 Add2=Clamp1 + Clamp0;
float4 Add1=Tex2D0 + Add2;
float4 Master0_0_NoInput = float4(0,0,0,0);
float4 Master0_1_NoInput = float4(0,0,1,1);
float4 Master0_3_NoInput = float4(0,0,0,0);
float4 Master0_4_NoInput = float4(0,0,0,0);
float4 Master0_5_NoInput = float4(1,1,1,1);
float4 Master0_7_NoInput = float4(0,0,0,0);
float4 Master0_6_NoInput = float4(1,1,1,1);
o.Emission = Add1;

				o.Normal = normalize(o.Normal);
			}
		ENDCG
	}
	Fallback "Diffuse"
}