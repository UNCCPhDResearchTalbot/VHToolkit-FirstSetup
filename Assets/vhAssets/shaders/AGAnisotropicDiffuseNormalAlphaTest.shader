/* --------------------------------------------------
Description
This shader gives an anisotropic highlight, similar to the highlights found on brushed metal or hair. 
The highlight can be blended from anisotropic to blinn based on the blue channel of the specular map.
Supports diffuse, normal, specular and gloss shading with alphatested transparency. Gloss and 
specular values also apply to the anisotropic highlight.
The highlight can be shifted up or down the surface using the Anisotropic Highlight Offset value.
The direction of the surface for anisotropic highlight is defined using a directional texture like 
these. These act similarly to tangent space normal maps, defining the direction of the surface. 
However, they should not be converted to normal maps in Unity.

Usage
Anisotropic Direction:  Direction of the surface highlight. Follows the same directional values as 
						a tangent space normal map.
Specular: 				The specular level is defined in the red channel of the specular texture. 
						This controls the brightness of the specular highlight.
Gloss: 					The gloss level is defined in the green channel of the specular texture. 
						This controls how sharp (full green) or wide (no green) the specular highlight 
						is. It's best to keep this value non-zero.
Anisotropic Mask: 		The blue channel of the specular texture is used to blend between anisotropic 
						and blinn highlights. Full blue = full anisotropic, no blue = full blinn.
Anisotropic Offset: 	Can be used to push the highlight towards or away from the centre point.

http://wiki.unity3d.com/index.php/Anisotropic_Highlight_Shader
-------------------------------------------------- */
Shader "AG/Anisotropic/Diffuse Normal Alpha-Test" {
     Properties {
         _Color ("Main Color", Color) = (1,1,1,1)
         _MainTex ("Diffuse (RGB) Alpha (A)", 2D) = "white" {}
         _SpecularColor ("Specular Intensity", Range(0, 2)) = 1
         _SpecularTex ("Specular (R) Gloss (G) Anisotropic Mask (B)", 2D) = "gray" {}
         _BumpMap ("Normal (Normal)", 2D) = "bump" {}
         _AnisoTex ("Anisotropic Direction (Normal)", 2D) = "bump" {}
         _AnisoOffset ("Anisotropic Highlight Offset", Range(-1,1)) = -0.2
         _Cutoff ("Alpha Cut-Off Threshold", Range(0,1)) = 0.5
     }

     SubShader{
         Tags { "RenderType" = "Opaque" }
     
         CGPROGRAM
         
             struct SurfaceOutputAniso {
                 fixed3 Albedo;
                 fixed3 Normal;
                 fixed4 AnisoDir;
                 fixed3 Emission;
                 half Specular;
                 fixed Gloss;
                 fixed Alpha;
             };

             float _AnisoOffset, _Cutoff;
             inline fixed4 LightingAniso (SurfaceOutputAniso s, fixed3 lightDir, fixed3 viewDir, fixed atten)
             {
             fixed3 h = normalize(normalize(lightDir) + normalize(viewDir));
             float NdotL = saturate(dot(s.Normal, lightDir));

             fixed HdotA = dot(normalize(s.Normal + s.AnisoDir.rgb), h);
             float aniso = max(0, sin(radians((HdotA + _AnisoOffset) * 180)));

             float spec = saturate(dot(s.Normal, h));
             spec = saturate(pow(lerp(spec, aniso, s.AnisoDir.a), s.Gloss * 128) * s.Specular);

             fixed4 c;
             c.rgb = ((s.Albedo * _LightColor0.rgb * NdotL) + (_LightColor0.rgb * spec)) * (atten * 2);
             c.a = 1;
             clip(s.Alpha - _Cutoff);
             return c;
             }

             #pragma surface surf Aniso
             #pragma target 3.0
             
             struct Input
             {
                 float2 uv_MainTex;
                 float2 uv_AnisoTex;
             };
             
             sampler2D _MainTex, _SpecularTex, _BumpMap, _AnisoTex;
             float4 _Color;
             float _SpecularColor; 
                 
             void surf (Input IN, inout SurfaceOutputAniso o)
             {
             fixed4 albedo = tex2D(_MainTex, IN.uv_MainTex);
              o.Albedo = albedo.rgb * _Color;
              o.Alpha = albedo.a;
              o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
              fixed3 spec = tex2D(_SpecularTex, IN.uv_MainTex).rgb;
              o.Specular = spec.r * _SpecularColor;
              o.Gloss = spec.g;
              o.AnisoDir = fixed4(UnpackNormal(tex2D(_AnisoTex, IN.uv_AnisoTex)), spec.b);
             }
         ENDCG
     }
     FallBack "Transparent/Cutout/VertexLit"
}