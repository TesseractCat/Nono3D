Shader "Unlit/voxelUnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NumberTex ("Number Texture Atlas", 2D) = "white" {}
        _GroupTex ("Group Indicator Texture Atlas", 2D) = "white" {}
        _AlphaCutoff ("Alpha Cutoff", float) = 0.3
        
        _ChunkSize ("Chunk Size", Vector) = (10.0,10.0,10.0,1.0)
        
        _DimVector ("Dim Vector", Vector) = (0,0,0,0)
        _DimAmount ("Dim Amount", float) = 0.3
        
        _OutlineCenter ("Outline Center", Vector) = (0,0,0,0)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", float) = 0.2
        
        _InnerOutlineThickness ("Inner Outline Thickness", float) = 0.1
        _InnerBackgroundColor ("Inner Background Color", Color) = (1,1,1,1)
        
        _SelectionColor ("Selection Color", Color) = (1,1,1,1)
        _HighlightColor ("Highlight Color", Color) = (1,1,1,1)
        _HighlightAndSelectionColor ("Highlight And Selection Color", Color) = (1,1,1,1)
        
        _HighlightVolumeTex ("Highlight Volume Texture", 3D) = "black" {}
        _ColorVolumeTex ("Color Volume Texture", 3D) = "transparent" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
        
            Stencil
            {
                Ref 4
                Comp always
                Pass replace
                ZFail keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color : COLOR0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _NumberTex;
            sampler2D _GroupTex;
            
            sampler3D _HighlightVolumeTex;
            sampler3D _ColorVolumeTex;
            
            float _AlphaCutoff;
            float3 _ChunkSize;
            
            fixed4 _DimVector;
            float _DimAmount;
            
            float _InnerOutlineThickness;
            fixed4 _OutlineColor;
            fixed4 _InnerBackgroundColor;
            
            fixed4 _SelectionColor;
            fixed4 _HighlightColor;
            fixed4 _HighlightAndSelectionColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o.normal = v.normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //Sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col = _InnerBackgroundColor;
                
                //Sample the color volume texture
                fixed4 color_volume_col = tex3D(_ColorVolumeTex, (i.color.xyz*_ChunkSize + float3(0.5,0.5,0.5))/_ChunkSize);
                if (color_volume_col.a > 0.0) {
                    col = color_volume_col;
                }
                
                //Sample the highlight volume texture
                fixed4 highlight_volume_col = tex3D(_HighlightVolumeTex, (i.color.xyz*_ChunkSize + float3(0.5,0.5,0.5))/_ChunkSize);
                int alpha = (highlight_volume_col.a * 255);
                if ((alpha & 2) > 0) {
                    col = _SelectionColor;
                }
                if ((alpha & 1) > 0) {
                    col = _HighlightColor;
                    
                    if ((alpha & 2) > 0) {
                        col = _HighlightAndSelectionColor;
                    }
                }
                
                //Draw numbers
                //x
                if (highlight_volume_col.x > 0.0 && abs(i.normal.x) > 0.5) {
                    //Get number
                    int num = ((highlight_volume_col.x*255)-1.0);
                    int count = ((num & 240) >> 4) - 1;
                    int groups = (num & 15);
                    if (groups > 3) {
                        groups = 3;
                    }
                    
                    if (tex2D(_NumberTex, (i.uv/float2(10.0,1.0)) + float2((1.0/10.0) * count, 0.0)).a > _AlphaCutoff && count > -1) {
                        col = tex2D(_NumberTex, (i.uv/float2(10.0,1.0)) + float2((1.0/10.0) * count, 0.0));
                    }
                    if (tex2D(_GroupTex, (i.uv/float2(4.0,1.0)) + float2((1.0/4.0) * groups, 0.0)).a > _AlphaCutoff && count > -1) {
                        col = tex2D(_GroupTex, (i.uv/float2(4.0,1.0)) + float2((1.0/4.0) * groups, 0.0));
                    }
                }
                //y
                if (highlight_volume_col.y > 0.0 && abs(i.normal.y) > 0.5) {
                    //Get number
                    int num = ((highlight_volume_col.y*255)-1.0);
                    int count = ((num & 240) >> 4) - 1;
                    int groups = (num & 15);
                    if (groups > 3) {
                        groups = 3;
                    }
                    
                    if (tex2D(_NumberTex, (i.uv/float2(10.0,1.0)) + float2((1.0/10.0) * count, 0.0)).a > _AlphaCutoff && count > -1) {
                        col = tex2D(_NumberTex, (i.uv/float2(10.0,1.0)) + float2((1.0/10.0) * count, 0.0));
                    }
                    if (tex2D(_GroupTex, (i.uv/float2(4.0,1.0)) + float2((1.0/4.0) * groups, 0.0)).a > _AlphaCutoff && count > -1) {
                        col = tex2D(_GroupTex, (i.uv/float2(4.0,1.0)) + float2((1.0/4.0) * groups, 0.0));
                    }
                }
                //z
                if (highlight_volume_col.z > 0.0 && abs(i.normal.z) > 0.5) {
                    //Get number
                    int num = ((highlight_volume_col.z*255)-1.0);
                    int count = ((num & 240) >> 4) - 1;
                    int groups = (num & 15);
                    if (groups > 3) {
                        groups = 3;
                    }
                    
                    if (tex2D(_NumberTex, (i.uv/float2(10.0,1.0)) + float2((1.0/10.0) * count, 0.0)).a > _AlphaCutoff && count > -1) {
                        col = tex2D(_NumberTex, (i.uv/float2(10.0,1.0)) + float2((1.0/10.0) * count, 0.0));
                    }
                    if (tex2D(_GroupTex, (i.uv/float2(4.0,1.0)) + float2((1.0/4.0) * groups, 0.0)).a > _AlphaCutoff && count > -1) {
                        col = tex2D(_GroupTex, (i.uv/float2(4.0,1.0)) + float2((1.0/4.0) * groups, 0.0));
                    }
                }
                
                //Draw outline
                if (i.uv.x < _InnerOutlineThickness || i.uv.y < _InnerOutlineThickness || i.uv.x > (1-_InnerOutlineThickness) || i.uv.y > (1-_InnerOutlineThickness)) {
                    col = _OutlineColor;
                }
                
                //Dim on scrobble mode
                //If the dot of the DimVector (with one non-zero .xyz component) with i.color.xyz
                if (abs(dot(_DimVector.xyz, i.color.xyz*_ChunkSize + (_DimVector.xyz/_DimVector.xyz)) - dot(_DimVector.xyz, _DimVector.xyz)) > 0.1) {
                    col = col * (1-(_DimAmount * _DimVector.a));
                }

                //Shading
                //col = col * (max(dot(i.normal, _WorldSpaceLightPos0.xyz), 0.0) + 0.8);
                
                return col;
            }
            ENDCG
        }
        
        Pass
        {
            Cull OFF
            Zwrite OFF
            ZTest ON
            
            Stencil
            {
                Ref 4
                Comp notequal
                Fail keep
                Pass replace
            }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };
            
            float4 _OutlineCenter;
            fixed4 _OutlineColor;
            float _OutlineThickness;

            v2f vert (appdata v)
            {
                v2f o;
                float4 newPos = v.vertex;
                newPos = newPos - _OutlineCenter; 
                newPos = newPos * (1.0f + _OutlineThickness);
                newPos = newPos + _OutlineCenter; 
                o.vertex = UnityObjectToClipPos(newPos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _OutlineColor;
                return col;
            }

            ENDCG
        }
    }
}
