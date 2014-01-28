Shader "AG/Effect/Brightness" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
    Brightness ("Brightness", Range (-5,5)) = 0
}

SubShader {
	Pass {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }
				
CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest 
#include "UnityCG.cginc"

uniform sampler2D _MainTex;
float Brightness;

fixed4 frag (v2f_img i) : COLOR
{
	fixed4 original = tex2D(_MainTex, i.uv);
	fixed4 output = tex2D(_MainTex, i.uv);
    output.rgb = original.rgb + (original.rgb * Brightness);
	output.a = original.a;
	return output;
}
ENDCG

	}
}

Fallback off

}