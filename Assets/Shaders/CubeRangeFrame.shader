// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "UFrame/CubeRangeFrame"
{
	Properties
	{
		_Color("Color", color) = (1,1,1,1)
		_CenterColor("Center Color", color) = (1,1,1,1)
		_Width("Width", range(0.01,1)) = 0.1
		_CenterWidth("Center Width", range(0.01,0.5)) = 0.1
	}
		SubShader
	{
		Tags { "Queue" = "Transparent" }
		Pass {
			Cull front
			ZWrite off
			blend srcalpha oneminussrcalpha
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			fixed4 _Color;
			fixed4 _CenterColor;
			fixed _Width;
			fixed _CenterWidth;

			struct a2v {
			  float4 vertex : POSITION;
			  float2 uv : TEXCOORD0;
			};

			struct v2f {
			   float4 pos : SV_POSITION;
			   //float2 uv : TEXCOORD0;
			   float4 wpos : TEXCOORD1;
			};

			v2f vert(a2v v) {
			   v2f o;
			   o.pos = UnityObjectToClipPos(v.vertex);
			   o.wpos = v.vertex;
			   return o;
			}

			float4 frag(v2f i) : SV_Target {
		       fixed4 col = _Color;
			   float len = length(i.wpos.xyz);
			   col *= step(_Width,len);

			   fixed4 centerCol = _CenterColor;
			   centerCol *= step(len, 0.4 + _CenterWidth);
			   return col + centerCol;
			}
			ENDCG
		}
	}
}