#version 310 es
precision highp float;

layout (std140, binding = 1) uniform Matrices {
  mat4 modelMatrix;
  mat4 viewMatrix;
  mat4 projectionMatrix;
  
  mat4 boneMatrices[16];  
};

struct Light {
  // 0x00 (vec3 needs to be 16-byte aligned)
  vec3 position;
  bool enabled;

  // 0x10 (vec3 needs to be 16-byte aligned)
  vec3 normal;
  int sourceType;

  // 0x20 (vec4 needs to be 16-byte aligned)
  vec4 color;
  
  // 0x30 (vec3 needs to be 16-byte aligned)
  vec3 cosineAttenuation;
  int diffuseFunction;

  // 0x40 (vec3 needs to be 16-byte aligned)
  vec3 distanceAttenuation;
  int attenuationFunction;
};

layout (std140, binding = 2) uniform Lights {
  Light lights[8];
  vec4 ambientLightColor;
  float useLighting;
};

uniform vec3 cameraPosition;
uniform float shininess;

struct Texture {
  sampler2D sampler;
  mat3x2 transform2d;
};

uniform sampler2D texture0;
uniform Texture texture2;
uniform Texture texture3;
uniform sampler2D normalTexture;
uniform vec3 color_GxMaterialColor0;
uniform vec3 color_GxAmbientColor0;
uniform float scalar_GxAlphaRegister0;

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 uv0;

out vec4 fragColor;

void getSurfaceToLightNormalAndAttenuation(Light light, vec3 position, vec3 normal, out vec3 surfaceToLightNormal, out float attenuation) {
  vec3 surfaceToLight = light.position - position;
  
  surfaceToLightNormal = (light.sourceType == 3)
    ? -light.normal : normalize(surfaceToLight);

  if (light.attenuationFunction == 0) {
    attenuation = 1.0;
    return;
  }

  // Attenuation is calculated as a fraction, (cosine attenuation) / (distance attenuation).

  // Numerator (Cosine attenuation)
  vec3 cosAttn = light.cosineAttenuation;
  
  vec3 attnDotLhs = (light.attenuationFunction == 1)
    ? normal : surfaceToLightNormal;
  float attn = dot(attnDotLhs, light.normal);
  vec3 attnPowers = vec3(1, attn, attn*attn);

  float attenuationNumerator = max(0.0, dot(cosAttn, attnPowers));

  // Denominator (Distance attenuation)
  float attenuationDenominator = 1.0;
  if (light.sourceType != 3) {
    vec3 distAttn = light.distanceAttenuation;
    
    if (light.attenuationFunction == 1) {
      float attn = max(0.0, -dot(normal, light.normal));
      if (light.diffuseFunction != 0) {
        distAttn = normalize(distAttn);
      }
      
      attenuationDenominator = dot(distAttn, attnPowers);
    } else {
      float dist2 = dot(surfaceToLight, surfaceToLight);
      float dist = sqrt(dist2);
      attenuationDenominator = dot(distAttn, vec3(1, dist, dist2));
    }
  }

  attenuation = attenuationNumerator / attenuationDenominator;
}

void getIndividualLightColors(Light light, vec3 position, vec3 normal, float shininess, out vec4 diffuseColor, out vec4 specularColor) {
  if (!light.enabled) {
     diffuseColor = specularColor = vec4(0);
     return;
  }

  vec3 surfaceToLightNormal = vec3(0);
  float attenuation = 0.0;
  getSurfaceToLightNormalAndAttenuation(light, position, normal, surfaceToLightNormal, attenuation);

  float diffuseLightAmount = 1.0;
  if (light.diffuseFunction == 1 || light.diffuseFunction == 2) {
    diffuseLightAmount = max(0.0, dot(normal, surfaceToLightNormal));
  }
  diffuseColor = light.color * diffuseLightAmount * attenuation;
  
  if (light.attenuationFunction != 0 && dot(normal, surfaceToLightNormal) >= 0.0) {
    vec3 surfaceToCameraNormal = normalize(cameraPosition - position);
    float specularLightAmount = pow(max(0.0, dot(reflect(-surfaceToLightNormal, normal), surfaceToCameraNormal)), shininess);
    specularColor = light.color * specularLightAmount * attenuation;
  } else {
    specularColor = vec4(0);
  }
}

void main() {
  // Have to renormalize because the vertex normals can become distorted when interpolated.
  vec3 fragNormal = normalize(vertexNormal);

  vec3 Q1 = dFdx(vertexPosition);
  vec3 Q2 = dFdy(vertexPosition);
  vec2 st1 = dFdx(uv0);
  vec2 st2 = dFdy(uv0);
  vec3 tangent = normalize(Q1*st2.t - Q2*st1.t);
  vec3 binormal = normalize(-Q1*st2.s + Q2*st1.s);

  vec3 textureNormal = texture(normalTexture, uv0).xyz * 2.0 - 1.0;
  fragNormal = normalize(mat3(tangent, binormal, fragNormal) * textureNormal);

  vec2 sphericalReflectionUv = acos(normalize(projectionMatrix * viewMatrix * vec4(fragNormal, 0)).xy) / 3.14159;

  vec4 individualLightDiffuseColors[8];
  vec4 individualLightSpecularColors[8];
  
  for (int i = 0; i < 8; ++i) {
    vec4 diffuseLightColor = vec4(0);
    vec4 specularLightColor = vec4(0);
    
    getIndividualLightColors(lights[i], vertexPosition, fragNormal, shininess, diffuseLightColor, specularLightColor);
    
    individualLightDiffuseColors[i] = diffuseLightColor;
    individualLightSpecularColors[i] = specularLightColor;
  }
  
  vec3 colorComponent = clamp(vec3(0.5)*texture(texture2.sampler, texture2.transform2d * vec3((sphericalReflectionUv).x, (sphericalReflectionUv).y, 1)).rgb + vec3(0.5) + color_GxMaterialColor0*clamp((individualLightDiffuseColors[0].rgb + color_GxAmbientColor0), 0.0, 1.0)*texture(texture0, uv0).rgb*vec3(2.0)*clamp((vec3(0.5)*texture(texture2.sampler, texture2.transform2d * vec3((sphericalReflectionUv).x, (sphericalReflectionUv).y, 1)).rgb + vec3(0.5)), 0.0, 1.0) + vec3(-0.5), 0.0, 1.0);

  float alphaComponent = texture(texture3.sampler, texture3.transform2d * vec3((uv0).x, (uv0).y, 1)).a + -1.0*(1.0 + -1.0*scalar_GxAlphaRegister0);

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent >= 0.003921569)) {
    discard;
  }
}