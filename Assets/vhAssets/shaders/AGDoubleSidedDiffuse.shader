Shader "AG/Double-Sided/Diffuse" {
//This shader does NOT calculate lighting information on the back face. As a result, the front
//and back faces will be lit as if they were both facing the front face's direction.

Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}

SubShader {
	Tags {"Queue"="Geometry" "IgnoreProjector"="False" "RenderType"="Opaque"}
	LOD 200
	Cull Off

CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;
fixed4 _Color;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	o.Albedo = c.rgb;
}
ENDCG
}

Fallback "Diffuse"
}
