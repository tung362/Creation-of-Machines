//Shader "Masked/Mask" 
//{
//	SubShader
//	{
//		//Stencil
//		//{
//		//	Ref 2
//		//	//Comp Greater
//		//	//Pass Replace
//		//}
//
//		// Render the mask after regular geometry, but before masked geometry and
//		// transparent things.
//		Tags{ "Queue" = "Geometry+10" }
//		// Don't draw in the RGBA channels; just the depth buffer
//		ColorMask 0
//		ZWrite On
//		// Do nothing specific in the pass:
//		Pass{}
//	}
//}

Shader "Masked/Mask" {

	SubShader{
		// Render the mask after regular geometry, but before masked geometry and
		// transparent things.

		Tags{ "Queue" = "Geometry-1" }

		// Don't draw in the RGBA channels; just the depth buffer

		ColorMask 0
		ZWrite On

		// Do nothing specific in the pass:

		Pass{}
	}
}
