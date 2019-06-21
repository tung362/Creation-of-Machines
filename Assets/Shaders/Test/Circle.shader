Shader "Custom/Circle"
{
	Properties
	{
		_Texture("Texture", 2D) = "white" {}
		_SurfaceColor("Surface Color", Color) = (1,1,1,1)
		_Multiplyer("Multiplyer", float) = 1.0
		_OuterCircle("Outer Circle", Range(0, 1)) = 0.5
		_InnerCircle("Inner Circle", Range(0, 1)) = 0.5
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
		fixed4 _SurfaceColor;
		float _Multiplyer;

		half _OuterCircle;
		half _InnerCircle;

		struct Input
		{
			float2 uv_Texture;
			float3 viewDir;
			float3 worldPos;
			float4 screenPos;
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
			float theDistance = Distance(float2(0.5, 0.5), IN.uv_Texture);
			o.Albedo = _SurfaceColor;
			o.Albedo *= _Multiplyer;
			if (theDistance < _OuterCircle / 2 && theDistance > _InnerCircle / 2) o.Alpha = _SurfaceColor.a;
			else o.Alpha = 0;
		}
	ENDCG
	}
	FallBack "Diffuse"
}