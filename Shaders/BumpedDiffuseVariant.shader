// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Bumped shader. Differences from regular Bumped one:
// - no Main Color
// - Normalmap uses Tiling/Offset of the Base texture
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Mobile/Bumped Diffuse Variant" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _BumpMap ("Normalmap", 2D) = "bump" {}
}

SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 250

	CGPROGRAM
		#pragma surface surf Lambert noforwardadd

		sampler2D _MainTex;
		sampler2D _BumpMap;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			//o.Normal = tex2D(_BumpMap, IN.uv_MainTex);
			//o.Normal = tex2D(_BumpMap, IN.uv_MainTex) * 2 - 1;
			o.Normal.rg = tex2D(_BumpMap, IN.uv_MainTex).rg * 2 - 1;
			o.Normal.b = 1;
		}
		ENDCG
	}

	FallBack "Mobile/Diffuse"
}