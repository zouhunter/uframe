// 遮罩

Shader "UI/HitMask"
{
	Properties
	{
		_Color("Tint", Color) = (1,1,1,1)
		[Toggle] _EnableWave("WaveEnable", Float) = 0
		_WaveSize("WaveSize", Range(0.1,100)) = 1
		_WaveFade("WaveFade", Range(0,1)) = 0
		_WaveSpeed("WaveSpeed", Range(-10,10)) = 1
		_Smooth("Smooth", Range(1,10)) = 1
		_Center("Center", vector) = (0, 0, 0, 0)
		_Slider("Radius", Range(0,1000)) = 1000
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
			float4 _ClipRect;
			float _WaveSpeed;
			float _WaveSize;
			float _Slider;
			float2 _Center;
			bool _EnableWave;
			float _Smooth;
			float _WaveFade;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				return OUT;
			}
			
			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 color =/* (tex2D(_MainTex, IN.texcoord.xy)) **/ IN.color;
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

				#ifdef UNITY_UI_ALPHACLIP
					clip(color.a - 0.001);
				#endif
				float centerDistance = distance(IN.worldPosition.xy, _Center.xy);
				float innerCercle = _Slider - _Smooth;
				if (centerDistance > _Slider)
				{
					if (_EnableWave)
					{
						float dis = distance(IN.worldPosition.xy, _Center.xy);
						float fade = 1 - dis / _Slider * _WaveFade;
						color.a *= fade * abs(sin(centerDistance / _WaveSize + _Time.y * _WaveSpeed));//(_Slider / dis) * (_WaveFade)
					}
				}
				else if (centerDistance > innerCercle && innerCercle > 0)
				{
					color.a *= (centerDistance - innerCercle) / _Smooth;
				}
				else
				{
					clip(-1);
				}
				//-------------------add----------------------
				return color;
			}
			ENDCG
		}
	}
}
