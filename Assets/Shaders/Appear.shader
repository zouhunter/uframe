Shader "UFrame/Appear" {
    Properties{
        _Color("Color Primary", Color) = (0.5,0.5,0.5,1)        
        _MainTex("Main Texture", 2D) = "white" {}

        _Speed("MoveSpeed", Range(1,50)) = 10 
        [Toggle(DOWN_ON)] _DOWN("Move Down?", Int) = 0
            
    }
 
        SubShader{
            Tags { "RenderType" = "Opaque"  }
            LOD 200
 
        CGPROGRAM
        #pragma surface surf ToonRamp vertex:vert addshadow 

        #pragma lighting ToonRamp exclude_path:prepass
        #pragma multi_compile_instancing


        #pragma shader_feature DOWN_ON


        inline half4 LightingToonRamp(SurfaceOutput s, half3 lightDir, half atten)
        {
            #ifndef USING_DIRECTIONAL_LIGHT
            lightDir = normalize(lightDir);
            #endif
            
            float d = dot(s.Normal, lightDir);
            float3 ramp = smoothstep(0, d + 0.06, d) + 0.8f;
            half4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten * 2);
            c.a = 0;
            return c;
        }
 
        sampler2D _MainTex;
        float _Speed;


        float4 _Color;
 
        struct Input {
            float2 uv_MainTex : TEXCOORD0;
        };
 
        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_DEFINE_INSTANCED_PROP(float, _Moved)
        UNITY_INSTANCING_BUFFER_END(Props)
 
        void vert(inout appdata_full v)//
        {
            v.vertex.xyz *= UNITY_ACCESS_INSTANCED_PROP(Props, _Moved);
            #if DOWN_ON
            v.vertex.y += _Speed-UNITY_ACCESS_INSTANCED_PROP(Props, _Moved * _Speed);
            #else
            v.vertex.y -= _Speed-UNITY_ACCESS_INSTANCED_PROP(Props, _Moved * _Speed);
            #endif
        }
 
        void surf(Input IN, inout SurfaceOutput o) {
            half4 c = tex2D(_MainTex, IN.uv_MainTex)* _Color;       
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
        }
            Fallback "Diffuse"
}