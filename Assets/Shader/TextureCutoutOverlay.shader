Shader "Custom/TextureCutoutOverlay" {
Properties {
 _MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
}
SubShader { 
 LOD 100
 Tags { "QUEUE"="Overlay" "IGNOREPROJECTOR"="true" "RenderType"="TransparentCutout" }
 Pass {
  Tags { "QUEUE"="Overlay" "IGNOREPROJECTOR"="true" "RenderType"="TransparentCutout" }
  ZWrite Off
  Fog { Mode Off }
  Blend SrcAlpha OneMinusSrcAlpha
  ColorMask RGB
  ColorMaterial AmbientAndDiffuse
  Offset -1, -1
  SetTexture [_MainTex] { combine texture * primary }
 }
}
}