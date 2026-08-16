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
        layout(location = 2) in vec3 inColor;
        layout(location = 0) out vec3 fragNormal;
        layout(location = 1) out vec3 fragColor;
        layout(push_constant) uniform Push { mat4 viewProj; } pc;
        void main() {
            gl_Position = pc.viewProj * vec4(inPosition, 1.0);
            fragNormal = inNormal;
            fragColor = inColor;
        }
        """;

    public const string MeshFragment = """
        #version 450
        layout(location = 0) in vec3 fragNormal;
        layout(location = 1) in vec3 fragColor;
        layout(location = 0) out vec4 outColor;
        void main() {
            vec3 n = normalize(fragNormal);
            vec3 light = normalize(vec3(0.35, 0.15, 0.92));
            float ndl = max(dot(n, light), 0.0);
            vec3 color = fragColor * (0.22 + 0.78 * ndl);
            outColor = vec4(color, 1.0);
        }
        """;
}
