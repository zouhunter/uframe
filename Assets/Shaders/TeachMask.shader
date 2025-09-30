// 遮罩
Shader "UFrame/TeachMask"
{
	Properties
	{
		_Color("Tint", Color) = (1,1,1,1)
		_Rect("Rect", vector) = (0, 0,100,100)
		_Smooth("Smooth", Range(1,10)) = 1
		_Round("Round", Range(1,1000)) = 1
		_Bound("Bound", float) = 1
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Name "Default"

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_ALPHACLIP

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
			};

			fixed4 _Color;
			float _Smooth;
			float _Round;
			float4 _Rect;
			float _Bound;
			sampler2D _MainTex;

			float4 _ClipRect;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				return OUT;
			}
			
			//calcute alpah
			float getAlphaMuti(v2f IN,float round)
			{
				float centerX = _Rect.x +_Rect.z * 0.5;
				float centerY = _Rect.y +_Rect.w * 0.5;

				float xDistance = abs(IN.worldPosition.x - centerX);//x
				float yDistance = abs(IN.worldPosition.y - centerY);//y

				float width = _Bound + _Rect.z * 0.5;
				float height = _Bound + _Rect.w * 0.5;

				float xRound = width - round;//x0
				float yRound = height - round;//y0

				float xRoundDistance = max(0, xDistance - xRound);
				float yRoundDistance = max(0, yDistance - yRound);
				float centerDistance = distance(float2(0, 0), float2(xRoundDistance, yRoundDistance));
				float dcenter = step(centerDistance, round);

				float smoothDistance = clamp(round - centerDistance,0, _Smooth);
				dcenter *= smoothDistance / _Smooth;

				float rx = fmod(xDistance, xRound);
				float ry = fmod(yDistance, yRound);

				float mx = step(width, abs(xDistance));
				float my = step(height, abs(yDistance));

				return step(mx + my, 0.5) * dcenter;
			}


			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 color = IN.color + tex2D(_MainTex, IN.texcoord);
				color.a = IN.color.a;
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

				#ifdef UNITY_UI_ALPHACLIP
					clip(color.a - 0.001);
				#endif
				float muti_a = getAlphaMuti(IN, _Round);
				color.a *= (1 - muti_a);
				return color;
			}
			ENDCG
		}
	}
}
