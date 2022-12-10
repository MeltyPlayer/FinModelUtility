﻿using Microsoft.CodeAnalysis;
using schema.parser;
using System.Collections.Generic;


namespace schema.attributes.memory {
  internal class PointerToParser {
    public void Parse(IList<Diagnostic> diagnostics,
                      ISymbol memberSymbol,
                      ITypeInfo memberTypeInfo,
                      IMemberType memberType) {
      var pointerToAttribute = SymbolTypeUtil.GetAttribute<PointerToAttribute>(
          diagnostics, memberSymbol);
      if (pointerToAttribute == null) {
        return;
      }

      AccessChainUtil.AssertAllNodesInTypeChainUntilTargetUseBinarySchema(
          diagnostics, pointerToAttribute.AccessChainToOtherMember);

      if (memberTypeInfo is IIntegerTypeInfo &&
          memberType is SchemaStructureParser.PrimitiveMemberType
              primitiveMemberType) {
        primitiveMemberType.AccessChainToPointer =
            pointerToAttribute.AccessChainToOtherMember;
      } else {
        diagnostics.Add(
            Rules.CreateDiagnostic(memberSymbol, Rules.NotSupported));
      }
    }
  }
}