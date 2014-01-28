Shader "AG/Double-Sided/Diffuse Alpha-Test" {
//This shader does NOT calculate lighting information on the back face. As a result, the front
//and back faces will be lit as if they were both facing the front face's direction.

Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
}

SubShader {
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="False" "RenderType"="TransparentCutout"}
	LOD 200
	Cull Back
    
    CGPROGRAM
    #pragma surface surf Lambert alphatest:_Cutoff
    
    sampler2D _MainTex;
    fixed4 _Color;
    
    struct Input {
    	float2 uv_MainTex;
    };
    
    void surf (Input IN, inout SurfaceOutput o) {
    	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
    	o.Albedo = c.rgb;
    	o.Alpha = c.a;
    }
    ENDCG
    
    //Second pass, for the back with proper lighting
    Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
    LOD 200
    Cull Front
    CGPROGRAM
    #pragma surface surf Lambert alphatest:_Cutoff vertex:vert
     
    void vert (inout appdata_full v){
        //Invert normal for proper lighting information
        v.normal = v.normal * -1;
    }
     
    sampler2D _MainTex;
    fixed4 _Color;
    
    struct Input {
     float2 uv_MainTex;
    };
    
    void surf (Input IN, inout SurfaceOutput o) {
     fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
     o.Albedo = c.rgb;
     o.Alpha = c.a;
    }
    ENDCG
     
    
}

Fallback "Transparent/Cutout/VertexLit"
}
