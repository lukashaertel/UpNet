#version 330 core

in vec4 vColor;
in vec2 vTexCoord;
out vec4 oColor;

uniform sampler2D uTexture;
uniform float uDistanceFactor;

uniform vec4 uColor = vec4(1.0);
uniform float uFontWeight = 0.0;

uniform float uShadowClipped = 0.0;
uniform vec4 uShadowColor = vec4(0.0);
uniform vec2 uShadowOffset = vec2(0.2, 0.2);
uniform float uShadowSmoothing = 0.0;

uniform vec4 uInnerShadowColor = vec4(0.0);
uniform float uInnerShadowRange = 0.0;


float median(float r, float g, float b) {
    return max(min(r, g), min(max(r, g), b));
}

vec4 blend(vec4 src, vec4 dst, float alpha) {
    // src OVER dst porter-duff blending
    float a = src.a + dst.a * (1.0 - src.a);
    vec3 rgb = (src.a * src.rgb + dst.a * dst.rgb * (1.0 - src.a)) / (a == 0.0 ? 1.0 : a);
    return vec4(rgb, a * alpha);
}

float linearstep(float a, float b, float x) {
    return clamp((x - a) / (b - a), 0.0, 1.0);
}

void main() {
    vec4 color = uColor * vec4(vColor.rgb, 1.0);
    
    // Glyph
    vec2 size = textureSize(uTexture, 0);
    vec4 msdf = texture(uTexture, vTexCoord);
    float distance = uDistanceFactor * (median(msdf.r, msdf.g, msdf.b) + uFontWeight - 0.5);
    float glyphAlpha = clamp(distance + 0.5, 0.0, 1.0);
    vec4 glyph = vec4(color.rgb, glyphAlpha * color.a);

    // Shadow
    distance = texture(uTexture, vTexCoord - uShadowOffset / size).a + uFontWeight;
    float shadowAlpha = linearstep(0.5 - uShadowSmoothing, 0.5 + uShadowSmoothing, distance) * uShadowColor.a;
    shadowAlpha *= 1.0 - glyphAlpha * uShadowClipped;
    vec4 shadow = vec4(uShadowColor.rgb, shadowAlpha);

    // Inner shadow
    distance = msdf.a + uFontWeight;
    float innerShadowAlpha = linearstep(0.5 + uInnerShadowRange, 0.5, distance) * uInnerShadowColor.a * glyphAlpha;
    vec4 innerShadow = vec4(uInnerShadowColor.rgb, innerShadowAlpha);

    oColor = blend(blend(innerShadow, glyph, 1.0), shadow, vColor.a);
}