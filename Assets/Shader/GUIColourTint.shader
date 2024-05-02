Shader "Custom/GUIColourTint" {
Properties {
 _Color ("Base Color", Color) = (1,1,1,1)
 _MainTex ("Texture", 2D) = "" {}
}
SubShader { 
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
  Color [_Color]
  ZWrite Off
  Cull Off
  SetTexture [_MainTex] { combine texture * primary }
 }
}
}