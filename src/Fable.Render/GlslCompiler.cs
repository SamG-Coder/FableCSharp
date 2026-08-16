using System.Runtime.InteropServices;
using Silk.NET.Shaderc;

namespace Fable.Render;

internal static class GlslCompiler
{
    public static unsafe byte[] Compile(string source, ShaderKind kind, string fileName)
    {
        var api = Shaderc.GetApi();
        var compiler = api.CompilerInitialize();
        var options = api.CompileOptionsInitialize();
        try
        {
            api.CompileOptionsSetSourceLanguage(options, SourceLanguage.Glsl);
            api.CompileOptionsSetTargetEnv(options, TargetEnv.Vulkan, (uint)EnvVersion.Vulkan12);
            api.CompileOptionsSetOptimizationLevel(options, OptimizationLevel.Performance);

            var result = api.CompileIntoSpv(
                compiler,
                source,
                (nuint)source.Length,
                kind,
                fileName,
                "main",
                options);

            try
            {
                var status = api.ResultGetCompilationStatus(result);
                if (status != CompilationStatus.Success)
                {
                    var message = Marshal.PtrToStringAnsi((nint)api.ResultGetErrorMessage(result));
                    throw new InvalidOperationException($"GLSL compile failed ({status}): {message}");
                }

                var length = (int)api.ResultGetLength(result);
                var bytes = api.ResultGetBytes(result);
                var spirv = new byte[length];
                Marshal.Copy((nint)bytes, spirv, 0, length);
                return spirv;
            }
            finally
            {
                api.ResultRelease(result);
            }
        }
        finally
        {
            api.CompileOptionsRelease(options);
            api.CompilerRelease(compiler);
            api.Dispose();
        }
    }
}
