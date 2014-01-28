Shader "AG/Invisible Shadow Caster" {
    Subshader
    {
       UsePass "VertexLit/SHADOWCOLLECTOR"    
       UsePass "VertexLit/SHADOWCASTER"
    }
 
    Fallback off
}
