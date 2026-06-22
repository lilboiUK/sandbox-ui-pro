FEATURES
{
    #include "common/features.hlsl"
}

MODES
{
    Forward();
    Depth();
}

COMMON
{
	#include "common/shared.hlsl"
}


struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	float2 vQuadUv : TEXCOORD14;
	float2 vScreenPos : TEXCOORD15;
};

VS
{
    #include "common/vertex.hlsl"

    float4 uNodeRect < Attribute( "NodeRect" ); >;
    float4 uNodeScreenRect < Attribute( "NodeScreenRect" ); >;

    PixelInput MainVs( VertexInput i )
    {
        PixelInput o = (PixelInput)0;

        float2 origin = float2( uNodeRect.x, uNodeRect.y - uNodeRect.w );
        float2 ndc = origin + i.vPositionOs.xy * uNodeRect.zw;

        o.vPositionPs = float4( ndc, 0.0, 1.0 );
        o.vQuadUv = i.vPositionOs.xy;
        o.vScreenPos = uNodeScreenRect.xy + float2( i.vPositionOs.x, 1.0 - i.vPositionOs.y ) * uNodeScreenRect.zw;
        return o;
    }
}

PS
{
    #include "common/pixel.hlsl"

    RenderState( CullMode, NONE );
    RenderState( BlendEnable, true );
    RenderState( SrcBlend, SRC_ALPHA );
    RenderState( DstBlend, INV_SRC_ALPHA );
    RenderState( SrcBlendAlpha, ONE );
    RenderState( DstBlendAlpha, INV_SRC_ALPHA );
    RenderState( BlendOp, ADD );
    RenderState( DepthEnable, false );
    RenderState( DepthWriteEnable, false );

    float4 uTint        < Attribute( "Tint" ); >;
    float4 uBorderColor < Attribute( "BorderColor" ); >;

    Texture2D    uTexture < Attribute( "Texture" ); >;
    SamplerState uSampler < Filter( Linear ); AddressU( CLAMP ); AddressV( CLAMP ); >;
    float2 uUvScale  < Attribute( "UvScale" );  Default2( 1.0, 1.0 ); >;
    float2 uUvOffset < Attribute( "UvOffset" ); Default2( 0.0, 0.0 ); >;

    float2 uNodeSize     < Attribute( "NodeSize" ); >;
    float  uCornerRadius < Attribute( "CornerRadius" ); >;
    float  uBorderWidth  < Attribute( "BorderWidth" ); >;

    float4 uClipRect < Attribute("ClipRect"); >;
    float uClipRadius < Attribute("ClipRadius"); >;

    float SdfRoundedBox( float2 p, float2 b, float r )
    {
        float2 q = abs( p ) - b + r;
        return min( max( q.x, q.y ), 0.0 ) + length( max( q, 0.0 ) ) - r;
    }

    float4 MainPs( PixelInput i ) : SV_Target0
    {
        float2 rectSize = uNodeSize;
        float  radius = uCornerRadius;
        float  borderWidth = uBorderWidth;

        float2 halfSize = rectSize * 0.5;

        radius = clamp( radius, 0.0, min( halfSize.x, halfSize.y ) );

        float2 pixelOffset = ( i.vQuadUv - 0.5 ) * rectSize;

        float signedDist = SdfRoundedBox( pixelOffset, halfSize, radius );

        float antiAliasWidth = max( fwidth( signedDist ), 0.0001 );

        float outerAlpha = 1.0 - smoothstep( -antiAliasWidth, antiAliasWidth, signedDist );

        float fillFactor = ( borderWidth > 0.0 )
            ? 1.0 - smoothstep( -borderWidth - antiAliasWidth, -borderWidth + antiAliasWidth, signedDist )
            : 1.0;

        float2 texUv = float2( i.vQuadUv.x, 1.0 - i.vQuadUv.y );
        texUv += uUvOffset;
        texUv *= uUvScale;

        float4 fillColor = Tex2DS( uTexture, uSampler, texUv ) * uTint;

        float2 inside = step( 0.0, texUv ) * step( texUv, 1.0 );
        fillColor.a *= inside.x * inside.y;

        float4 outputColor = lerp( uBorderColor, fillColor, fillFactor );
        outputColor.a *= outerAlpha;

        float2 clipHalf = uClipRect.zw * 0.5;
        float2 clipCenter = uClipRect.xy + clipHalf;
        float  clipRadius = clamp( uClipRadius, 0.0, min( clipHalf.x, clipHalf.y ) );

        float2 clipOffset = i.vScreenPos - clipCenter;
        float  clipDist = SdfRoundedBox( clipOffset, clipHalf, clipRadius );

        float clipAaWidth = max( fwidth( clipDist ), 0.0001 );
        float clipAlpha = 1.0 - smoothstep( -clipAaWidth, clipAaWidth, clipDist );

        outputColor.a *= clipAlpha;

        return outputColor;
    }
}