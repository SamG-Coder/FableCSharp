# Fable scene slice

This tool freezes the retail-data-driven `StartOakVale` intro at the boundary
between engine scene construction and the graphics API. It starts New Game via
Gameflow, advances the real `CS_OAKVALE_INTRO_FATHER` interpreter, models the
user skipping its blocking video, and stops only after the script executes
`UseCamera CAM_OVIF_SHOT2`.

The capture is deliberately grep-first:

```powershell
dotnet run --project tools/Fable.SceneSlice -- --backend=capture
rg "^(CAMERA|MATRIX|PASS|TEXTURE)" tools/Fable.SceneSlice/out/start-oakvale/scene-render-grep.txt
rg "^(SCENE|DRAW|TEXTURE)" tools/Fable.SceneSlice/out/start-oakvale/dx9-render-grep.txt
rg "^(STATE|SAMPLER|PUSH|SHADER_LITERAL|GAP)" tools/Fable.SceneSlice/out/start-oakvale/vulkan-render-grep.txt
```

Render the exact captured packet through the Vulkan backend:

```powershell
dotnet run --project tools/Fable.SceneSlice -- --backend=vulkan
```

`dx9-render-grep.txt` is the DX9 semantic contract, not a native D3D9 visual
backend. Both it and Vulkan consume the same `SceneRenderPacket`, which keeps
asset parsing, culling, batching, camera state, and graphics-API translation
separable. A native D3D9 backend can implement `ISceneRenderBackend` without
changing the slice producer.

The generated `out` directory is ignored because a complete capture is around
100 MB and can always be reproduced from the installed retail data.
