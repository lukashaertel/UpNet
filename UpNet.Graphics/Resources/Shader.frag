#version 330 core

uniform vec4 uTint;
uniform sampler2D uTexture;
uniform sampler2D uHeight;
uniform float uHeightScale;
uniform float uBumpScale = 1;

in vec3 vPos;
in vec2 vTexCoord;
in vec3 vNormal;
in vec3 vBitangent;
in vec3 vTangent;
in vec3 vColor;
out vec4 oColor;

const vec3 ambientColor = vec3(0.7, 0.8, 1.0);
const float ambientStrength = 0.02;

const vec3 lightPos = vec3(10.0, 0.0, 4.0);
const vec3 lightColor = vec3(1.0, 0.95, 0.9);

//const vec3 specularColor = vec3(1.0, 0.9, 0.7);
//const float specularStrength = 0.5;
//const float specularExponent = 32;



void main(){
    vec2 size = textureSize(uHeight, 0);
    vec2 dSTdx = vec2(1.0 / size.x, 0);
    vec2 dSTdy = vec2(0, 1.0 / size.y);

    mat3 TBN = mat3(vTangent, vBitangent, vNormal);
    float Hll = uBumpScale * texture(uHeight, vTexCoord).x;
    float dBx = uBumpScale * texture(uHeight, vTexCoord + dSTdx).x - Hll;
    float dBy = uBumpScale * texture(uHeight, vTexCoord + dSTdy).x - Hll;
    vec3 normal = TBN * normalize(vec3(dBy, -dBx, 1.0));

    //    vec3 normal = normalize(vNormal);
    vec3 lightDir = normalize(lightPos - vPos);

    vec3 ambient = ambientColor * ambientStrength;
    vec3 diffuse = lightColor * max(dot(normal, lightDir), 0.0);

    vec3 composed = ambient + diffuse;

    vec4 textureColor = texture(uTexture, vTexCoord);
    oColor = vec4(composed * textureColor.rgb * vColor, textureColor.a) * uTint;
}