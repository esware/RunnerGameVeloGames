Shader "Custom/AdvancedCurvedWorld" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _BendAmount ("Bend Amount", Vector) = (0.1,0.1,0.1,0)
        _BendAxis ("Bend Axis", Float) = 0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _BendAmount;
            float _BendAxis;

            v2f vert (appdata v) {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // Eğimli dünya hesaplaması
                float angleX = v.vertex.x * _BendAmount.x;
                float angleY = v.vertex.y * _BendAmount.y;
                float angleZ = v.vertex.z * _BendAmount.z;

                // Eksen seçimine bağlı olarak vertex pozisyonunu ayarlama
                if (_BendAxis == 0) { // X Ekseni
                    v.vertex.y += sin(angleX);
                    v.vertex.z += cos(angleX);
                } else if (_BendAxis == 1) { // Y Ekseni
                    v.vertex.x += sin(angleY);
                    v.vertex.z += cos(angleY);
                } else if (_BendAxis == 2) { // Z Ekseni
                    v.vertex.x += sin(angleZ);
                    v.vertex.y += cos(angleZ);
                }

                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
