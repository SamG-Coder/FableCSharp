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

    public const string MeshVertex = """
        #version 450
        layout(location = 0) in vec3 inPosition;
        layout(location = 1) in vec3 inNormal;
        layout(location = 2) in vec2 inUv;
        layout(location = 3) in vec4 inColor;
        layout(location = 0) out vec3 fragNormal;
        layout(location = 1) out vec2 fragUv;
        layout(location = 2) out vec4 fragColor;
        layout(location = 3) out vec3 fragWorld;
        layout(location = 4) out float fragFog;
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
            vec4 t0 = texture(albedo0, fragUv);
            vec4 t1 = texture(albedo1, fragUv);
            vec3 n = fragNormal;
            float nlen = length(n);
            // VS: dp3 r, n, -c19; max r.x, r, c0.x; mul r.x, r.x, r.x;
            // mul r, r.x, c20; mad r, -r.y, c35, r. c35.rgb = 0.
            vec3 ldir = -pc.lightDir.xyz;
            float ndl = nlen < 1e-8 ? 0.0 : max(dot(normalize(n), ldir), 0.0);
            vec3 litAdd = pc.pass.yzw;
            vec3 v0 = fragColor.rgb * (pc.lightColor.rgb * (ndl * ndl) + litAdd);
            float mode = pc.pass.x;
            vec3 lit;
            if (mode < 0.5)
                lit = t0.rgb * v0;
            else if (mode < 1.5)
                lit = clamp(t1.rgb * v0 * 2.0, 0.0, 1.0);
            else if (mode < 2.5)
                lit = t1.rgb * fragColor.rgb;
            else
                lit = t1.rgb * v0;
            // FOGENABLE=1, FOGCOLOR black: rgb * oFog + (1-oFog) * 0
            outColor = vec4(lit * fragFog, 1.0);
        }
        """;
}
