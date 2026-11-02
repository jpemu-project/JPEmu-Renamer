// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;

namespace JPEmuRenamer.Ui;

/// <summary>
/// 走査線・CRT風の見た目をつけるピクセルシェーダー。
/// HLSL を起動時に D3DCompiler（Windows標準の d3dcompiler_47.dll）でコンパイルする。
/// </summary>
public sealed class RetroShaderEffect : ShaderEffect
{
    private const string Common = @"
sampler2D input : register(s0);
float4 size : register(c0); // x,y = ゲームの解像度、z,w = 表示上の大きさ
static const float PI = 3.14159265;
";

    private const string ScanlinesSource = Common + @"
float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 c = tex2D(input, uv);
    float l = frac(uv.y * size.y);
    float s = 0.55 + 0.6 * pow(sin(l * PI), 1.4);
    return float4(saturate(c.rgb * s), c.a);
}";

    private const string CrtSource = Common + @"
float4 main(float2 uv : TEXCOORD) : COLOR
{
    float2 cc = uv * 2.0 - 1.0;
    float2 bent = cc * (1.0 + float2(cc.y * cc.y, cc.x * cc.x) * 0.045);
    float2 p = bent * 0.5 + 0.5;
    if (p.x < 0.0 || p.x > 1.0 || p.y < 0.0 || p.y > 1.0) return float4(0, 0, 0, 1);
    float4 c = tex2D(input, p);
    float l = frac(p.y * size.y);
    float s = 0.5 + 0.65 * pow(sin(l * PI), 1.6);
    float m = fmod(floor(uv.x * size.z), 3.0);
    float3 mask = m < 1.0 ? float3(1.0, 0.8, 0.8) : (m < 2.0 ? float3(0.8, 1.0, 0.8) : float3(0.8, 0.8, 1.0));
    float v = pow(16.0 * p.x * p.y * (1.0 - p.x) * (1.0 - p.y), 0.18);
    return float4(saturate(c.rgb * s * mask * v * 1.15), 1.0);
}";

    private static readonly Dictionary<string, PixelShader?> Cache = new();

    public static readonly DependencyProperty InputProperty =
        RegisterPixelShaderSamplerProperty("Input", typeof(RetroShaderEffect), 0, SamplingMode.NearestNeighbor);

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register("Size", typeof(Point4D), typeof(RetroShaderEffect),
            new UIPropertyMetadata(new Point4D(256, 240, 768, 720), PixelShaderConstantCallback(0)));

    public Brush Input
    {
        get => (Brush)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }

    /// <summary>x,y = ゲームの解像度、z,w = 表示上の大きさ（ピクセル）</summary>
    public Point4D Size
    {
        get => (Point4D)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    private RetroShaderEffect(PixelShader shader)
    {
        PixelShader = shader;
        UpdateShaderValue(InputProperty);
        UpdateShaderValue(SizeProperty);
    }

    /// <summary>"Scanlines" / "Crt" の効果を作る。コンパイルできなければ null</summary>
    public static RetroShaderEffect? Create(string kind)
    {
        if (!Cache.TryGetValue(kind, out var shader))
        {
            var src = kind switch { "Scanlines" => ScanlinesSource, "Crt" => CrtSource, _ => null };
            shader = src == null ? null : Compile(src);
            Cache[kind] = shader;
        }
        return shader == null ? null : new RetroShaderEffect(shader);
    }

    [DllImport("d3dcompiler_47.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern unsafe int D3DCompile(byte* src, nuint srcSize, string? sourceName, IntPtr defines, IntPtr include,
        [MarshalAs(UnmanagedType.LPStr)] string entry, [MarshalAs(UnmanagedType.LPStr)] string target, uint flags1, uint flags2,
        out IntPtr code, out IntPtr errors);

    private static unsafe PixelShader? Compile(string source)
    {
        try
        {
            var bytes = System.Text.Encoding.ASCII.GetBytes(source);
            IntPtr code, errors;
            int hr;
            fixed (byte* p = bytes)
                hr = D3DCompile(p, (nuint)bytes.Length, null, IntPtr.Zero, IntPtr.Zero, "main", "ps_3_0", 1 << 15 /* OPTIMIZATION_LEVEL3 */, 0, out code, out errors);
            if (errors != IntPtr.Zero) Release(errors);
            if (hr < 0 || code == IntPtr.Zero) return null;
            byte[] blob = BlobBytes(code);
            Release(code);
            var ps = new PixelShader();
            ps.SetStreamSource(new MemoryStream(blob));
            ps.Freeze();
            return ps;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            return null;
        }
    }

    // ID3DBlob: QueryInterface, AddRef, Release, GetBufferPointer, GetBufferSize
    private static unsafe byte[] BlobBytes(IntPtr blob)
    {
        var vtbl = *(IntPtr**)blob;
        var getPtr = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr>)vtbl[3];
        var getSize = (delegate* unmanaged[Stdcall]<IntPtr, nuint>)vtbl[4];
        var ptr = getPtr(blob);
        var size = (int)getSize(blob);
        var result = new byte[size];
        Marshal.Copy(ptr, result, 0, size);
        return result;
    }

    private static unsafe void Release(IntPtr unk)
    {
        var vtbl = *(IntPtr**)unk;
        ((delegate* unmanaged[Stdcall]<IntPtr, uint>)vtbl[2])(unk);
    }
}
