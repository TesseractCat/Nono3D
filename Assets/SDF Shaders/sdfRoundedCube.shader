// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/sdfRoundedCube"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Epsilon ("Epsilon", float) = 0.1
        _Steps ("Steps", int) = 20
        _BoxDimensions ("Box Dimensions", Vector) = (1.0,1.0,1.0,1.0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Epsilon;
            int _Steps;
            float4 _BoxDimensions;

            float sdRoundBox(float3 p, float3 b, float3 cornerRadius) {
                float3 q = abs(p) - b;
                return length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0) - cornerRadius;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 position = i.worldPosition;
                float3 viewDirection = normalize(position - _WorldSpaceCameraPos);

                fixed4 raycastCol = fixed4(0.0,0.0,0.0,0.0);

                float3 offset = float3(0.0,0.0,0.0) - mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0)).xyz;

                for (int k = 0; k < _Steps; k++) {
                    if (sdRoundBox(position + offset, _BoxDimensions.xyz, _BoxDimensions.w) < _Epsilon) {
                        raycastCol = fixed4(1.0,1.0,1.0,1.0);
                    }

                    position += viewDirection * sdRoundBox(position + offset, _BoxDimensions, _BoxDimensions.w); 
                }

                if (raycastCol.a == 0) {
                    clip(-1);
                }

                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
