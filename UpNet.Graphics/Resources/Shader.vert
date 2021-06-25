#version 330 core

uniform mat4 uModel;
uniform mat4 uPV;
uniform sampler2D uHeight;
uniform float uHeightScale;


in vec3 aPosition;
in vec2 aTexCoord;
in vec3 aNormal;
in vec3 aBitangent;
in vec3 aTangent;
in vec3 aColor;

out vec3 vPos;
out vec2 vTexCoord;
out vec3 vNormal;
out vec3 vBitangent;
out vec3 vTangent;
out vec3 vColor;


void main(){
    vec3 displaced = aPosition + aNormal * textureLod(uHeight, aTexCoord, 0).r * uHeightScale;

    gl_Position = uPV * uModel * vec4(displaced, 1.0);
    vPos = vec3(uModel * vec4(displaced, 1.0));
    vTexCoord = aTexCoord;
    vNormal = vec3(uModel * vec4(aNormal, 0.0));
    vBitangent = vec3(uModel * vec4(aBitangent, 0.0));
    vTangent = vec3(uModel * vec4(aTangent, 0.0));
    vColor =  aColor;
}