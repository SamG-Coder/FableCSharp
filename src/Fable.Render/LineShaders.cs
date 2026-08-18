namespace Fable.Render;

internal static class LineShaders
{
    public const string Vertex = """
        #version 450
        layout(location = 0) in vec3 inPosition;
        layout(location = 1) in vec3 inColor;
        layout(location = 0) out vec3 fragColor;
        layout(push_constant) uniform Push { mat4 viewProj; } pc;
        void main() {
            gl_Position = pc.viewProj * vec4(inPosition, 1.0);
            fragColor = inColor;
        }
        """;

    public const string Fragment = """
        #version 450
        layout(location = 0) in vec3 fragColor;
        layout(location = 0) out vec4 outColor;
        void main() {
            outColor = vec4(fragColor, 1.0);
        }
        """;

    /// <summary>
    /// Screen-space triangle for <c>0041BEB0</c> type
    /// <c>0x22</c> fade overlay. Color is the packed
    /// RGB from <c>[+212]</c> and A from
    /// <c>004348D0</c>.
    /// </summary>
    public const string OverlayVertex = """
        #version 450
        void main() {
            vec2 p = vec2((gl_VertexIndex << 1) & 2, gl_VertexIndex & 2);
            gl_Position = vec4(p * 2.0 - 1.0, 0.0, 1.0);
        }
        """;

    public const string OverlayFragment = """
        #version 450
        layout(location = 0) out vec4 outColor;
        layout(push_constant) uniform Push { vec4 color; } pc;
        void main() {
            outColor = pc.color;
        }
        """;

    /// <summary>
    /// <c>00628B79</c> dest (scale to viewport width,
    /// center leftover) then <c>009DC870</c> 2D
    /// submit. UV 0–1 inside dest; outside is the
    /// leftover black bars.
    /// </summary>
    public const string VideoVertex = """
        #version 450
        layout(location = 0) out vec2 fragUv;
        void main() {
            vec2 p = vec2((gl_VertexIndex << 1) & 2, gl_VertexIndex & 2);
            fragUv = p;
            gl_Position = vec4(p * 2.0 - 1.0, 0.0, 1.0);
        }
        """;

    public const string VideoFragment = """
        #version 450
        layout(location = 0) in vec2 fragUv;
        layout(location = 0) out vec4 outColor;
        layout(set = 0, binding = 0) uniform sampler2D video;
        layout(push_constant) uniform Push { vec4 dest; } pc;
        void main() {
            if (fragUv.x < pc.dest.x || fragUv.x > pc.dest.z ||
                fragUv.y < pc.dest.y || fragUv.y > pc.dest.w) {
                // 009BE420 / 009D8CF0 already
                // cleared to [0x13961E0]. The
                // dest quad does not write bars.
                discard;
            }
            vec2 t = (fragUv - pc.dest.xy) / (pc.dest.zw - pc.dest.xy);
            // 00A3B730 writes GetPointer row 0 into
            // LockRect row 0 (no CPU V flip). RGB24
            // VIDEOINFOHEADER is a bottom-up DIB;
            // 009DC870 2D dest has v=0 at dest top.
            // Invert V so the first sample row sits
            // at dest.w (screen bottom), matching
            // that blit.
            outColor = texture(video, vec2(t.x, 1.0 - t.y));
        }
        """;

    public const string MeshVertex = """
        #version 450
        layout(location = 0) in vec3 inPosition;
        layout(location = 1) in vec3 inNormal;
        layout(location = 2) in vec2 inUv;
        layout(location = 3) in vec4 inColor;
        layout(location = 4) in vec3 inExtra;
        layout(location = 0) out vec3 fragNormal;
        layout(location = 1) out vec2 fragUv;
        layout(location = 2) out vec4 fragColor;
        layout(location = 3) out vec3 fragWorld;
        layout(location = 4) out float fragFog;
        layout(location = 5) out vec3 fragExtra;
        layout(push_constant) uniform Push {
            mat4 viewProj;
            vec4 cameraPos;
            vec4 lightDir;
            vec4 lightColor;
            vec4 pass;
        } pc;
        void main() {
            gl_Position = pc.viewProj * vec4(inPosition, 1.0);
            fragNormal = inNormal;
            fragUv = inUv;
            fragColor = inColor;
            fragWorld = inPosition;
            fragExtra = inExtra;
            // 00B47630 plane: dp4 world, c2. mad oFog, min(dp,c0.y), -c18.w, c0.y
            // D3D fog interpolator saturates; the mad has no _sat.
            float dp = dot(vec4(inPosition, 1.0), pc.cameraPos);
            float c0y = 1.0;
            float c18w = 1.0;
            fragFog = clamp(min(dp, c0y) * (-c18w) + c0y, 0.0, 1.0);
        }
        """;

    public const string MeshFragment = """
        #version 450
        layout(location = 0) in vec3 fragNormal;
        layout(location = 1) in vec2 fragUv;
        layout(location = 2) in vec4 fragColor;
        layout(location = 3) in vec3 fragWorld;
        layout(location = 4) in float fragFog;
        layout(location = 5) in vec3 fragExtra;
        layout(location = 0) out vec4 outColor;
        layout(set = 0, binding = 0) uniform sampler2D albedo0;
        layout(set = 1, binding = 0) uniform sampler2D albedo1;
        layout(push_constant) uniform Push {
            mat4 viewProj;
            vec4 cameraPos;
            vec4 lightDir;
            vec4 lightColor;
            vec4 pass;
        } pc;
        void main() {
            float mode = pc.pass.x;
            // BG (mode 0): oT0 = v3 (ExtraRgb.XY).
            // FG (mode 1): oT0.xy = v3.yz; oT1 = dp4(pos,c40/c41)=(0,0).
            // STATIC/PALSKIN (mode 3): oT0 = v2 / v4 = mesh UV (fragUv).
            vec2 ot0 = mode < 0.5 ? fragExtra.xy
                     : mode < 1.5 ? fragExtra.yz
                     : fragUv;
            vec2 ot1 = fragUv;
            vec4 t0 = texture(albedo0, ot0);
            vec4 t1 = texture(albedo1, ot1);
            vec3 n = fragNormal;
            float nlen = length(n);
            // VS: dp3 n,-c19; max(.,c0.x); square; *c20; mad c35; add c3.
            // First-seen c35.rgb=0. c3 leftover is table (0, 0.125, 0, 0).
            vec3 ldir = -pc.lightDir.xyz;
            float ndl = nlen < 1e-8 ? 0.0 : max(dot(normalize(n), ldir), 0.0);
            vec3 litAdd = pc.pass.yzw;
            vec3 c3 = vec3(0.0, 0.125, 0.0);
            vec3 v0 = fragColor.rgb * (pc.lightColor.rgb * (ndl * ndl) + litAdd + c3);
            vec3 lit;
            // STATIC/PALSKIN/BG: mov oD0.w, c0.y (first-seen 1).
            // FG: (dp3(r2,c42)+c42.w)*v3.x; first-seen c42=0 so 0.
            float v0a = (mode > 0.5 && mode < 1.5) ? 0.0 : 1.0;
            float alpha = 1.0;
            if (mode < 0.5)
            {
                // PSHADER_LANDSCAPE_BACKGROUND: mul_x2_sat t0 * v0
                lit = clamp(t0.rgb * v0 * 2.0, 0.0, 1.0);
                alpha = clamp(t0.a * v0a, 0.0, 1.0);
            }
            else if (mode < 1.5)
            {
                // PSHADER_LANDSCAPE_FOREGROUND: mul_x2_sat t1 * v0; mul_sat t0.a * v0.a
                lit = clamp(t1.rgb * v0 * 2.0, 0.0, 1.0);
                alpha = clamp(t0.a * v0a, 0.0, 1.0);
            }
            else if (mode < 2.5)
            {
                // PSHADER_INNER_SKY / SIMPLE: mul_sat rgb*v0 then *v0.w.
                // mul_x2 c2/c1 has no first-seen writer; do not invent 0.
                lit = t1.rgb * fragColor.rgb * fragColor.a;
            }
            else
            {
                // PSHADER_TEXTURE_DIFFUSE: mul v0*c0 then mul_x2 t0.
                // First-seen c0 is PSCONST_OUTPUT_FACTOR = (1,1,1,1).
                lit = clamp(t0.rgb * v0 * 2.0, 0.0, 1.0);
            }
            // Land/static/PALSKIN write oFog. INNER_SKY does not
            // (FirstSeenInnerSkyWritesFog=false); D3D default 1.
            float fog = (mode > 1.5 && mode < 2.5) ? 1.0 : fragFog;
            // FOGENABLE=1, FOGCOLOR black: rgb * oFog + (1-oFog) * 0
            outColor = vec4(lit * fog, alpha);
        }
        """;
}
