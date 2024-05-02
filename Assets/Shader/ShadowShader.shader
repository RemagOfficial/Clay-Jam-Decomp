Shader "Shadow" {
Properties {
 _Color ("Color Tint", Color) = (1,1,1,1)
 _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
}
SubShader { 
 Tags { "QUEUE"="Geometry+1" "IGNOREPROJECTOR"="true" "RenderType"="TransparentCutout" }
 Pass {
  Tags { "QUEUE"="Geometry+1" "IGNOREPROJECTOR"="true" "RenderType"="TransparentCutout" }
  ZWrite Off
  Blend SrcAlpha OneMinusSrcAlpha
  SetTexture [_MainTex] { ConstantColor [_Color] combine texture * constant }
 }
}
}