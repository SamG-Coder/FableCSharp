using Fable.Core;

namespace Fable.ExeIndex;

/// <summary>
/// Produces focused, grep-first evidence graphs for native engine subsystems.
/// The probe labels organize the output; only decoded instructions, references,
/// edges, and hashed retail artifacts are evidence.
/// </summary>
internal static class SystemBehaviorExport
{
    private static readonly NativeEvidenceSpec[] Specs =
    [
        Spec("world-level-streaming", "world and level loading, region graphs, maps, and streaming",
            [
                P(0x00500540, "region_request_probe"), P(0x00501450, "region_catalog_probe"),
                P(0x006C1BE0, "load_job_apply_probe"), P(0x006C20A0, "loader_work_probe"),
                P(0x006C2710, "loader_update_probe"), P(0x004FB150, "current_region_probe"),
                P(0x004FC180, "region_record_probe"), P(0x004FC210, "region_name_lookup_probe"),
                P(0x004FC8A0, "region_loaded_probe"), P(0x004FD040, "wld_region_write_probe"),
                P(0x0050881D, "wld_region_read_probe"), P(0x00B42750, "static_maps_open_probe"),
                P(0x00B428E0, "static_map_select_probe"), P(0x004FCBB0, "topology_activate_probe"),
                P(0x005223F0, "global_thing_map_dispatch_probe"),
                P(0x00521AE0, "thing_manager_load_file_probe"),
            ],
            ["FinalAlbion.wld", "RegionGraph", "NewRegion", "ContainsMap", "SeesMap", "OpenStaticMaps", "Level loader", "SetStaticMap"],
            ["Levels/FinalAlbion.wld", "Levels/FinalAlbion.bwd", "Levels/FinalAlbion.wad", "Levels/FinalAlbion_RT.stb", "compiled/game.bin"]),

        Spec("rendering-3d", "3D renderer, scene layers, shaders, landscape, sky, water, and draw submission",
            [
                P(0x00B25950, "render_frame_probe"), P(0x00B29880, "engine_component_probe"),
                P(0x00B262C0, "render_layer_probe"), P(0x00B33B50, "main_scene_probe"),
                P(0x00B3D200, "shader_register_probe"), P(0x00B324A0, "draw_dispatch_probe"),
                P(0x009DB700, "primitive_enqueue_probe"), P(0x009D9C80, "primitive_flush_probe"),
                P(0x009DA9F0, "layer_flush_probe"), P(0x00B6B0B0, "landscape_draw_probe"),
                P(0x00BD549D, "palskin_draw_probe"), P(0x00B662F0, "sky_draw_probe"),
                P(0x00B7ED80, "water_enable_probe"), P(0x00B4B6F0, "primitives_2d_probe"),
                P(0x00A04630, "dx9_state_slot_ctor_probe"), P(0x00A044E0, "dx9_render_state_flush_probe"),
                P(0x00A058C0, "dx9_sampler_state_flush_probe"), P(0x00B25180, "per_frame_sampler_defaults_probe"),
                P(0x009BEF80, "dx9_viewport_probe"), P(0x009BEF20, "dx9_begin_scene_probe"),
                P(0x009BEF50, "dx9_end_scene_probe"), P(0x009BEEB0, "dx9_present_probe"),
                P(0x009D8CF0, "dx9_clear_probe"), P(0x009FC810, "texture_uv_probe"),
                P(0x00B24BF7, "landscape_cull_state_probe"), P(0x00BB2540, "static_lit_bind_probe"),
                P(0x00BB2DA2, "static_lit_cull_state_probe"), P(0x00BD3070, "palskin_bind_probe"),
                P(0x00BD71B0, "palskin_submit_probe"), P(0x00B84720, "primitive_queue_submit_probe"),
                P(0x00B33010, "main_scene_layer_drain_probe"), P(0x00BBE090, "primitive_none_draw_probe"),
                P(0x00B47630, "fog_constant_probe"), P(0x00B54310, "world_inverse_constant_probe"),
                P(0x009896D0, "vertex_shader_constants_probe"), P(0x00988290, "world_matrix_probe"),
                P(0x009BE8B0, "texture_decode_probe"), P(0x009FA450, "texture_lock_probe"),
            ],
            ["Add Render Layer", "MainScene", "EnableSky", "EnableLandscape", "EnableWater", "VSHADER_", "PSHADER_", "SetRenderState", "SetSamplerState", "SetVertexShaderConstantF", "DrawIndexedPrimitive", "Render"],
            ["graphics/graphics.big", "graphics/pc/textures.big", "shaders/pc/shaders.big", "Levels/FinalAlbion_RT.stb", "compiled/game.bin"]),

        Spec("input-player", "input acquisition, control bindings, player input processing, and action dispatch",
            [
                P(0x0041E3F6, "input_construct_probe"), P(0x0041DB84, "input_state_probe"),
                P(0x0041E61B, "input_thunk_probe"), P(0x0041BEB0, "input_pack_probe"),
                P(0x004457F0, "player_preprocess_probe"), P(0x00446A30, "player_input_pump_probe"),
                P(0x00446330, "player_input_poll_probe"), P(0x009F4ED0, "device_input_probe"),
                P(0x004AEAA0, "player_action_probe"), P(0x004AE9C0, "player_tick_probe"),
                P(0x0044A530, "player_slot_create_probe"),
            ],
            ["Input", "Control Method", "Mouse", "Keyboard", "BindKey", "Player Interface", "Create Players"],
            ["compiled/game.bin", "compiled/frontend.bin"]),

        Spec("thing-components", "Thing construction, definitions, components, factories, and native object composition",
            [
                P(0x00522A20, "thing_register_probe"), P(0x006A5950, "thing_construct_probe"),
                P(0x006A9DD0, "thing_params_probe"), P(0x004C9030, "cthing_construct_probe"),
                P(0x004C97B0, "thing_name_bind_probe"), P(0x004C7CF0, "thing_script_activate_probe"),
                P(0x004C9D60, "component_add_probe"), P(0x0052AC10, "building_factory_probe"),
                P(0x005296B0, "building_construct_probe"), P(0x007E14C0, "multistatic_construct_probe"),
                P(0x007E15C0, "multistatic_apply_probe"), P(0x006AC910, "player_creature_create_probe"),
                P(0x005223F0, "global_thing_map_dispatch_probe"),
                P(0x00521AE0, "thing_manager_load_file_probe"),
                P(0x00520D00, "thing_text_walk_probe"), P(0x0051FD80, "load_single_thing_probe"),
            ],
            ["CThing", "CREATEBUILDING", "CREATURE_", "OBJECT_", "Thing Definition", "Component Definition"],
            ["compiled/game.bin", "Levels/FinalAlbion.wad", "graphics/graphics.big"]),

        Spec("animation", "animation playback, action binding, skeletons, bones, palettes, and skinned drawing",
            [
                P(0x0070B3F0, "animation_factory_probe"), P(0x00686920, "animation_component_probe"),
                P(0x0070D580, "animation_play_probe"), P(0x005B37F7, "appearance_default_probe"),
                P(0x00903570, "action_animation_probe"), P(0x009035F0, "combat_animation_probe"),
                P(0x00834760, "combat_animation_apply_probe"), P(0x00AA0090, "bone_hierarchy_probe"),
                P(0x00BD2F91, "palskin_probe"), P(0x009896D0, "palette_upload_probe"),
                P(0x004C7470, "thing_animation_probe"),
            ],
            ["Animation", "XSeq", "PlayAnimation", "PlayCombatAnimation", "PALSKIN", "Bone", "Skeleton", "Appearance"],
            ["graphics/graphics.big", "Misc/sound_animation_events.bin", "compiled/game.bin"]),

        Spec("audio", "sound initialization, lookup, registration, music, atmosphere, and script audio calls",
            [
                P(0x00417A58, "sound_init_probe"), P(0x00415550, "sound_locale_probe"),
                P(0x004196B2, "sound_lookup_probe"), P(0x009919C0, "sound_register_probe"),
                P(0x00991C10, "atmosphere_register_probe"), P(0x00991840, "sound_map_lookup_probe"),
                P(0x00A38C20, "sound_symbols_compiled_probe"), P(0x00A01A4F, "sound_symbols_text_probe"),
                P(0x00CC8EAC, "script_play_music_probe"), P(0x009E5120, "music_lookup_probe"),
                P(0x00CC7258, "script_mute_sounds_probe"),
            ],
            ["Init Sound", "PlayMusic", "MuteSounds", "MUSIC_SET_", "SOUND_", "Audio", ".ogg"],
            ["Misc/sound_animation_events.bin", "compiled/game.bin", "compiled/script.bin"]),

        Spec("save-profile-persistence", "profiles, new-game defaults, save/load, autosave, and persisted region state",
            [
                P(0x005958F5, "profile_bind_probe"), P(0x005955AB, "profile_enumerate_probe"),
                P(0x00595845, "profile_missing_probe"), P(0x00596917, "new_profile_bind_probe"),
                P(0x004069E0, "profile_default_probe"), P(0x0059697A, "profile_commit_probe"),
                P(0x004067C0, "profile_create_check_probe"), P(0x004A3200, "load_save_probe"),
                P(0x00449F90, "save_region_name_probe"), P(0x0049FB5C, "save_region_site_probe"),
                P(0x004045C0, "persistence_helper_probe"),
            ],
            ["Profile", "Save Game", "Load Game", "Autosave", "Continue", "Persist", "PlayerRegionName"],
            ["compiled/game.bin", "compiled/script.bin", "Levels/FinalAlbion.qst"]),

        Spec("camera", "camera ownership, interpolation, field of view, frustum state, and script camera control",
            [
                P(0x004164E0, "camera_update_probe"), P(0x0049E080, "world_camera_apply_probe"),
                P(0x006B42F0, "camera_blend_probe"), P(0x00416231, "camera_time_probe"),
                P(0x0041707E, "camera_interpolation_probe"), P(0x004166E2, "camera_interpolation_time_probe"),
                P(0x0041919C, "camera_clamp_probe"), P(0x00B2FD60, "frustum_extract_probe"),
                P(0x00B30B50, "camera_fov_probe"), P(0x00B314E0, "render_camera_update_probe"),
                P(0x00B54310, "camera_constants_probe"), P(0x00CBF29F, "script_camera_probe"),
                P(0x00CC9F3A, "script_use_camera_probe"), P(0x00CC9E6A, "script_no_load_camera_probe"),
            ],
            ["Camera", "UseCamera", "NoLoadUseCamera", "FOV", "LookAt", "CAM_"],
            ["compiled/script.bin", "compiled/game.bin", "Levels/FinalAlbion.wad"]),

        Spec("combat-inventory-interaction", "combat actions, inventory mutation, weapons, damage, and world interaction",
            [
                P(0x00CC15E3, "script_combat_animation_probe"), P(0x00CC16FD, "combat_animation_apply_probe"),
                P(0x00834760, "combat_action_probe"), P(0x006AD9D0, "player_combat_probe"),
                P(0x009035F0, "combat_animation_name_probe"), P(0x0061AB30, "inventory_confirm_probe"),
                P(0x0061ACB3, "inventory_give_probe"), P(0x008902E0, "give_named_object_probe"),
                P(0x008910D0, "inventory_remove_probe"), P(0x004C9B80, "thing_remove_probe"),
                P(0x0072DF50, "building_interaction_probe"),
            ],
            ["PlayCombatAnimation", "InventoryQuests", "Quest Card", "Combat", "Inventory", "Weapon", "Expression"],
            ["compiled/game.bin", "compiled/script.bin"]),

        Spec("ai-navigation", "AI tasks, path movement, creature behavior, opinions, following, and conversations",
            [
                P(0x006A9550, "wait_task_probe"), P(0x00CC0CB5, "script_sneak_to_probe"),
                P(0x00CC0F1A, "sneak_to_poll_probe"), P(0x00CC083D, "script_walk_to_probe"),
                P(0x006E5660, "dialog_wait_probe"), P(0x006BDC60, "opinion_tick_probe"),
                P(0x006E60F0, "conversation_probe"), P(0x0051F070, "thing_manager_flush_probe"),
                P(0x006BB990, "environment_update_probe"),
            ],
            ["Navigation", "WalkTo", "SneakTo", "Opinion", "FollowTarget", "Conversation"],
            ["compiled/game.bin", "compiled/script.bin", "Levels/FinalAlbion.wad"]),

        Spec("hud-ingame", "in-game player GUI, HUD state, health, mana, inventory, quests, and context display",
            [
                P(0x0043A080, "player_gui_tick_probe"), P(0x0043A380, "player_gui_init_probe"),
                P(0x00487FB0, "gui_allocate_probe"), P(0x0043B570, "gui_construct_probe"),
                P(0x004195AF, "gui_store_probe"), P(0x00462F93, "gui_definition_factory_probe"),
                P(0x00459BB6, "gui_definition_construct_probe"), P(0x004736C4, "gui_persist_probe"),
                P(0x0061AB30, "quest_confirm_probe"), P(0x0061ACB3, "quest_give_probe"),
                P(0x006496BC, "overlay_draw_probe"),
            ],
            ["Player GUI", "MiniMap", "HUD", "Health Bar", "Mana Bar", "Inventory Screen", "Quest Screen"],
            ["compiled/game.bin", "graphics/pc/frontend.big", "lang/English/fonts.big", "lang/English/text.big"]),

        Spec("quest-script-lifecycle", "native quest fibers, cutscene child return, instruction/event gates, HUD ownership, and gameplay handoff",
            [
                P(0x00DB8680, "s_qnovi_parent_probe"), P(0x00DB86B0, "intro_child_start_probe"),
                P(0x00DB88FD, "intro_child_return_probe"), P(0x00DB894C, "highlight_instruction_probe"),
                P(0x00DB8A83, "good_deed_hud_create_probe"), P(0x00DB8B00, "father_loop_entry_probe"),
                P(0x00DB9785, "father_loop_exit_probe"), P(0x00DB97A0, "theresa_parent_probe"),
                P(0x00DBB21B, "raid_avi_parent_probe"), P(0x00DBB2A7, "attack_over_store_probe"),
                P(0x00DB7DB0, "barrel_watcher_probe"), P(0x00DB7E10, "barrel_instruction_probe"),
                P(0x00894370, "world_event_query_probe"), P(0x008ABED0, "event_journal_search_probe"),
                P(0x0073B6FE, "event_type_12_registration_probe"), P(0x0073F310, "cheering_event_construct_probe"),
                P(0x006E7510, "fiber_window_start_probe"), P(0x006E7530, "fiber_window_end_probe"),
                P(0x008929D0, "instruction_submit_probe"), P(0x00891B40, "hud_create_probe"),
                P(0x00891920, "hud_enable_probe"),
                P(0x0089B5B0, "scripted_thing_resource_acquire_probe"),
                P(0x00903810, "scripted_thing_resource_fallback_ctor_probe"),
                P(0x009039D0, "scripted_thing_resource_ctor_probe"),
                P(0x00712C20, "scripted_thing_component31_bind_probe"),
                P(0x008AB960, "scripted_resource_queue_probe"),
                P(0x008ABD10, "scripted_resource_fallback_queue_probe"),
                P(0x0089B110, "father_loop_vtbl_1480_probe"),
                P(0x00890AB0, "father_loop_vtbl_1516_probe"),
                P(0x00DAEA70, "father_loop_quest_state_probe"),
                P(0x006E7410, "fiber_yield_probe"), P(0x006E75C0, "script_manager_pump_probe"),
                P(0x00A44880, "microthread_update_probe"), P(0x00A44660, "microthread_resume_probe"),
                P(0x0049D870, "world_frame_read_probe"), P(0x00687540, "world_event_post_probe"),
                P(0x006874B0, "world_event_expire_probe"), P(0x00416E78, "player_interface_pump_probe"),
            ],
            ["S_QNOVI", "Q_NewOakValeIntro", "TEXT_QST_048_", "HUD_DEED_GOOD_ICON", "CHEERING", "AttackOver", "PostAttack", "Player Interface"],
            ["compiled/script.bin", "compiled/game.bin", "Levels/FinalAlbion.qst", "Levels/FinalAlbion.wad", "lang/English/text.big"]),

        Spec("video-cutscene", "video playback, cutscene script calls, media graph setup, frame blitting, and presentation",
            [
                P(0x00CBFB7D, "cutscene_interpreter_probe"), P(0x00CCA26D, "script_play_avi_probe"),
                P(0x00CCA2BD, "play_avi_apply_probe"), P(0x0088F890, "video_vtable_probe"),
                P(0x0040D2A0, "video_singleton_probe"), P(0x006286F0, "video_player_probe"),
                P(0x00A3B9D0, "media_open_probe"), P(0x0099C1E0, "media_path_rewrite_probe"),
                P(0x00A3B510, "video_renderer_construct_probe"), P(0x00A3B130, "video_run_probe"),
                P(0x00A3B730, "video_copy_probe"), P(0x009DC870, "video_blit_probe"),
                P(0x009BEF20, "begin_scene_probe"), P(0x009BEF50, "end_scene_probe"),
                P(0x009BEEB0, "present_probe"), P(0x009D9C80, "primitive_flush_probe"),
            ],
            ["PlayAVI", "Data\\Video\\", ".xmv", ".wmv", "Cutscene", "FadeOut", "FadeIn"],
            ["compiled/script.bin", "Video/intro_comp.wmv"]),
    ];

    public static void Run(PeImage pe, GameInstall? install, string[] args)
    {
        var outputRoot = ResolveOutputRoot(args);
        var requested = ReadOption(args, "--system");
        var selected = requested is null
            ? Specs
            : Specs.Where(spec => Slug(spec) == requested).ToArray();

        if (selected.Length == 0)
            throw new ArgumentException($"Unknown system '{requested}'. Valid systems: {string.Join(", ", Specs.Select(Slug))}");

        Directory.CreateDirectory(outputRoot);
        foreach (var spec in selected)
        {
            var output = Path.Combine(outputRoot, spec.DefaultFile);
            var systemArgs = args.Concat([spec.OutputOption, output]).ToArray();
            LifecycleBehaviorExport.RunCustom(pe, install, systemArgs, spec);
        }

        var manifest = Path.Combine(outputRoot, "INDEX.txt");
        File.WriteAllLines(manifest,
        [
            "FABLE_NATIVE_SYSTEM_EXPORTS_V1",
            "Generated from retail executable instructions/string xrefs and hashed retail artifacts.",
            "Probe labels are navigation_only and are not recovered semantics.",
            $"generated_this_run\t{string.Join(',', selected.Select(Slug))}",
            .. Specs.Select(spec => $"{Slug(spec)}\t{spec.DefaultFile}\t{spec.Format}\t{spec.Description}"),
        ]);
        Console.WriteLine($"systems index {manifest}");
    }

    private static NativeEvidenceSpec Spec(
        string slug, string description, IReadOnlyList<(uint Va, string Hint)> probes,
        IReadOnlyList<string> anchors, IReadOnlyList<string> artifacts) =>
        new($"FABLE_{slug.Replace('-', '_').ToUpperInvariant()}_GREP_V1",
            $"--{slug}-out", $"{slug}-grep.txt", description, probes, anchors, artifacts);

    private static (uint Va, string Hint) P(uint va, string hint) => (va, hint);

    private static string Slug(NativeEvidenceSpec spec) =>
        Path.GetFileNameWithoutExtension(spec.DefaultFile)[..^5]; // remove "-grep"

    private static string ResolveOutputRoot(string[] args)
    {
        var explicitPath = ReadOption(args, "--systems-out");
        return Path.GetFullPath(explicitPath ?? Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "out", "systems"));
    }

    private static string? ReadOption(string[] args, string option)
    {
        var index = Array.FindIndex(args, value => value.Equals(option, StringComparison.OrdinalIgnoreCase));
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}
