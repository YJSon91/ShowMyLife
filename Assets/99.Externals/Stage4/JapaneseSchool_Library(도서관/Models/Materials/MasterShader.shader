// Shader created with Shader Forge v1.40 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.40;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,cpap:True,lico:1,lgpr:1,limd:3,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:False,rprd:True,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:2865,x:33739,y:32681,varname:node_2865,prsc:2|diff-5182-OUT,diffpow-3624-OUT,spec-630-OUT,gloss-4622-OUT,normal-7190-OUT,difocc-6301-OUT;n:type:ShaderForge.SFN_Multiply,id:6343,x:32184,y:32578,varname:node_6343,prsc:2|A-7736-RGB,B-6665-RGB;n:type:ShaderForge.SFN_Color,id:6665,x:31921,y:32805,ptovrint:True,ptlb:Color,ptin:AlbedoColor_,varname:AlbedoColor_,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Tex2d,id:7736,x:31921,y:32588,ptovrint:True,ptlb:Albedo,ptin:Material_Texture2D_1,varname:Material_Texture2D_1,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:abc00000000008933953923732267454,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:5964,x:32291,y:33358,ptovrint:True,ptlb:Normal,ptin:Material_Texture2D_0,varname:Material_Texture2D_0,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:abc00000000015569947093866672648,ntxv:3,isnm:False;n:type:ShaderForge.SFN_Slider,id:358,x:32169,y:32760,ptovrint:False,ptlb:Metallic,ptin:_Metallic,varname:_Metallic,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Slider,id:1813,x:32158,y:32907,ptovrint:False,ptlb:Gloss,ptin:_Gloss,varname:_Gloss,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:2;n:type:ShaderForge.SFN_OneMinus,id:145,x:32437,y:33083,varname:node_145,prsc:2|IN-8462-G;n:type:ShaderForge.SFN_Multiply,id:4622,x:32655,y:33032,varname:node_4622,prsc:2|A-1813-OUT,B-145-OUT;n:type:ShaderForge.SFN_Multiply,id:630,x:32514,y:32829,varname:node_630,prsc:2|A-358-OUT,B-8462-B;n:type:ShaderForge.SFN_Tex2d,id:8462,x:31921,y:33006,ptovrint:True,ptlb:Masks,ptin:Material_Texture2D_2,varname:Material_Texture2D_2,prsc:1,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:abc00000000014811669424492944334,ntxv:1,isnm:False;n:type:ShaderForge.SFN_Vector1,id:6301,x:32685,y:33191,varname:node_6301,prsc:2,v1:1;n:type:ShaderForge.SFN_Vector1,id:3624,x:32817,y:32681,varname:node_3624,prsc:2,v1:1;n:type:ShaderForge.SFN_OneMinus,id:9756,x:32605,y:33475,varname:node_9756,prsc:2|IN-5964-B;n:type:ShaderForge.SFN_Append,id:8455,x:33092,y:33406,varname:node_8455,prsc:2|A-159-OUT,B-5964-R;n:type:ShaderForge.SFN_Append,id:159,x:32898,y:33360,varname:node_159,prsc:2|A-5964-G,B-9756-OUT;n:type:ShaderForge.SFN_RemapRange,id:7190,x:33324,y:33353,varname:node_7190,prsc:2,frmn:0,frmx:1,tomn:-1,tomx:1|IN-8455-OUT;n:type:ShaderForge.SFN_Vector1,id:9620,x:32497,y:33248,varname:node_9620,prsc:2,v1:0;n:type:ShaderForge.SFN_RgbToHsv,id:7678,x:32438,y:32522,varname:node_7678,prsc:2|IN-6343-OUT;n:type:ShaderForge.SFN_HsvToRgb,id:5182,x:32890,y:32533,varname:node_5182,prsc:2|H-5701-OUT,S-7678-SOUT,V-7678-VOUT;n:type:ShaderForge.SFN_Add,id:5701,x:32619,y:32385,varname:node_5701,prsc:2|A-5268-OUT,B-7678-HOUT;n:type:ShaderForge.SFN_Slider,id:5268,x:32216,y:32310,ptovrint:False,ptlb:Hue,ptin:_Hue,varname:_Hue,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;proporder:7736-5964-8462-6665-358-1813-5268;pass:END;sub:END;*/

Shader "Nimikko/MasterShader" {
    Properties {
        Material_Texture2D_1 ("Albedo", 2D) = "white" {}
        Material_Texture2D_0 ("Normal", 2D) = "bump" {}
        Material_Texture2D_2 ("Masks", 2D) = "gray" {}
        AlbedoColor_ ("Color", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range(0, 1)) = 1
        _Gloss ("Gloss", Range(0, 2)) = 1
        _Hue ("Hue", Range(0, 1)) = 0
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma target 3.0
            uniform sampler2D Material_Texture2D_1; uniform float4 Material_Texture2D_1_ST;
            uniform sampler2D Material_Texture2D_0; uniform float4 Material_Texture2D_0_ST;
            uniform sampler2D Material_Texture2D_2; uniform float4 Material_Texture2D_2_ST;
            UNITY_INSTANCING_BUFFER_START( Props )
                UNITY_DEFINE_INSTANCED_PROP( float4, AlbedoColor_)
                UNITY_DEFINE_INSTANCED_PROP( float, _Metallic)
                UNITY_DEFINE_INSTANCED_PROP( float, _Gloss)
                UNITY_DEFINE_INSTANCED_PROP( float, _Hue)
            UNITY_INSTANCING_BUFFER_END( Props )
            struct VertexInput {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD10;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID( v );
                UNITY_TRANSFER_INSTANCE_ID( v, o );
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #elif UNITY_SHOULD_SAMPLE_SH
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                UNITY_SETUP_INSTANCE_ID( i );
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float4 Material_Texture2D_0_var = tex2D(Material_Texture2D_0,TRANSFORM_TEX(i.uv0, Material_Texture2D_0));
                float3 normalLocal = (float3(float2(Material_Texture2D_0_var.g,(1.0 - Material_Texture2D_0_var.b)),Material_Texture2D_0_var.r)*2.0+-1.0);
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float _Gloss_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Gloss );
                half4 Material_Texture2D_2_var = tex2D(Material_Texture2D_2,TRANSFORM_TEX(i.uv0, Material_Texture2D_2));
                float gloss = (_Gloss_var*(1.0 - Material_Texture2D_2_var.g));
                float perceptualRoughness = 1.0 - (_Gloss_var*(1.0 - Material_Texture2D_2_var.g));
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                    d.ambient = 0;
                    d.lightmapUV = i.ambientOrLightmapUV;
                #else
                    d.ambient = i.ambientOrLightmapUV;
                #endif
                #if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
                    d.boxMin[0] = unity_SpecCube0_BoxMin;
                    d.boxMin[1] = unity_SpecCube1_BoxMin;
                #endif
                #if UNITY_SPECCUBE_BOX_PROJECTION
                    d.boxMax[0] = unity_SpecCube0_BoxMax;
                    d.boxMax[1] = unity_SpecCube1_BoxMax;
                    d.probePosition[0] = unity_SpecCube0_ProbePosition;
                    d.probePosition[1] = unity_SpecCube1_ProbePosition;
                #endif
                d.probeHDR[0] = unity_SpecCube0_HDR;
                d.probeHDR[1] = unity_SpecCube1_HDR;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float _Metallic_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Metallic );
                float3 specularColor = (_Metallic_var*Material_Texture2D_2_var.b);
                float specularMonochrome;
                float _Hue_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Hue );
                float4 Material_Texture2D_1_var = tex2D(Material_Texture2D_1,TRANSFORM_TEX(i.uv0, Material_Texture2D_1));
                float4 AlbedoColor__var = UNITY_ACCESS_INSTANCED_PROP( Props, AlbedoColor_ );
                float3 node_6343 = (Material_Texture2D_1_var.rgb*AlbedoColor__var.rgb);
                float4 node_7678_k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 node_7678_p = lerp(float4(float4(node_6343,0.0).zy, node_7678_k.wz), float4(float4(node_6343,0.0).yz, node_7678_k.xy), step(float4(node_6343,0.0).z, float4(node_6343,0.0).y));
                float4 node_7678_q = lerp(float4(node_7678_p.xyw, float4(node_6343,0.0).x), float4(float4(node_6343,0.0).x, node_7678_p.yzx), step(node_7678_p.x, float4(node_6343,0.0).x));
                float node_7678_d = node_7678_q.x - min(node_7678_q.w, node_7678_q.y);
                float node_7678_e = 1.0e-10;
                float3 node_7678 = float3(abs(node_7678_q.z + (node_7678_q.w - node_7678_q.y) / (6.0 * node_7678_d + node_7678_e)), node_7678_d / (node_7678_q.x + node_7678_e), node_7678_q.x);;
                float3 diffuseColor = (lerp(float3(1,1,1),saturate(3.0*abs(1.0-2.0*frac((_Hue_var+node_7678.r)+float3(0.0,-1.0/3.0,1.0/3.0)))-1),node_7678.g)*node_7678.b); // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                half surfaceReduction;
                #ifdef UNITY_COLORSPACE_GAMMA
                    surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;
                #else
                    surfaceReduction = 1.0/(roughness*roughness + 1.0);
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                half grazingTerm = saturate( gloss + specularMonochrome );
                float3 indirectSpecular = (gi.indirect.specular);
                indirectSpecular *= FresnelLerp (specularColor, grazingTerm, NdotV);
                indirectSpecular *= surfaceReduction;
                float3 specular = (directSpecular + indirectSpecular);
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += gi.indirect.diffuse;
                indirectDiffuse *= 1.0; // Diffuse AO
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma target 3.0
            uniform sampler2D Material_Texture2D_1; uniform float4 Material_Texture2D_1_ST;
            uniform sampler2D Material_Texture2D_0; uniform float4 Material_Texture2D_0_ST;
            uniform sampler2D Material_Texture2D_2; uniform float4 Material_Texture2D_2_ST;
            UNITY_INSTANCING_BUFFER_START( Props )
                UNITY_DEFINE_INSTANCED_PROP( float4, AlbedoColor_)
                UNITY_DEFINE_INSTANCED_PROP( float, _Metallic)
                UNITY_DEFINE_INSTANCED_PROP( float, _Gloss)
                UNITY_DEFINE_INSTANCED_PROP( float, _Hue)
            UNITY_INSTANCING_BUFFER_END( Props )
            struct VertexInput {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID( v );
                UNITY_TRANSFER_INSTANCE_ID( v, o );
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                UNITY_SETUP_INSTANCE_ID( i );
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float4 Material_Texture2D_0_var = tex2D(Material_Texture2D_0,TRANSFORM_TEX(i.uv0, Material_Texture2D_0));
                float3 normalLocal = (float3(float2(Material_Texture2D_0_var.g,(1.0 - Material_Texture2D_0_var.b)),Material_Texture2D_0_var.r)*2.0+-1.0);
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float _Gloss_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Gloss );
                half4 Material_Texture2D_2_var = tex2D(Material_Texture2D_2,TRANSFORM_TEX(i.uv0, Material_Texture2D_2));
                float gloss = (_Gloss_var*(1.0 - Material_Texture2D_2_var.g));
                float perceptualRoughness = 1.0 - (_Gloss_var*(1.0 - Material_Texture2D_2_var.g));
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float _Metallic_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Metallic );
                float3 specularColor = (_Metallic_var*Material_Texture2D_2_var.b);
                float specularMonochrome;
                float _Hue_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Hue );
                float4 Material_Texture2D_1_var = tex2D(Material_Texture2D_1,TRANSFORM_TEX(i.uv0, Material_Texture2D_1));
                float4 AlbedoColor__var = UNITY_ACCESS_INSTANCED_PROP( Props, AlbedoColor_ );
                float3 node_6343 = (Material_Texture2D_1_var.rgb*AlbedoColor__var.rgb);
                float4 node_7678_k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 node_7678_p = lerp(float4(float4(node_6343,0.0).zy, node_7678_k.wz), float4(float4(node_6343,0.0).yz, node_7678_k.xy), step(float4(node_6343,0.0).z, float4(node_6343,0.0).y));
                float4 node_7678_q = lerp(float4(node_7678_p.xyw, float4(node_6343,0.0).x), float4(float4(node_6343,0.0).x, node_7678_p.yzx), step(node_7678_p.x, float4(node_6343,0.0).x));
                float node_7678_d = node_7678_q.x - min(node_7678_q.w, node_7678_q.y);
                float node_7678_e = 1.0e-10;
                float3 node_7678 = float3(abs(node_7678_q.z + (node_7678_q.w - node_7678_q.y) / (6.0 * node_7678_d + node_7678_e)), node_7678_d / (node_7678_q.x + node_7678_e), node_7678_q.x);;
                float3 diffuseColor = (lerp(float3(1,1,1),saturate(3.0*abs(1.0-2.0*frac((_Hue_var+node_7678.r)+float3(0.0,-1.0/3.0,1.0/3.0)))-1),node_7678.g)*node_7678.b); // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "Meta"
            Tags {
                "LightMode"="Meta"
            }
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_META 1
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "UnityMetaPass.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma target 3.0
            uniform sampler2D Material_Texture2D_1; uniform float4 Material_Texture2D_1_ST;
            uniform sampler2D Material_Texture2D_2; uniform float4 Material_Texture2D_2_ST;
            UNITY_INSTANCING_BUFFER_START( Props )
                UNITY_DEFINE_INSTANCED_PROP( float4, AlbedoColor_)
                UNITY_DEFINE_INSTANCED_PROP( float, _Metallic)
                UNITY_DEFINE_INSTANCED_PROP( float, _Gloss)
                UNITY_DEFINE_INSTANCED_PROP( float, _Hue)
            UNITY_INSTANCING_BUFFER_END( Props )
            struct VertexInput {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID( v );
                UNITY_TRANSFER_INSTANCE_ID( v, o );
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID( i );
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
                
                o.Emission = 0;
                
                float _Hue_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Hue );
                float4 Material_Texture2D_1_var = tex2D(Material_Texture2D_1,TRANSFORM_TEX(i.uv0, Material_Texture2D_1));
                float4 AlbedoColor__var = UNITY_ACCESS_INSTANCED_PROP( Props, AlbedoColor_ );
                float3 node_6343 = (Material_Texture2D_1_var.rgb*AlbedoColor__var.rgb);
                float4 node_7678_k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 node_7678_p = lerp(float4(float4(node_6343,0.0).zy, node_7678_k.wz), float4(float4(node_6343,0.0).yz, node_7678_k.xy), step(float4(node_6343,0.0).z, float4(node_6343,0.0).y));
                float4 node_7678_q = lerp(float4(node_7678_p.xyw, float4(node_6343,0.0).x), float4(float4(node_6343,0.0).x, node_7678_p.yzx), step(node_7678_p.x, float4(node_6343,0.0).x));
                float node_7678_d = node_7678_q.x - min(node_7678_q.w, node_7678_q.y);
                float node_7678_e = 1.0e-10;
                float3 node_7678 = float3(abs(node_7678_q.z + (node_7678_q.w - node_7678_q.y) / (6.0 * node_7678_d + node_7678_e)), node_7678_d / (node_7678_q.x + node_7678_e), node_7678_q.x);;
                float3 diffColor = (lerp(float3(1,1,1),saturate(3.0*abs(1.0-2.0*frac((_Hue_var+node_7678.r)+float3(0.0,-1.0/3.0,1.0/3.0)))-1),node_7678.g)*node_7678.b);
                float specularMonochrome;
                float3 specColor;
                float _Metallic_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Metallic );
                half4 Material_Texture2D_2_var = tex2D(Material_Texture2D_2,TRANSFORM_TEX(i.uv0, Material_Texture2D_2));
                diffColor = DiffuseAndSpecularFromMetallic( diffColor, (_Metallic_var*Material_Texture2D_2_var.b), specColor, specularMonochrome );
                float _Gloss_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Gloss );
                float roughness = 1.0 - (_Gloss_var*(1.0 - Material_Texture2D_2_var.g));
                o.Albedo = diffColor + specColor * roughness * roughness * 0.5;
                
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
