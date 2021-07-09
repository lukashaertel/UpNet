#version 330 core

uniform vec4 uTint;
uniform mat4 uSpline;
uniform sampler2D uNoise;
in vec3 vPos;
in vec3 vColor;
out vec4 oColor;

float bx(float x, float center, float intensity){
    return max(0.0, 1.0 - pow(intensity * (x-center), 12.0));
}

void main(){
    vec4 textureColor = texture(uNoise, (vPos.xy + vPos.xz + vPos.xz) * 0.02);
    vec3 isp = (uSpline * vec4(vPos, 1.0)).xyz + textureColor.xyz * 0.3;
    float m = bx(isp.x, 1, 8) +
    bx(isp.x, -1, 8) +
    bx(isp.z, 1, 8) +
    bx(isp.z, -1, 8);
    oColor = vec4(m, m, m, 1) + uTint * 0.001;
}