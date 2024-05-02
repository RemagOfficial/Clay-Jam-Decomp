Shader "Custom/AvalancheShaderFlash" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
 _Saturation ("Saturation", Color) = (0,0,0,0)
}
SubShader { 
 Tags { "QUEUE"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" }
  ZWrite Off
  Blend SrcAlpha OneMinusSrcAlpha
  ColorMaterial AmbientAndDiffuse
  SetTexture [_MainTex] { combine primary * texture, texture alpha * primary alpha }
  SetTexture [_MainTex] { ConstantColor [_Saturation] combine previous + constant, previous alpha }
  SetTexture [_MainTex] { ConstantColor [_Color] combine previous * constant double, previous alpha }
 }
}
Fallback "Alpha/VertexLit"
}