Shader "Custom/PlayerShadowProjector"
{
    Properties
    {
        _Color ("Shadow Color", Color) = (0,0,0,0.6)
        _MainTex ("Projector Texture", 2D) = "white" {}
        _FalloffTex ("Falloff Texture", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        
        Pass
        {
            ZWrite Off
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha
            Offset -1, -1
            
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
                float4 texc : TEXCOORD1;
            };
            
            float4x4 unity_Projector;
            float4x4 unity_ProjectorClip;
            
            sampler2D _MainTex;
            sampler2D _FalloffTex;
            float4 _MainTex_ST;
            float4 _FalloffTex_ST;
            float4 _Color;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.texc = mul(unity_Projector, v.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Projector 좌표계로 변환
                float4 texc = i.texc;
                texc.xy = texc.xy / texc.w;
                
                // Projector 범위 내에 있는지 확인
                if (texc.x < 0 || texc.x > 1 || texc.y < 0 || texc.y > 1)
                    discard;
                
                // 메인 텍스처 샘플링
                fixed4 col = tex2D(_MainTex, texc.xy);
                
                // Falloff 텍스처로 가장자리 페이드
                fixed4 falloff = tex2D(_FalloffTex, texc.xy);
                
                // 최종 색상 계산
                fixed4 finalColor = _Color * col * falloff;
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    Fallback "Transparent/VertexLit"
}
