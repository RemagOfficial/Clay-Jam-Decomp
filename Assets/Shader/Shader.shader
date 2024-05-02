Shader "Gouge" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _Ambient ("Ambient Color", Color) = (0,0,0,1)
 _Emission ("Emissive Color", Color) = (0,0,0,0)
}
SubShader { 
 Tags { "QUEUE"="AlphaTest" "IGNOREPROJECTOR"="true" "RenderType"="TransparentCutout" }
 Pass {
  Tags { "QUEUE"="AlphaTest" "IGNOREPROJECTOR"="true" "RenderType"="TransparentCutout" }
  Lighting On
  Material {
   Ambient [_Ambient]
   Diffuse [_Color]
   Emission [_Emission]
  }
  ZWrite Off
  Blend DstColor SrcColor
 }
}
}