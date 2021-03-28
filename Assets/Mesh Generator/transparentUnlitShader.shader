﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/transparentUnlitShader"
{
     Properties {
         _Color ("Base (RGB) Trans (A)", Color) = (0.0,0.0,0.0,0.0)
     }
     
     SubShader {
         Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
         LOD 100
         
         ZWrite Off
         Blend SrcAlpha OneMinusSrcAlpha 
         
         Pass {  
             CGPROGRAM
                 #pragma vertex vert
                 #pragma fragment frag
                 #pragma multi_compile_fog
                 
                 #include "UnityCG.cginc"
     
                 struct appdata_t {
                     float4 vertex : POSITION;
                     float2 texcoord : TEXCOORD0;
                 };
     
                 struct v2f {
                     float4 vertex : SV_POSITION;
                     half2 texcoord : TEXCOORD0;
                     UNITY_FOG_COORDS(1)
                 };
     
                 fixed4 _Color;
                 
                 v2f vert (appdata_t v)
                 {
                     v2f o;
                     o.vertex = UnityObjectToClipPos(v.vertex);
                     return o;
                 }
                 
                 fixed4 frag (v2f i) : SV_Target
                 {
                     return _Color;
                 }
             ENDCG
         }
     }
 
}
