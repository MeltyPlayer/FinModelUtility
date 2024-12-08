#version 310 es
precision mediump float;

uniform sampler2D texture0;

in vec2 sphericalReflectionUv;

out vec4 fragColor;

void main() {
  vec3 colorComponent = texture(texture0, sphericalReflectionUv).rgb;

  float alphaComponent = 1.0;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0.0)) {
    discard;
  }
}