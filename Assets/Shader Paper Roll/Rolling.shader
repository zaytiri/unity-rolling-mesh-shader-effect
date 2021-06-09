Shader "Custom/Rolling"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _StartAngle("Start Angle (Radians)", float) = 60.0
        _AnglePerUnit("Radians per Unit", float) = 0.2
        _Pitch("Pitch", float) = 0.02
        _UnrolledAngle("Unrolled Angle", float) = 1.5

        radiusAtSplit("radiusAtSplit", float) = 1
        
    }
    SubShader
    {
        //Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        //ZWrite Off
        //Blend SrcAlpha OneMinusSrcAlpha
        //Cull front

        Tags {"RenderType"="Opaque"}
        Cull Off

        LOD 200

        CGPROGRAM

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert //alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _StartAngle;
        float _AnglePerUnit;
        float _Pitch;
        float _UnrolledAngle;


        float fromStart;
        float fromOrigin;
        float radiusAtSplit = 1;
        float radius;
        float shifted;
        float3 objectWorldPosition;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float arcLengthToAngle(float angle) {
            float radical = sqrt(angle * angle + 1.0f);
            return _Pitch * 0.5f * (angle * radical + log(angle + radical));
        }

        void vert(inout appdata_full v) { 

            fromStart = -v.vertex.x * _AnglePerUnit ;

            fromOrigin = _StartAngle - fromStart ;

            objectWorldPosition.x = unity_ObjectToWorld._m03_m13_m23.x;

            if (fromStart < _UnrolledAngle) {
                v.normal = float3(0, 1, 0);
                _Glossiness = 0;
            }
            else {
                radiusAtSplit += _Pitch * (_StartAngle - _UnrolledAngle) ;
                radius += _Pitch * fromOrigin + v.vertex.y;

                shifted += fromStart - _UnrolledAngle ;

                v.vertex.y = radiusAtSplit - cos(shifted) * radius ;
                v.vertex.x = -sin(shifted) * radius;

                objectWorldPosition.x -= _UnrolledAngle * 5.07f;

                unity_ObjectToWorld._m03_m13_m23.x = objectWorldPosition.x;

                v.normal = float3(0, cos(shifted), -sin(shifted));
                _Glossiness = 1;
            }
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;

            o.Normal *= sign(dot(IN.viewDir, o.Normal));
        }
        ENDCG
    }
    FallBack "Diffuse"
}