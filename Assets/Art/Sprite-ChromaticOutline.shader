Shader "Sprites/Chromatic Outline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		
        // Add values to determine if outlining is enabled and outline color.
        [PerRendererData] _Outline ("Outline", Float) = 0
        [PerRendererData] _OutlineColor("Outline Color", Color) = (1,1,1,1)
		
		// Value for the reality fade effect. Zero means it dissapears
		[PerRendererData] _FadeoutValue ("Reality Fade", Range (0.0, 1.0)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma shader_feature ETC1_EXTERNAL_ALPHA
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
            };

            fixed4 _Color;
            float _Outline;
            fixed4 _OutlineColor;
			float _FadeoutValue;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                return OUT;
            }

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            float4 _MainTex_TexelSize;

            fixed4 SampleSpriteTexture (float2 uv)
            {
                fixed4 color = tex2D (_MainTex, uv);

                #if ETC1_EXTERNAL_ALPHA
                // get the color from an external texture (usecase: Alpha support for ETC1 on android)
                color.a = tex2D (_AlphaTex, uv).r;
                #endif //ETC1_EXTERNAL_ALPHA

                return color;
            }
			
			float hash (float2 co)
			{
				return frac(sin(dot(co.xy, fixed2(12.9898,78.233))) * 43758.5453);
			}

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;

                // If outline is enabled and there is a pixel, try to draw an outline.
                if (_Outline > 0)// && c.a != 0)
				{
					fixed splitStrength = _OutlineColor.r + _FadeoutValue * 2.0;
				
                    // Get the neighbouring four pixels.
                    fixed4 pixelUp = tex2D(_MainTex, IN.texcoord + fixed2(0, _MainTex_TexelSize.y));
                    fixed4 pixelDown = tex2D(_MainTex, IN.texcoord - fixed2(0, _MainTex_TexelSize.y));
                    fixed4 pixelRight = tex2D(_MainTex, IN.texcoord + fixed2(_MainTex_TexelSize.x, 0));
                    fixed4 pixelLeft = tex2D(_MainTex, IN.texcoord - fixed2(_MainTex_TexelSize.x, 0));

                    // If one of the neighbouring pixels is invisible, we render an outline.
                    /*if (pixelUp.a * pixelDown.a * pixelRight.a * pixelLeft.a == 0)
					{
						fixed4 targetColor = fixed4(pixelLeft.a, pixelRight.a, pixelRight.a, 1);
                        c.rgba = lerp( c.rgba, targetColor, _OutlineColor.r );
                    }
					// We render chromatic abberation
					else*/
					{
						float wildnessSplit = floor( max( 0, splitStrength - 0.5 ) * 2.0 ) * _MainTex_TexelSize.x;
					
						// Build coordinates used as source for glitch
						float2 samplingOffset = _MainTex_TexelSize * round( 2.0 * splitStrength );
						float2 glitchCoord = IN.texcoord.xy;
						// Make it pixel perfect for the random hash
						glitchCoord.x = floor(glitchCoord.x / _MainTex_TexelSize.x) * _MainTex_TexelSize.x;
						glitchCoord.y = floor(glitchCoord.y / _MainTex_TexelSize.y) * _MainTex_TexelSize.y;
						glitchCoord.y += cos( glitchCoord.y + _SinTime.w * 4.5 );
						// Generate glitch coordinates
						float glitchRaw = hash( glitchCoord.yy );
						// Make it pixel perfect
						glitchRaw = floor(glitchRaw / _MainTex_TexelSize.x) * _MainTex_TexelSize.x;
						// Perform the offset
						samplingOffset.x *= glitchRaw * cos( glitchCoord.y + _CosTime.w * 6.5 ) * ( wildnessSplit + 0.15 ) * 4.0;
						// Perform hover split
						samplingOffset.x += wildnessSplit;
						
						// Create wild split
						float offsetDupe = hash( glitchCoord.yy * 1.1 );
						offsetDupe *= _MainTex_TexelSize.x * wildnessSplit * 8.0;
						offsetDupe *= cos( glitchCoord.y + _SinTime.w * 5.5 );
					
						// Grab the aberration left and right for the effect
						fixed offsetLeft  = samplingOffset.x - offsetDupe.x;
						fixed offsetRight = samplingOffset.x + offsetDupe.x;
						fixed4 pixelFuckLeft = tex2D(_MainTex, IN.texcoord - fixed2(offsetLeft, 0));
						fixed4 pixelFuckRight = tex2D(_MainTex, IN.texcoord + fixed2(offsetRight, 0));
					
						c.r  = pixelFuckLeft.r   * min( 1.4, max( 1.0, offsetLeft  / _MainTex_TexelSize.x ));
						c.gb = pixelFuckRight.gb * min( 1.4, max( 1.0, offsetRight / _MainTex_TexelSize.x ));
						c.a  = min( 1.0, (pixelFuckLeft.a + pixelFuckRight.a)*0.707 ) * ( c.a + 0.5 ) / 1.5;
						
						// If one of the neighbouring pixels is invisible, we render an outline.
						if (pixelUp.a * pixelDown.a * pixelRight.a * pixelLeft.a * c.a == 0)
						{
							c.rgb *= 1.414;
						}
					}
					
					{
						float glitchCoord = IN.texcoord.y;
						glitchCoord = floor(glitchCoord / _MainTex_TexelSize.y) * _MainTex_TexelSize.y;
						glitchCoord += cos( glitchCoord + _SinTime.w * 4.5 );
						
						float glitchRaw = hash( glitchCoord );
						glitchRaw += max( 0, sin( glitchCoord * 4.5 + _CosTime.w * 3.5 ));
						glitchRaw = abs(glitchRaw * 0.5);
						
						// Drop pixels
						if ( glitchRaw < _FadeoutValue )
						{
							c.a *= glitchRaw * (1 - _FadeoutValue);
							c.rgb *= 1.0 + (glitchRaw * _FadeoutValue * 20.0);
						}
					}
                }
				
				// Normal alpha blending
                c.rgb *= c.a;

                return c;
            }
            ENDCG
        }
    }
}