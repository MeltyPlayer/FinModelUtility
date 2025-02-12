﻿using System.Buffers.Binary;

using fin.util.asserts;

namespace ttyd.api;

public static class TtydTangents {
  private static float[] tangents_;

  static TtydTangents() {
      var tangentHexText
          = "C26527CCC1E51673C198A5E3C164CF4CC136E156C1183AB8C1024F26C0E3B0F5C0CA0A2AC0B57B11C0A4A01EC0968C45C08A9B65C0805849C06ED9DBC05F31BDC0515597C044F8B6C039DE8EC02FD6A3C026B9BFC01E67CAC016C641C00FBF0CC0093F95C0033828BFFB36C7BFF0BB9ABFE6EB03BFDDB3D0BFD50715BFCCD7BCBFC51A46BFBDC489BFB6CD89BFB02D43BFA9DC95BFA3D51CBF9E1118BF988B5EBF933F42BF8E2889BF89435EBF848C3EBF7FFFFBBF77375BBF6EB94BBF6680DCBF5E898EBF56CF38BF4F4DFDBF480252BF40E8E7BF39FEADBF3340CABF2CAC94BF263F91BF1FF76DBF19D1FDBF13CD38BF0DE730BF081E19BF027040BEF9B812BEEEBFDDBEE3F4FFBED954BFBECEDC83BEC489D1BEBA5A4CBEB04BB3BEA65BDDBE9C88B9BE92D048BE8930A1BE7F4FD3BE6C68B3BE59A86ABE470B9FBE348F0DBE222F85BE0FE9EEBDFB7675BDD740E0BDB32D3FBD8F35C8BD56A987BD0F0913BC8EFDEB000000003C8EFDEB3D0F09133D56A9873D8F35C83DB32D3F3DD740E03DFB76753E0FE9EE3E222F853E348F0D3E470B9F3E59A86A3E6C68B33E7F4FD33E8930A13E92D0483E9C88B93EA65BDD3EB04BB33EBA5A4C3EC489D13ECEDC833ED954BF3EE3F4FF3EEEBFDD3EF9B8123F0270403F081E193F0DE7303F13CD383F19D1FD3F1FF76D3F263F913F2CAC943F3340CA3F39FEAD3F40E8E73F4802523F4F4DFD3F56CF383F5E898E3F6680DC3F6EB94B3F77375B3F7FFFFB3F848C3E3F89435E3F8E28893F933F423F988B5E3F9E11183FA3D51C3FA9DC953FB02D433FB6CD893FBDC4893FC51A463FCCD7BC3FD507153FDDB3D03FE6EB033FF0BB9A3FFB36C74003382840093F95400FBF0C4016C641401E67CA4026B9BF402FD6A34039DE8E4044F8B640515597405F31BD406ED9DB40805849408A9B6540968C4540A4A01E40B57B1140CA0A2A40E3B0F541024F2641183AB84136E1564164CF4C4198A5E341E51673";

      var ttydTangentBytes = Convert.FromHexString(tangentHexText);
      tangents_ = new float[ttydTangentBytes.Length / 4 + 1];
      for (var i = 0; i < ttydTangentBytes.Length / 4; ++i) {
        var ttydTangent
            = BinaryPrimitives.ReadSingleBigEndian(
                ttydTangentBytes.AsSpan(4 * i, 4));
        tangents_[i] = ttydTangent;
      }

      tangents_[^1] = -tangents_[0];
    }

  public static float GetTangent(sbyte value) {
      Asserts.True(value >= -89,
                   "Expected angle to be greater than or equal to -89 degrees!");
      Asserts.True(value <= 89,
                   "Expected angle to be less than 89 degrees!");
      return tangents_[value + 89];
    }
}