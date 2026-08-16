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
        layout(push_constant) uniform Push {
            mat4 viewProj;
            vec4 cameraPos;
            vec4 fogColor;
            vec4 lightDir;
        } pc;
        void main() {
            gl_Position = pc.viewProj * vec4(inPosition, 1.0);
            fragNormal = inNormal;
            fragUv = inUv;
            fragColor = inColor;
            fragWorld = inPosition;
        }
        """;

    public const string MeshFragment = """
        #version 450
        layout(location = 0) in vec3 fragNormal;
        layout(location = 1) in vec2 fragUv;
        layout(location = 2) in vec4 fragColor;
        layout(location = 3) in vec3 fragWorld;
        layout(location = 0) out vec4 outColor;
        layout(set = 0, binding = 0) uniform sampler2D albedo0;
        layout(set = 1, binding = 0) uniform sampler2D albedo1;
        layout(push_constant) uniform Push {
            mat4 viewProj;
            vec4 cameraPos;
            vec4 fogColor;
            vec4 lightDir;
        } pc;
        void main() {
            vec4 t0 = texture(albedo0, fragUv);
            vec4 t1 = texture(albedo1, fragUv);
            vec3 tex = mix(t0.rgb, t1.rgb, t1.a);
            vec3 n = fragNormal;
            float nlen = length(n);
            float ndl = nlen < 0.1 ? 1.0 : max(dot(normalize(n), normalize(pc.lightDir.xyz)), 0.0);
            vec3 lit = tex * fragColor.rgb * (0.28 + 0.72 * ndl);
            float fogEnd = max(pc.cameraPos.w, pc.fogColor.w + 1.0);
            float fog = clamp((length(pc.cameraPos.xyz - fragWorld) - pc.fogColor.w) / (fogEnd - pc.fogColor.w), 0.0, 1.0);
            if (nlen < 0.1)
                fog *= 0.35;
            outColor = vec4(mix(lit, pc.fogColor.rgb, fog), 1.0);
        }
        """;
}
