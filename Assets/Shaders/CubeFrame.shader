Shader "UFrame/CubeFrame"
{
	Properties
	{
		_Color("Color", color) = (1,1,1,1)
		_Width("Width", range(0.01,0.5)) = 0.1
	}
		SubShader
	{
		Tags { "Queue" = "Transparent" }
		Pass {
			cull off
			ZWrite off
			blend srcalpha oneminussrcalpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			fixed4 _Color;
			fixed _Width;

			struct a2v {
			  float4 vertex : POSITION;
			  float2 uv : TEXCOORD0;
			};

			struct v2f {
			   float4 pos : SV_POSITION;
			   float2 uv : TEXCOORD0;
			};

			v2f vert(a2v v) {
			   v2f o;
			   o.pos = UnityObjectToClipPos(v.vertex);
			   o.uv = v.uv;
			   return o;
			}

			float4 frag(v2f i) : SV_Target {
			   fixed4 col = _Color;//CubeµÄ»ù´¡ÑÕÉ«
			   fixed left = max(lerp(1, 0, i.uv.x / _Width), 0);
			   fixed top = max(lerp(1, 0, i.uv.y / _Width), 0);
			   fixed right = max(lerp(1, 0, (1 - i.uv.x) / _Width), 0);
			   fixed down =  max(lerp(1, 0, (1 - i.uv.y) / _Width), 0);
			   col.a *= saturate(left + right + top + down);
			   return col;
			}
			ENDCG
		}
	}
}

/*fixed4 col = _Color;
float lx = step(_Width, i.uv.x);
float ly = step(_Width, i.uv.y);
float hx = step(i.uv.x, 1.0 - _Width);
float hy = step(i.uv.y, 1.0 - _Width);
col.a = 1 - lx * ly * hx * hy;*/