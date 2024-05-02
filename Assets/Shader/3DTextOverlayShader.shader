Shader "Custom/3DTextOverlayShader" {
Properties {
 _EmisColor ("Emissive Color", Color) = (0.2,0.2,0.2,0)
 _MainTex ("Particle Texture", 2D) = "white" {}
}
SubShader { 
 Tags { "QUEUE"="Overlay" "IGNOREPROJECTOR"="true" "RenderType"="TransparentCutout" }
 Pass {
  Tags { "QUEUE"="Overlay" "IGNOREPROJECTOR"="true" "RenderType"="TransparentCutout" }
  Color [_EmisColor]
  ZWrite Off
  Cull Off
  Blend SrcAlpha OneMinusSrcAlpha
  SetTexture [_MainTex] { combine texture * primary }
 }
}
}