Shader "Custom/VertexColNoTex" {
Properties {
 _Color ("Color Tint", Color) = (1,1,1,1)
}
SubShader { 
 Tags { "QUEUE"="Geometry+1" "IGNOREPROJECTOR"="true" "RenderType"="TransparentCutout" }
 Pass {
  Tags { "QUEUE"="Geometry+1" "IGNOREPROJECTOR"="true" "RenderType"="TransparentCutout" }
  Color [_Color]
  ZWrite Off
  Blend SrcAlpha OneMinusSrcAlpha
 }
}
}