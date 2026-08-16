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
            vec4 lightDir;
            vec4 lightColor;
            vec4 pass;
        } pc;
        void main() {
            vec4 t0 = texture(albedo0, fragUv);
            vec4 t1 = texture(albedo1, fragUv);
            vec3 n = fragNormal;
            float nlen = length(n);
            float ndl = nlen < 0.1 ? 1.0 : max(dot(normalize(n), pc.lightDir.xyz), 0.0);
            // VS: mul r3, NdotL, c20; mad r3, *, c35, r3. c35 rgb is 0 at TOD 0.
            vec3 v0 = fragColor.rgb * (pc.lightColor.rgb * ndl + pc.pass.yzw);
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
            outColor = vec4(lit, 1.0);
        }
        """;
}
