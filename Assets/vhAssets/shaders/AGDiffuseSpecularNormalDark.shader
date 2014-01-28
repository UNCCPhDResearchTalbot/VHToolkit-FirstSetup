Shader "AG/Diffuse Specular Normal Dark" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1)
		_SpecColor ("Spec Color", Color) = (1,1,1)
		_Shininess ("Shininess", Range (0.01,1)) = 0
		
		_MainTex ("Diffuse (RGB)", 2D) = "white" {}
		_SpecMap ("Specular Map (Grayscale)", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_MultiplyMap ("Decal Map (Uses 2nd UV Set)", 2D) = "white" {}
	}

    SubShader {
		Tags {"Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Opaque"}
		LOD 400
		
		CGPROGRAM
		#pragma surface surf BlinnPhong
		#pragma exclude_renderers flash

		sampler2D _MainTex;
		sampler2D _SpecMap;
		sampler2D _BumpMap;
		sampler2D _MultiplyMap;
		float4 _Color;
		float _Shininess;
		float _Height;

		struct Input {
			float2 uv_MainTex;
			float2 uv2_MultiplyMap;
			float3 viewDir;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 tex = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			
			o.Albedo = tex.rgb ;
			o.Albedo = tex.rgb * tex2D(_MultiplyMap, IN.uv2_MultiplyMap); 
			o.Gloss = _Shininess;
			o.Specular = _Shininess * tex2D(_SpecMap, IN.uv_MainTex).rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
		}
	ENDCG
	}

FallBack "Bumped Diffuse"
}