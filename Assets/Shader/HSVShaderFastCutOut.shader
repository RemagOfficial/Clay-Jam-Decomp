Shader "Custom/HSVShaderFastCutout" {
Properties {
 _MainTex ("Texture", 2D) = "white" {}
 _HueShift ("HueShift", Float) = 0
 _Sat ("Saturation", Float) = 1
 _Val ("Value", Float) = 1
 _VSU ("VSU", Float) = 0
 _VSW ("VSW", Float) = 0
 _RR ("RR", Float) = 0
 _RG ("RG", Float) = 0
 _RB ("RB", Float) = 0
 _GR ("GR", Float) = 0
 _GG ("GG", Float) = 0
 _GB ("GB", Float) = 0
 _BR ("BR", Float) = 0
 _BG ("BG", Float) = 0
 _BB ("BB", Float) = 0
}
	//DummyShaderTextExporter
	
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard fullforwardshadows
#pragma target 3.0
		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
		}
		ENDCG
	}
}