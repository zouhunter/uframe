Shader "Ogxd/Grid"
{
   Properties
   {
      _Color("Main Color", Color) = (0.5, 1.0, 1.0)
      _SecondaryColor("Secondary Color", Color) = (0.0, 0.0, 0.0)
      _BackgroundColor("Background Color", Color) = (0.0, 0.0, 0.0, 0.0)
      _MainTex("Main Texture", 2D) = "white" {}

      // 添加 Stencil 和 ColorMask 属性
      _Stencil ("Stencil Reference", Int) = 0
      _StencilComp ("Stencil Comparison", Int) = 8
      _StencilOp ("Stencil Operation", Int) = 0
      _StencilReadMask("Stencil Read Mask", Int) = 255
      _StencilWriteMask("Stencil Write Mask", Int) = 255
      _ColorMask("Color Mask", Int) = 15  // 新增 ColorMask 属性

      [Header(Grid)]
      _Scale("Scale", Float) = 1.0
      _GraduationScale("Graduation Scale", Float) = 1.0
      _Thickness("Lines Thickness", Range(0.0001, 0.01)) = 0.005
      _SecondaryFadeInSpeed("Secondary Fade In Speed", Range(0.1, 4)) = 0.5
   }
   SubShader
   {
      Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
      ZWrite Off  // 关闭深度写入
      ZTest LEqual // 使用 ZTest LEqual 以正确混合 UI 元素
      Blend SrcAlpha OneMinusSrcAlpha

      Pass
      {
         // 使用属性来设置 Stencil 和 ColorMask 操作
         Stencil
         {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
         }
         
         ColorMask [_ColorMask]  // 添加 ColorMask 操作

         CGPROGRAM
         #pragma vertex vert
         #pragma fragment frag
         #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
         #include "UnityCG.cginc"

         struct appdata
         {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
         };

         struct v2f
         {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
            UNITY_VERTEX_OUTPUT_STEREO
         };

         // sampler2D _MaskTexture;
         // float4 _MaskTexture_ST;

         float _Scale;
         float _GraduationScale;

         float _Thickness;
         float _SecondaryFadeInSpeed;

         fixed4 _Color;
         fixed4 _SecondaryColor;
         fixed4 _BackgroundColor;

         v2f vert (appdata v)
         {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            o.vertex = UnityObjectToClipPos(v.vertex);

            // Remap UVs from [0:1] to [-0.5:0.5] to make scaling effect start from the center 
            o.uv = v.uv - 0.5f;
            // Scale the whole thing if necessary
            o.uv *= _GraduationScale;

            return o;
         }

         // Remap value from a range to another
         float remap(float value, float from1, float to1, float from2, float to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
         }

         // Apply the calculated scale
         float applyScale(float base, float scale) {
            return floor(frac((base - 0.5 * _Thickness) * scale) + _Thickness * scale);
         }

         fixed4 frag (v2f i) : SV_Target
         {
            fixed4 col;

            float logMappedScale = _Scale / pow(10, ceil(log10(_Scale)));
            float localScale = 1 / logMappedScale;
            float fade =  pow(1 - remap(logMappedScale, 0.1, 1, 0.00001, 0.99999), _SecondaryFadeInSpeed);

            float2 pos;

            pos.x = applyScale(i.uv.x, localScale);
            pos.y = applyScale(i.uv.y, localScale);

            if (pos.x == 1 || pos.y == 1) {
               col = _Color;
               col.a = max((1 - fade), fade);
            } else {
               pos.x = applyScale(i.uv.x, 10.0 * localScale);
               pos.y = applyScale(i.uv.y, 10.0 * localScale);

               if (pos.x == 1 || pos.y == 1) {
                  col = _SecondaryColor;
                  col.a = (1 - fade);
               } else {
                  col = _BackgroundColor;
               }
            }

            // // 采样 Mask 贴图的 Alpha 通道
            // fixed4 maskCol = tex2D(_MaskTexture, i.uv);
            // // 将 Mask 的 Alpha 乘到当前的颜色 Alpha 上
            // col.a *= maskCol.a;

            return col;
         }

         ENDCG
      }
   }
}
