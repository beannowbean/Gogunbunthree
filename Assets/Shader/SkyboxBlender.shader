Shader "Custom/SkyboxBlender"
{
    Properties
    {
        _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
        [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Rotation", Range(0, 360)) = 0
        [NoScaleOffset] _Tex1 ("Day Texture", Cube) = "grey" {}
        [NoScaleOffset] _Tex2 ("Night Texture", Cube) = "grey" {}
        _Blend ("Blend (0:Day, 1:Night)", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            samplerCUBE _Tex1;
            samplerCUBE _Tex2;
            half4 _Tint;
            half _Exposure;
            float _Rotation;
            float _Blend;

            float3 RotateAroundYInDegrees(float3 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = RotateAroundYInDegrees(v.vertex.xyz, _Rotation);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 두 개의 큐브맵 샘플링
                half4 tex1 = texCUBE(_Tex1, i.texcoord);
                half4 tex2 = texCUBE(_Tex2, i.texcoord);
                
                // 두 색상을 _Blend 값에 따라 섞음
                half4 res = lerp(tex1, tex2, _Blend);
                
                // 틴트 및 노출도 적용
                half3 c = res.rgb * _Tint.rgb * unity_ColorSpaceDouble.rgb;
                c *= _Exposure;
                
                return half4(c, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}