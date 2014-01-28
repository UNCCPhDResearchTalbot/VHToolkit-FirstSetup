Shader "AG/Effect/Saturation" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
    Saturation ("Saturation", Range (-5,5)) = 0
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
uniform sampler2D _RampTex;
uniform half _RampOffset;
float Saturation;

fixed4 frag (v2f_img i) : COLOR
{
    fixed3 grayscaleCoefficient = {0.3, 0.59, 0.11};
	fixed4 original = tex2D(_MainTex, i.uv);
	fixed4 output = tex2D(_RampTex, i.uv);
    output.rgb = lerp(original.rgb, dot(original.rgb, grayscaleCoefficient), -1*Saturation);
	output.a = original.a;
	return output;
}
ENDCG

	}
}

Fallback off

}