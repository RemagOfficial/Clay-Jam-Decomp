Shader "Custom/PebbleSmoke" {
Properties {
 _MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
}
SubShader { 
 LOD 100
 Tags { "QUEUE"="Overlay-500" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Overlay-500" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
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