﻿using System.Numerics;

using fin.model;

namespace fin.gl.material {
  public static class CommonShaderPrograms {
    private static GlShaderProgram? texturelessShaderProgram_;

    public static GlShaderProgram TEXTURELESS_SHADER_PROGRAM {
      get {
        if (CommonShaderPrograms.texturelessShaderProgram_ == null) {
          CommonShaderPrograms.texturelessShaderProgram_ =
              GlShaderProgram.FromShaders(@"
# version 120

varying vec4 vertexColor;

void main() {
    gl_Position = gl_ProjectionMatrix * gl_ModelViewMatrix * gl_Vertex; 
    vertexColor = gl_Color;
}",
                                          @"
# version 130 

out vec4 fragColor;

in vec4 vertexColor;

void main() {
    fragColor = vertexColor;
}");
        }

        return texturelessShaderProgram_;
      }
    }

    public static TNumber UseThenAdd<TNumber>(ref TNumber value, TNumber delta)
        where TNumber : INumber<TNumber> {
      var initialValue = value;
      value += delta;
      return initialValue;
    }

    // TODO: Only include uvs/colors as needed
    public static string GetVertexSrc(IModel model, bool useBoneMatrices) {
      var location = 0;
      var vertexSrc = $@"
# version 330

uniform mat4 {ShaderConstants.UNIFORM_MODEL_VIEW_MATRIX_NAME};
uniform mat4 {ShaderConstants.UNIFORM_PROJECTION_MATRIX_NAME};";

      if (useBoneMatrices) {
        vertexSrc += @$"
uniform mat4 {ShaderConstants.UNIFORM_BONE_MATRICES_NAME}[{1 + model.Skin.BoneWeights.Count}];";
      }

      vertexSrc += @$"

layout(location = {location++}) in vec3 in_Position;
layout(location = {location++}) in vec3 in_Normal;
layout(location = {location++}) in vec4 in_Tangent;";

      if (useBoneMatrices) {
        vertexSrc += @$"
layout(location = {location++}) in int in_MatrixId;";
      }

      vertexSrc += @$"
layout(location = {UseThenAdd(ref location, MaterialConstants.MAX_UVS)}) in vec2 in_Uvs[{MaterialConstants.MAX_UVS}];
layout(location = {UseThenAdd(ref location, MaterialConstants.MAX_COLORS)}) in vec4 in_Colors[{MaterialConstants.MAX_COLORS}];

out vec3 vertexNormal;
out vec3 tangent;
out vec3 binormal;
out vec2 normalUv;";

      for (var i = 0; i < MaterialConstants.MAX_UVS; ++i) {
        vertexSrc += $@"
out vec2 uv{i};";
      }

      for (var i = 0; i < MaterialConstants.MAX_COLORS; ++i) {
        vertexSrc += $@"
out vec4 vertexColor{i};";
      }

      vertexSrc += @"
void main() {";

      if (useBoneMatrices) {
        vertexSrc += $@"
  mat4 vertexMatrix = {ShaderConstants.UNIFORM_BONE_MATRICES_NAME}[in_MatrixId];

  mat4 vertexModelMatrix = {ShaderConstants.UNIFORM_MODEL_VIEW_MATRIX_NAME} * vertexMatrix;
  mat4 projectionVertexModelMatrix = {ShaderConstants.UNIFORM_PROJECTION_MATRIX_NAME} * vertexModelMatrix;

  gl_Position = projectionVertexModelMatrix * vec4(in_Position, 1);
  vertexNormal = normalize(vertexModelMatrix * vec4(in_Normal, 0)).xyz;
  tangent = normalize(vertexModelMatrix * vec4(in_Tangent)).xyz;
  binormal = cross(vertexNormal, tangent);
  normalUv = normalize(projectionVertexModelMatrix * vec4(in_Normal, 0)).xy;";
      } else {
        vertexSrc += $@"
  gl_Position = {ShaderConstants.UNIFORM_PROJECTION_MATRIX_NAME} * {ShaderConstants.UNIFORM_MODEL_VIEW_MATRIX_NAME} * vec4(in_Position, 1);
  vertexNormal = normalize({ShaderConstants.UNIFORM_MODEL_VIEW_MATRIX_NAME} * vec4(in_Normal, 0)).xyz;
  tangent = normalize({ShaderConstants.UNIFORM_MODEL_VIEW_MATRIX_NAME} * vec4(in_Tangent)).xyz;
  binormal = cross(vertexNormal, tangent); 
  normalUv = normalize({ShaderConstants.UNIFORM_PROJECTION_MATRIX_NAME} * {ShaderConstants.UNIFORM_MODEL_VIEW_MATRIX_NAME} * vec4(in_Normal, 0)).xy;";
      }

      for (var i = 0; i < MaterialConstants.MAX_UVS; ++i) {
        vertexSrc += $@"
  uv{i} = in_Uvs[{i}];";
      }

      for (var i = 0; i < MaterialConstants.MAX_COLORS; ++i) {
        vertexSrc += $@"
  vertexColor{i} = in_Colors[{i}];";
      }

      vertexSrc += @"
}";

      return vertexSrc;
    }
  }
}