#version 430


out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(2)*vec3(1,0.7019608020782471,0);

  float alphaComponent = 1;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0)) {
    discard;
  }
}
