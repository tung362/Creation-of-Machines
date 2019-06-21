Shader "Custom/RegularUnlit"
{
	Properties
	{
		_Texture("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		ZWrite Off
		Blend One One
		Lighting Off
		LOD 200

		CGPROGRAM
		#pragma surface surf BlinnPhong alpha

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		//Custom Variables
		sampler2D _Texture;
		float4 _Color;

		struct Input
		{
			float2 uv_Texture;
		};

		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 texColor = tex2D(_Texture, IN.uv_Texture);
			texColor *= _Color;
			o.Emission = texColor;
			o.Alpha = texColor.a * texColor.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
