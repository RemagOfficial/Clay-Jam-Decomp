Shader "Custom/VertexColored2" {
Properties {
 _MainTex ("Texture", 2D) = "" {}
}
SubShader { 
 Tags { "QUEUE"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" }
  ZWrite Off
  Blend SrcAlpha OneMinusSrcAlpha
  ColorMaterial AmbientAndDiffuse
  SetTexture [_MainTex] { combine texture * primary, texture alpha * primary alpha }
 }
}
}