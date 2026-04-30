// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SH_WaterFlow"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_MaskClipValue( "Mask Clip Value", Float ) = 0.5
		_Occlusion("Occlusion", Float) = 0.2
		_WaterNormal("Water Normal", 2D) = "bump" {}
		_Normal01Scale("Normal 01 Scale", Float) = 0
		_Normal02Scale("Normal 02 Scale", Float) = 0
		_WaterDepth("Water Depth", Float) = 0
		_WaterFalloff("Water Falloff", Float) = 0
		_Refraction("Refraction", Float) = 0.2
		_Opacity("Opacity", Float) = 0.6
		_Transluceny("Transluceny", Float) = 0.2
		_WaterColor("Water Color", Color) = (0.2977941,0.6751521,0.75,1)
		_Distortion("Distortion", Float) = 0.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[Header(Translucency)]
		_Translucency("Strength", Range( 0 , 50)) = 1
		_TransNormalDistortion("Normal Distortion", Range( 0 , 1)) = 0.1
		_TransScattering("Scaterring Falloff", Range( 1 , 50)) = 2
		_TransDirect("Direct", Range( 0 , 1)) = 1
		_TransAmbient("Ambient", Range( 0 , 1)) = 0.2
		_TransShadow("Shadow", Range( 0 , 1)) = 0.9
		[Header(Refraction)]
		_ChromaticAberration("Chromatic Aberration", Range( 0 , 0.3)) = 0.1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Transparent+0" }
		Cull Back
		ZTest LEqual
		GrabPass{ "RefractionGrab0" }
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#pragma target 3.0
		#pragma multi_compile _ALPHAPREMULTIPLY_ON
		#pragma surface surf StandardSpecularCustom keepalpha finalcolor:RefractionF  
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
			float3 worldPos;
		};

		struct SurfaceOutputStandardSpecularCustom
		{
			fixed3 Albedo;
			fixed3 Normal;
			half3 Emission;
			fixed3 Specular;
			half Smoothness;
			half Occlusion;
			fixed Alpha;
			fixed3 Translucency;
		};

		uniform half _Normal01Scale;
		uniform sampler2D _WaterNormal;
		uniform float4 _WaterNormal_ST;
		uniform half _Normal02Scale;
		uniform half4 _WaterColor;
		uniform sampler2D WaterGrab;
		uniform half _Distortion;
		uniform sampler2D _CameraDepthTexture;
		uniform half _WaterDepth;
		uniform half _WaterFalloff;
		uniform half _Occlusion;
		uniform half _Translucency;
		uniform half _TransNormalDistortion;
		uniform half _TransScattering;
		uniform half _TransDirect;
		uniform half _TransAmbient;
		uniform half _TransShadow;
		uniform half _Transluceny;
		uniform half _Opacity;
		uniform sampler2D RefractionGrab0;
		uniform float _ChromaticAberration;
		uniform half _Refraction;
		uniform float _MaskClipValue = 0.5;

		inline half4 LightingStandardSpecularCustom(SurfaceOutputStandardSpecularCustom s, half3 viewDir, UnityGI gi )
		{
			#if !DIRECTIONAL
			float3 lightAtten = gi.light.color;
			#else
			float3 lightAtten = lerp( _LightColor0, gi.light.color, _TransShadow );
			#endif
			half3 lightDir = gi.light.dir + s.Normal * _TransNormalDistortion;
			half transVdotL = pow( saturate( dot( viewDir, -lightDir ) ), _TransScattering );
			half3 translucency = lightAtten * (transVdotL * _TransDirect + gi.indirect.diffuse * _TransAmbient) * s.Translucency;
			half4 c = half4( s.Albedo * translucency * _Translucency, 0 );

			SurfaceOutputStandardSpecular r;
			r.Albedo = s.Albedo;
			r.Normal = s.Normal;
			r.Emission = s.Emission;
			r.Specular = s.Specular;
			r.Smoothness = s.Smoothness;
			r.Occlusion = s.Occlusion;
			r.Alpha = s.Alpha;
			return LightingStandardSpecular (r, viewDir, gi) + c;
		}

		inline void LightingStandardSpecularCustom_GI(SurfaceOutputStandardSpecularCustom s, UnityGIInput data, inout UnityGI gi )
		{
			UNITY_GI(gi, s, data);
		}

		inline float4 Refraction( Input i, SurfaceOutputStandardSpecularCustom o, float indexOfRefraction, float chomaticAberration ) {
			float3 worldNormal = o.Normal;
			float4 screenPos = i.screenPos;
			#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
			#else
				float scale = 1.0;
			#endif
			float halfPosW = screenPos.w * 0.5;
			screenPos.y = ( screenPos.y - halfPosW ) * _ProjectionParams.x * scale + halfPosW;
			#if SHADER_API_D3D9 || SHADER_API_D3D11
				screenPos.w += 0.0000001;
			#endif
			float2 projScreenPos = ( screenPos / screenPos.w ).xy;
			float3 worldViewDir = normalize( UnityWorldSpaceViewDir( i.worldPos ) );
			float3 refractionOffset = ( ( ( ( indexOfRefraction - 1.0 ) * mul( UNITY_MATRIX_V, float4( worldNormal, 0.0 ) ) ) * ( 1.0 / ( screenPos.z + 1.0 ) ) ) * ( 1.0 - dot( worldNormal, worldViewDir ) ) );
			float2 cameraRefraction = float2( refractionOffset.x, -( refractionOffset.y * _ProjectionParams.x ) );
			float4 redAlpha = tex2D( RefractionGrab0, ( projScreenPos + cameraRefraction ) );
			float green = tex2D( RefractionGrab0, ( projScreenPos + ( cameraRefraction * ( 1.0 - chomaticAberration ) ) ) ).g;
			float blue = tex2D( RefractionGrab0, ( projScreenPos + ( cameraRefraction * ( 1.0 + chomaticAberration ) ) ) ).b;
			return float4( redAlpha.r, green, blue, redAlpha.a );
		}

		void RefractionF( Input i, SurfaceOutputStandardSpecularCustom o, inout fixed4 color )
		{
			#ifdef UNITY_PASS_FORWARDBASE
				color.rgb = color.rgb + Refraction( i, o, _Refraction, _ChromaticAberration ) * ( 1 - color.a );
				color.a = 1;
			#endif
		}

		void surf( Input i , inout SurfaceOutputStandardSpecularCustom o )
		{
			float2 uv_WaterNormal = i.uv_texcoord * _WaterNormal_ST.xy + _WaterNormal_ST.zw;
			float3 temp_output_24_0 = BlendNormals( UnpackScaleNormal( tex2D( _WaterNormal,(abs( uv_WaterNormal+_Time[1] * float2(-0.3,0.5 )))) ,_Normal01Scale ) , UnpackScaleNormal( tex2D( _WaterNormal,(abs( uv_WaterNormal+_Time[1] * float2(0.04,1.6 )))) ,_Normal02Scale ) );
			o.Normal = temp_output_24_0;
			float2 appendResult67 = float2( i.screenPos.x , i.screenPos.y );
			float eyeDepth1 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD(i.screenPos))));
			o.Albedo = lerp( _WaterColor , tex2D( WaterGrab, ( half3( ( appendResult67 / i.screenPos.w ) ,  0.0 ) + ( temp_output_24_0 * _Distortion ) ).xy ) , saturate( pow( ( abs( ( eyeDepth1 - i.screenPos.w ) ) + _WaterDepth ) , _WaterFalloff ) ) ).rgb;
			o.Occlusion = _Occlusion;
			half3 temp_cast_3 = (_Transluceny).xxx;
			o.Translucency = temp_cast_3;
			o.Alpha = _Opacity;
			o.Normal = o.Normal + 0.00001 * i.screenPos * i.worldPos;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=7003
734;413;1632;970;280.3058;1888.084;2.2;True;True
Node;AmplifyShaderEditor.CommentaryNode;152;-946.8988,-555.6;Float;False;828.5967;315.5001;Screen depth difference to get intersection and fading effect with terrain and obejcts;5;89;3;1;2;158;
Node;AmplifyShaderEditor.CommentaryNode;151;-935.9057,-1082.484;Float;False;1281.603;457.1994;Blend panning normals to fake noving ripples;8;19;23;24;21;22;17;48;168;
Node;AmplifyShaderEditor.ScreenPosInputsNode;2;-898.4998,-436.7993;Float;False;1;False;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.TextureCoordinatesNode;21;-885.9058,-1005.185;Float;False;0;17;2;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;FLOAT2;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ScreenDepthNode;1;-693.4999,-490.2993;Float;False;0;0;FLOAT4;0,0,0,0;False;FLOAT
Node;AmplifyShaderEditor.PannerNode;22;-613.2062,-1032.484;Float;False;-0.3;0.5;0;FLOAT2;0,0;False;1;FLOAT;0.0;False;FLOAT2
Node;AmplifyShaderEditor.PannerNode;19;-614.9061,-825.9849;Float;False;0.04;1.6;0;FLOAT2;0,0;False;1;FLOAT;0.0;False;FLOAT2
Node;AmplifyShaderEditor.SimpleSubtractOpNode;3;-469.1001,-393.9992;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;48;-617.3063,-927.3858;Float;False;Property;_Normal01Scale;Normal 01 Scale;2;0;0;0;0;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;168;-601.8059,-683.9839;Float;False;Property;_Normal02Scale;Normal 02 Scale;2;0;0;0;0;FLOAT
Node;AmplifyShaderEditor.AbsOpNode;89;-283.904,-396.1832;Float;False;0;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.SamplerNode;17;-267.3054,-827.2839;Float;True;Property;_WaterNormal;Water Normal;1;0;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;0;SAMPLER2D;0,0;False;1;FLOAT2;1.0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;23;-269.2061,-1024.185;Float;True;Property;_Normal2;Normal2;-1;0;None;True;0;True;bump;Auto;True;Instance;17;Auto;Texture2D;0;SAMPLER2D;0,0;False;1;FLOAT2;1.0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.CommentaryNode;150;467.1957,-1501.783;Float;False;985.6011;418.6005;Get screen color for refraction and disturbe it with normals;8;67;96;66;68;97;98;65;149;
Node;AmplifyShaderEditor.CommentaryNode;159;-59.60013,-555.2;Float;False;1113.201;508.3005;Depths controls and colors;5;94;87;10;88;6;
Node;AmplifyShaderEditor.BlendNormalsNode;24;124.697,-923.8847;Float;False;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;FLOAT3
Node;AmplifyShaderEditor.WireNode;158;-154.2079,-377.2831;Float;False;0;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.ScreenPosInputsNode;66;517.1959,-1451.783;Float;False;1;False;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;6;64.79953,-276.8994;Float;False;Property;_WaterDepth;Water Depth;5;0;0;0;0;FLOAT
Node;AmplifyShaderEditor.WireNode;149;487.4943,-1188.882;Float;False;0;FLOAT3;0.0,0,0;False;FLOAT3
Node;AmplifyShaderEditor.AppendNode;67;712.195,-1440.384;Float;False;FLOAT2;0;0;0;0;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;FLOAT2
Node;AmplifyShaderEditor.RangedFloatNode;97;710.096,-1203.183;Float;False;Property;_Distortion;Distortion;9;0;0.5;0;0;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;88;290.1941,-434.9824;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;10;285.9994,-208.9;Float;False;Property;_WaterFalloff;Water Falloff;6;0;0;0;0;FLOAT
Node;AmplifyShaderEditor.PowerNode;87;483.4936,-378.9829;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.SimpleDivideOpNode;68;885.8966,-1387.683;Float;False;0;FLOAT2;0.0,0;False;1;FLOAT;0,0;False;FLOAT2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;98;888.1974,-1279.783;Float;False;0;FLOAT3;0.0,0,0;False;1;FLOAT;0,0,0;False;FLOAT3
Node;AmplifyShaderEditor.SaturateNode;94;709.0955,-383.8837;Float;False;0;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;96;1041.296,-1346.683;Float;False;0;FLOAT2;0.0,0;False;1;FLOAT3;0,0;False;FLOAT3
Node;AmplifyShaderEditor.ColorNode;163;1246.093,-1050.982;Float;False;Property;_WaterColor;Water Color;7;0;0.2977941,0.6751521,0.75,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ScreenColorNode;65;1232.797,-1350.483;Float;False;Global;WaterGrab;WaterGrab;-1;0;Object;-1;0;FLOAT2;0,0;False;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.WireNode;162;1139.392,-763.2817;Float;False;0;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;167;1214.396,-441.184;Float;False;Property;_Opacity;Opacity;7;0;0.6;0;0;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;165;1198.396,-613.184;Float;False;Property;_Transluceny;Transluceny;7;0;0.2;0;0;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;103;1198.795,-692.9843;Float;False;Property;_Occlusion;Occlusion;-1;0;0.2;0;0;FLOAT
Node;AmplifyShaderEditor.LerpOp;93;1688.996,-989.6854;Float;False;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0.0;False;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;166;1206.396,-523.184;Float;False;Property;_Refraction;Refraction;7;0;0.2;0;0;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1862.601,-743.1998;Half;False;True;2;Half;ASEMaterialInspector;0;StandardSpecular;SH_WaterFlow;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;3;False;0;0;Custom;0.5;True;False;0;True;Opaque;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;Relative;0;;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0.0,0,0;False;7;FLOAT3;0.0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0.0,0,0;False;12;FLOAT3;0,0,0;False;13;OBJECT;0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False
WireConnection;1;0;2;0
WireConnection;22;0;21;0
WireConnection;19;0;21;0
WireConnection;3;0;1;0
WireConnection;3;1;2;4
WireConnection;89;0;3;0
WireConnection;17;1;19;0
WireConnection;17;5;168;0
WireConnection;23;1;22;0
WireConnection;23;5;48;0
WireConnection;24;0;23;0
WireConnection;24;1;17;0
WireConnection;158;0;89;0
WireConnection;149;0;24;0
WireConnection;67;0;66;1
WireConnection;67;1;66;2
WireConnection;88;0;158;0
WireConnection;88;1;6;0
WireConnection;87;0;88;0
WireConnection;87;1;10;0
WireConnection;68;0;67;0
WireConnection;68;1;66;4
WireConnection;98;0;149;0
WireConnection;98;1;97;0
WireConnection;94;0;87;0
WireConnection;96;0;68;0
WireConnection;96;1;98;0
WireConnection;65;0;96;0
WireConnection;162;0;94;0
WireConnection;93;0;163;0
WireConnection;93;1;65;0
WireConnection;93;2;162;0
WireConnection;0;0;93;0
WireConnection;0;1;24;0
WireConnection;0;5;103;0
WireConnection;0;7;165;0
WireConnection;0;8;166;0
WireConnection;0;9;167;0
ASEEND*/
//CHKSM=DB3A8D58F9D554781EE40E96230D5BA9333BEF1C