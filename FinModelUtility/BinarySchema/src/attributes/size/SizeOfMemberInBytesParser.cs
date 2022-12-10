﻿using Microsoft.CodeAnalysis;
using schema.parser;
using System.Collections.Generic;


namespace schema.attributes.size {
  internal class SizeOfMemberInBytesParser {
    public void Parse(IList<Diagnostic> diagnostics,
                      ISymbol memberSymbol,
                      ITypeInfo memberTypeInfo,
                      IMemberType memberType) {
      var sizeOfAttribute =
          SymbolTypeUtil.GetAttribute<SizeOfMemberInBytesAttribute>(
              diagnostics, memberSymbol);
      if (sizeOfAttribute == null) {
        return;
      }

      AccessChainUtil.AssertAllNodesInTypeChainUntilTargetUseBinarySchema(
          diagnostics, sizeOfAttribute.AccessChainToOtherMember);

      if (memberTypeInfo is IIntegerTypeInfo &&
          memberType is SchemaStructureParser.PrimitiveMemberType
              primitiveMemberType) {
        primitiveMemberType.AccessChainToSizeOf =
            sizeOfAttribute.AccessChainToOtherMember;
      } else {
        diagnostics.Add(
            Rules.CreateDiagnostic(memberSymbol, Rules.NotSupported));
      }
    }
  }
}