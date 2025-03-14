﻿using System.Collections.Generic;

namespace UoT {
  public static class BetterFileNames {
    private static readonly IDictionary<string, string> impl_ =
        new Dictionary<string, string>();

    // TODO: Add ability to sort into categories too?
    static BetterFileNames() {
      Add(
          ("object_am", "Armos"),
          ("object_Bb", "Bubble"),
          ("object_bdan_objects", "Misc. Inside Jabu-Jabu's Belly"),
          ("object_blkobj", "Dark Link's Room"),
          ("object_bombf", "Bomb Flower"),
          ("object_bw", "Torch Slug"),
          ("object_bwall", "Bombable Wall"),
          ("object_cow", "Cow"),
          ("object_d_hsblock", "Misc. Hookshot"),
          ("object_ddan_objects", "Misc. Dodongo's Cavern"),
          ("object_dekunuts", "Deku Scrub"),
          ("object_dekubaba", "Deku Baba"),
          ("object_dh", "Dead Hand"),
          ("object_dodojr", "Dodongo (Baby)"),
          ("object_dodongo", "Dodongo"),
          ("object_dog", "Dog"),
          ("object_du", "Darunia"),
          ("object_dy_obj", "Great Fairy"),
          ("object_ei", "Stinger"),
          ("object_fd", "Volvagia (Flying)"),
          ("object_fd2", "Volvagia (Ground)"),
          ("object_firefly", "Keese"),
          ("object_fu", "Guru-Guru"),
          ("object_ganon2", "Ganon"),
          ("object_gnd", "Phantom Ganon"),
          ("object_gndd", "Ganondorf"),
          ("object_gol", "Ghoma Larva"),
          ("object_goroiwa", "Boulder"),
          ("object_goma", "Ghoma"),
          ("object_gt", "Ganon's Castle (Exterior)"),
          ("object_hakach_objects", "Misc. Bottom of the Well"),
          ("object_hidan_objects", "Misc. Fire Temple"),
          ("object_horse", "Epona"),
          ("object_ice_objects", "Misc. Ice Cavern"),
          ("object_im", "Impa"),
          ("object_in", "Ingo"),
          ("object_jj", "Jabu-Jabu"),
          ("object_kibako2", "Box"),
          ("object_kingdodongo", "King Dodongo"),
          ("object_kz", "King Zora"),
          ("object_link_boy", "Link (adult)"),
          ("object_link_child", "Link (child)"),
          ("object_ma1", "Malon (child)"),
          ("object_ma2", "Malon (adult)"),
          ("object_mamenoki", "Magic Bean Leaf"),
          ("object_md", "Mido"),
          ("object_mizu_objects", "Misc. Water Temple"),
          ("object_mm", "Running Man"),
          ("object_mori_hineri1", "Twisting Hallway"),
          ("object_ms", "Bean Seller"),
          ("object_niw", "Cucco"),
          ("object_oF1d_map", "Goron"),
          ("object_okuta", "Octorok"),
          ("object_os", "Happy Mask Salesman"),
          ("object_ossan", "Bazaar Shopkeeper"),
          ("object_owl", "Kaepora Gaebora"),
          ("object_peehat", "Peahat"),
          ("object_po_field", "Poe (Hyrule Field)"),
          ("object_poh", "Poe"),
          ("object_ps", "Poe Collector"),
          ("object_relay_objects", "Misc. Windmill"),
          ("object_rd", "Gibdo"),
          ("object_reeba", "Leever"),
          ("object_rl", "Rauru"),
          ("object_rr", "Like Like"),
          ("object_ru1", "Ruto (child)"),
          ("object_ru2", "Ruto (adult)"),
          ("object_sb", "Shell Blade"),
          ("object_sk2", "Stalfos"),
          ("object_skb", "Stalchild"),
          ("object_skj", "Skull Kid"),
          ("object_spot00_objects", "Misc. Hyrule Field"),
          ("object_spot06_objects", "Misc. Lake Hylia"),
          ("object_sst", "Bongo-Bongo (hand)"),
          ("object_st", "Skulltula"),
          ("object_ta", "Talon"),
          ("object_tite", "Tektite"),
          ("object_tk", "Dampé"),
          ("object_toki_objects", "Misc. Temple of Time"),
          ("object_torch2", "Dark Link"),
          ("object_tsubo", "Pot"),
          ("object_vase", "Vase"),
          ("object_vm", "Beamos"),
          ("object_wallmaster", "Wallmaster"),
          ("object_wf", "Wolfos"),
          ("object_xc", "Sheik"),
          ("object_ydan_objects", "Misc. Inside the Deku Tree"),
          ("object_zf", "Lizalfos/Dinolfos"),
          ("object_zl1", "Princess Zelda (child)"),
          ("object_zl2", "Princess Zelda (adult)"),
          ("object_zl2_anime1", "Animations (1) for Princess Zelda (adult)"),
          ("object_zl2_anime2", "Animations (2) for Princess Zelda (adult)"),
          ("object_zo", "Zora")
      );

      Add(
          ("bdan_scene", "Inside Jabu-Jabu's Belly"),
          ("Bmori1_scene", "Forest Temple"),
          ("bowling_scene", "Bombchu Bowling Alley"),
          ("ddan_scene", "Dodongo's Cavern"),
          ("FIRE_bs_scene", "Fire Temple Boss Chamber"),
          ("ganon_tou_scene", "Ganon's Casstle (exterior)"),
          ("gerudoway_scene", "Gerudo Fortress"),
          ("hairal_niwa2_scene", "Hyrule Castle Courtyard"),
          ("HAKAdan_bs_scene", "Shadow Temple Boss Chamber"),
          ("HAKAdan_scene", "Shadow Temple"),
          ("hakasitarelay_scene", "Dampé's Race"),
          ("HIDAN_scene", "Fire Temple"),
          ("kenjyanoma_scene", "Chamber of Sages"),
          ("jyasinboss_scene", "Spirit Temple Boss Chamber"),
          ("jyasinzou_scene", "Spirit Temple"),
          ("MIZUsin_bs_scene", "Water Temple Boss Chamber"),
          ("MIZUsin_scene", "Water Temple"),
          ("moribossroom_scene", "Forest Temple Boss Chamber"),
          ("spot00_scene", "Hyrule Field"),
          ("spot01_scene", "Kakariko Village"),
          ("spot02_scene", "Kakariko Graveyard"),
          ("spot03_scene", "River to Zora's Domain"),
          ("spot04_scene", "Kokiri Forest"),
          ("spot06_scene", "Lake Hylia"),
          ("spot10_scene", "Lost Woods"),
          ("spot13_scene", "Haunted Wasteland"),
          ("spot15_scene", "Hyrule Castle"),
          ("spot16_scene", "Death Mountain Trail"),
          ("spot17_scene", "Death Mountain Crater"),
          ("spot18_scene", "Goron City"),
          ("syatekijyou_scene", "Shooting Gallery"),
          ("tokinoma_scene", "Temple of Time (interior)"),
          ("ydan_scene", "Inside the Deku Tree"),
          ("ydan_boss_scene", "Inside the Deku Tree Boss Chamber")
      );
    }

    public static string Get(string filename) {
      impl_.TryGetValue(filename, out var betterFilename);
      return betterFilename ?? filename;
    }

    private static void Add(params (string, string)[] pairs) {
      foreach (var pair in pairs) {
        impl_.Add(pair.Item1, pair.Item2);
      }
    }
  }
}