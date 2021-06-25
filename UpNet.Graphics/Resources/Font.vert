#version 330 core

uniform mat4 uModel;
uniform mat4 uPV;

in vec3 aPosition;
in vec2 aTexCoord;
in vec4 aColor;
out vec2 vTexCoord;
out vec4 vColor;

void main() {
    gl_Position = uPV * uModel * vec4(aPosition, 1.0);
    vTexCoord = aTexCoord;
    vColor = aColor;
}