Shader "LXCommon/Core/URP/ThickLineShader"
{
    Properties
    {
        _LineThickness ("Line thickness", Float) = 3
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }
        LOD 100
        Cull Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #pragma target 4.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float thickness : TEXCOORD0;
            };

            struct v2g
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float thickness : TEXCOORD0;
            };

            struct g2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            float4 _Color;
            half _LineThickness;

            v2g vert (appdata v)
            {
                v2g o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.color = v.color;
                o.thickness = v.thickness;
                return o;
            }

            [maxvertexcount(6)]
            void geom(line v2g input[2], inout TriangleStream<g2f> triStream)
            {
                float4 side = normalize(float4(input[1].vertex.y - input[0].vertex.y, input[0].vertex.x - input[1].vertex.x, 0, 0)) * (min(input[0].vertex.w, input[1].vertex.w) / _ScreenParams.x * _LineThickness * input[0].thickness);
                g2f output;
                float4 topLeft = input[0].vertex - side, topRight = input[0].vertex + side, bottomLeft = input[1].vertex - side, bottomRight = input[1].vertex + side;

                output.color = input[0].color;
                output.vertex = topLeft;
                triStream.Append(output);

                output.color = input[1].color;
                output.vertex = bottomLeft;
                triStream.Append(output);

                output.color = input[1].color;
                output.vertex = bottomRight;
                triStream.Append(output);

                output.color = input[0].color;
                output.vertex = topLeft;
                triStream.Append(output);

                output.color = input[1].color;
                output.vertex = bottomRight;
                triStream.Append(output);

                output.color = input[0].color;
                output.vertex = topRight;
                triStream.Append(output);
            }

            half4 frag (g2f i) : SV_Target
            {
                float4 col = i.color;
                return col;
            }
            ENDHLSL
        }
    }
}
