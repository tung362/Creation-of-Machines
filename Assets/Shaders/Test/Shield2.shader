Shader "Custom/Shield2"
{
	Properties
	{
		_Texture("Texture", 2D) = "white" {}
		_InnerRange("Inner Range", float) = 0.0
		_OuterIntensity("Outer Intensity", float) = 0.0
		_SurfaceColor("Surface Color", Color) = (1,1,1,1)
		_Multiplyer("Multiplyer", Range(0, 2)) = 1.0
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		ZWrite Off
		//Blend One One
		LOD 200

		CGPROGRAM
		#pragma surface surf BlinnPhong alpha

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _Texture;
		float _InnerRange;
		float _OuterIntensity;
		fixed4 _SurfaceColor;
		half _Multiplyer;

		struct Input
		{
			float2 uv_Texture;
			float3 viewDir;
		};
		
		float Distance(float2 a, float2 b)
		{
			float2 distance = a - b;
			float magnitude = sqrt(distance.x*distance.x + distance.y*distance.y);
			return magnitude;
		}
		
		//void surf (Input IN, inout SurfaceOutputStandard o) 
		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 texColor = tex2D(_Texture, IN.uv_Texture);
			fixed4 scalar = saturate(1.0 - (1.0 - Distance(float2(0.5, 0.5), IN.uv_Texture)));
			scalar = pow(scalar, _InnerRange) * _OuterIntensity;
			o.Albedo = _SurfaceColor;
			o.Albedo *= _Multiplyer;
			o.Alpha = scalar * _SurfaceColor.a;
		}
	ENDCG
	}
	FallBack "Diffuse"
}