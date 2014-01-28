Shader "AG/Diffuse Specular Normal" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1)
		_SpecColor ("Spec Color", Color) = (1,1,1)
		_Shininess ("Shininess", Range (0.01,100)) = 0.01
		
		_MainTex ("Diffuse (RGB)", 2D) = "white" {}
		_SpecMap ("Specular Map (Grayscale)", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
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
		float3 _Color;
		float _Shininess;
		float _Height;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half3 tex = tex2D(_MainTex, IN.uv_MainTex);
			half3 spc = tex2D(_SpecMap, IN.uv_MainTex);

			o.Albedo = tex.rgb * _Color; 
			o.Gloss = spc.rgb;
			o.Specular = _Shininess * spc.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
		}
	ENDCG
	}

FallBack "Bumped Diffuse"
}