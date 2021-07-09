#version 330 core

uniform mat4 uModel;
uniform mat4 uPV;
uniform sampler2D uHeight;
uniform float uHeightScale;
uniform float uHeightOffset;


in vec3 aPosition;
in vec2 aTexCoord;
in vec3 aNormal;
in vec3 aColor;

out vec3 vPos;
out vec3 vColor;


void main(){
    vec3 displaced = aPosition + aNormal * (textureLod(uHeight, aTexCoord, 0).r + uHeightOffset) * uHeightScale;

    gl_Position = uPV * uModel * vec4(displaced, 1.0);
    vPos = vec3(uModel * vec4(displaced, 1.0));
    vColor =  aColor;
}