Shader "Custom/UberShader"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_AmbientCol("Ambient Color", Color) = (1,1,1,1)
		_SpecColor("Specular Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_AO("Ambient Occlusion", 2D) = "white" {}
		_AoStrength("Ambient Amount", range(0,1)) = 0
		_Bump("Normal Map", 2D) = "bump" {}
		_Specular("Specular(RGBA)", 2D) = "white" {}
		_Alpha("Transparency(RGB)Alpha(A)", 2D) = "white" {}
		_AlphaStrength("Transparent Amount", range(0,1)) = 0.5
		_Glossy("Glossy", range(1,10)) = 1
		_Emission("Glow (Alpha)", 2D) = "White" {}
		_EmitColor("Glow Color", Color) = (1,1,1,1)
		_GlowAmount("Glow Amount", range(0,2)) = 0
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

		LOD 200

		Pass
		{
			ZWrite On
			ColorMask 0
		}

		CGPROGRAM
		#pragma surface surf BlinnPhong alpha

		float4 _Color;
		float4 _AmbientCol;
		sampler2D _MainTex;
		sampler2D _AO;
		float _AoStrength;
		sampler2D _Bump;
		sampler2D _Specular;
		sampler2D _Alpha;
		float _AlphaStrength;
		float _Glossy;
		sampler2D _Emission;
		float4 _EmitColor;
		float _GlowAmount;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_AO;
			float2 uv_Bump;
			float2 uv_Specular;
			float2 uv_Alpha;
			float2 uv_Emission;
		};

		void surf(Input IN, inout SurfaceOutput o)
		{
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			half4 AO = tex2D(_AO, IN.uv_AO) * _AmbientCol * _AoStrength;
			half4 Alpha = tex2D(_Alpha, IN.uv_Alpha);
			half4 Spec = tex2D(_Specular, IN.uv_Specular);
			half4 EMIT = tex2D(_Emission, IN.uv_Emission) * _GlowAmount * _EmitColor;
			o.Albedo = (c.rgb * _Color) + AO.rgb;
			o.Normal = UnpackNormal(tex2D(_Bump, IN.uv_Bump));
			o.Specular = _Glossy * (1 - Spec);
			o.Gloss = 1.0;
			o.Alpha = Alpha.rgb * _AlphaStrength;
			o.Emission = EMIT;
		}
		ENDCG
	}
	FallBack "Diffuse"
}