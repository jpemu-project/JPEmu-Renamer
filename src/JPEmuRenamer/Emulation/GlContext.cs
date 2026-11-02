// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Runtime.InteropServices;

namespace JPEmuRenamer.Emulation;

/// <summary>
/// ハードウェア描画（OpenGL）のコア向けの描画環境。
/// 見えないウィンドウに OpenGL の文脈を作り、フレームバッファ（FBO）にコアが描いた結果を読み出して WPF に渡す。
/// 作成・使用・破棄はすべてエミュレーションスレッドで行う。
/// </summary>
public sealed unsafe class GlContext : IDisposable
{
    private const uint GL_TEXTURE_2D = 0x0DE1, GL_RGBA8 = 0x8058, GL_RGBA = 0x1908, GL_BGRA = 0x80E1, GL_UNSIGNED_BYTE = 0x1401,
        GL_FRAMEBUFFER = 0x8D40, GL_READ_FRAMEBUFFER = 0x8CA8, GL_RENDERBUFFER = 0x8D41, GL_COLOR_ATTACHMENT0 = 0x8CE0,
        GL_DEPTH_STENCIL_ATTACHMENT = 0x821A, GL_DEPTH_ATTACHMENT = 0x8D00, GL_DEPTH24_STENCIL8 = 0x88F0,
        GL_DEPTH_COMPONENT24 = 0x81A6, GL_PACK_ALIGNMENT = 0x0D05, GL_TEXTURE_MIN_FILTER = 0x2801, GL_TEXTURE_MAG_FILTER = 0x2800,
        GL_NEAREST = 0x2600, GL_FRAMEBUFFER_COMPLETE = 0x8CD5;

    private const int WGL_CONTEXT_MAJOR_VERSION_ARB = 0x2091, WGL_CONTEXT_MINOR_VERSION_ARB = 0x2092,
        WGL_CONTEXT_FLAGS_ARB = 0x2094, WGL_CONTEXT_PROFILE_MASK_ARB = 0x9126,
        WGL_CONTEXT_CORE_PROFILE_BIT_ARB = 1, WGL_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB = 2, WGL_CONTEXT_DEBUG_BIT_ARB = 1;

    [StructLayout(LayoutKind.Sequential)]
    private struct PIXELFORMATDESCRIPTOR
    {
        public ushort nSize, nVersion;
        public uint dwFlags;
        public byte iPixelType, cColorBits, cRedBits, cRedShift, cGreenBits, cGreenShift, cBlueBits, cBlueShift,
            cAlphaBits, cAlphaShift, cAccumBits, cAccumRedBits, cAccumGreenBits, cAccumBlueBits, cAccumAlphaBits,
            cDepthBits, cStencilBits, cAuxBuffers, iLayerType, bReserved;
        public uint dwLayerMask, dwVisibleMask, dwDamageMask;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowEx(int exStyle, string className, string windowName, int style, int x, int y, int w, int h,
        IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);
    [DllImport("user32.dll")] private static extern bool DestroyWindow(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);
    [DllImport("gdi32.dll")] private static extern int ChoosePixelFormat(IntPtr dc, ref PIXELFORMATDESCRIPTOR pfd);
    [DllImport("gdi32.dll")] private static extern bool SetPixelFormat(IntPtr dc, int format, ref PIXELFORMATDESCRIPTOR pfd);
    [DllImport("opengl32.dll")] private static extern IntPtr wglCreateContext(IntPtr dc);
    [DllImport("opengl32.dll")] private static extern bool wglMakeCurrent(IntPtr dc, IntPtr rc);
    [DllImport("opengl32.dll")] private static extern bool wglDeleteContext(IntPtr rc);
    [DllImport("opengl32.dll", CharSet = CharSet.Ansi)] private static extern IntPtr wglGetProcAddress(string name);
    [DllImport("opengl32.dll")] private static extern void glReadPixels(int x, int y, int w, int h, uint format, uint type, void* data);
    [DllImport("opengl32.dll")] private static extern void glPixelStorei(uint pname, int param);
    [DllImport("opengl32.dll")] private static extern void glGenTextures(int n, uint* textures);
    [DllImport("opengl32.dll")] private static extern void glDeleteTextures(int n, uint* textures);
    [DllImport("opengl32.dll")] private static extern void glBindTexture(uint target, uint texture);
    [DllImport("opengl32.dll")] private static extern void glTexParameteri(uint target, uint pname, int param);
    [DllImport("opengl32.dll")] private static extern void glTexImage2D(uint target, int level, int internalFormat, int w, int h, int border, uint format, uint type, void* data);
    [DllImport("opengl32.dll")] private static extern void glFinish();
    [DllImport("kernel32.dll", CharSet = CharSet.Ansi)] private static extern IntPtr GetProcAddress(IntPtr module, string name);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr GetModuleHandle(string name);

    private readonly IntPtr hwnd, dc;
    private IntPtr rc;
    private readonly IntPtr opengl32;
    private uint fbo, colorTex, depthRb;
    private int fbWidth, fbHeight;

    private readonly delegate* unmanaged[Stdcall]<int, uint*, void> glGenFramebuffers;
    private readonly delegate* unmanaged[Stdcall]<int, uint*, void> glDeleteFramebuffers;
    private readonly delegate* unmanaged[Stdcall]<uint, uint, void> glBindFramebuffer;
    private readonly delegate* unmanaged[Stdcall]<uint, uint, uint, uint, int, void> glFramebufferTexture2D;
    private readonly delegate* unmanaged[Stdcall]<int, uint*, void> glGenRenderbuffers;
    private readonly delegate* unmanaged[Stdcall]<int, uint*, void> glDeleteRenderbuffers;
    private readonly delegate* unmanaged[Stdcall]<uint, uint, void> glBindRenderbuffer;
    private readonly delegate* unmanaged[Stdcall]<uint, uint, int, int, void> glRenderbufferStorage;
    private readonly delegate* unmanaged[Stdcall]<uint, uint, uint, uint, void> glFramebufferRenderbuffer;
    private readonly delegate* unmanaged[Stdcall]<uint, uint> glCheckFramebufferStatus;

    public bool Depth { get; }
    public bool Stencil { get; }
    public bool BottomLeftOrigin { get; }
    public uint Framebuffer => fbo;

    public GlContext(RetroHwRenderCallback hw, int width, int height)
    {
        Depth = hw.Depth != 0;
        Stencil = hw.Stencil != 0;
        BottomLeftOrigin = hw.BottomLeftOrigin != 0;

        hwnd = CreateWindowEx(0, "STATIC", "JPEmuGL", 0, 0, 0, 16, 16, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        if (hwnd == IntPtr.Zero) throw new InvalidOperationException("CreateWindowEx failed");
        dc = GetDC(hwnd);
        var pfd = new PIXELFORMATDESCRIPTOR
        {
            nSize = (ushort)sizeof(PIXELFORMATDESCRIPTOR),
            nVersion = 1,
            dwFlags = 0x4 | 0x20 | 0x1, // DRAW_TO_WINDOW | SUPPORT_OPENGL | DOUBLEBUFFER
            iPixelType = 0,
            cColorBits = 32,
            cDepthBits = 24,
            cStencilBits = 8
        };
        int pf = ChoosePixelFormat(dc, ref pfd);
        if (pf == 0 || !SetPixelFormat(dc, pf, ref pfd)) throw new InvalidOperationException("SetPixelFormat failed");

        // まず互換の文脈を作り、wglCreateContextAttribsARB があれば要求された版で作り直す
        var legacy = wglCreateContext(dc);
        if (legacy == IntPtr.Zero || !wglMakeCurrent(dc, legacy)) throw new InvalidOperationException("wglCreateContext failed");
        rc = legacy;
        var createAttribs = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int*, IntPtr>)wglGetProcAddress("wglCreateContextAttribsARB");
        bool wantCore = hw.ContextType == Retro.HwContextOpenGlCore || hw.VersionMajor >= 3;
        if (createAttribs != null && wantCore)
        {
            int major = (int)Math.Max(3, hw.VersionMajor), minor = (int)hw.VersionMinor;
            int* attribs = stackalloc int[]
            {
                WGL_CONTEXT_MAJOR_VERSION_ARB, major,
                WGL_CONTEXT_MINOR_VERSION_ARB, minor,
                WGL_CONTEXT_PROFILE_MASK_ARB, hw.ContextType == Retro.HwContextOpenGlCore ? WGL_CONTEXT_CORE_PROFILE_BIT_ARB : WGL_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB,
                WGL_CONTEXT_FLAGS_ARB, hw.DebugContext != 0 ? WGL_CONTEXT_DEBUG_BIT_ARB : 0,
                0
            };
            var modern = createAttribs(dc, IntPtr.Zero, attribs);
            if (modern != IntPtr.Zero)
            {
                wglMakeCurrent(dc, modern);
                wglDeleteContext(legacy);
                rc = modern;
            }
        }

        opengl32 = GetModuleHandle("opengl32.dll");
        glGenFramebuffers = (delegate* unmanaged[Stdcall]<int, uint*, void>)Proc("glGenFramebuffers");
        glDeleteFramebuffers = (delegate* unmanaged[Stdcall]<int, uint*, void>)Proc("glDeleteFramebuffers");
        glBindFramebuffer = (delegate* unmanaged[Stdcall]<uint, uint, void>)Proc("glBindFramebuffer");
        glFramebufferTexture2D = (delegate* unmanaged[Stdcall]<uint, uint, uint, uint, int, void>)Proc("glFramebufferTexture2D");
        glGenRenderbuffers = (delegate* unmanaged[Stdcall]<int, uint*, void>)Proc("glGenRenderbuffers");
        glDeleteRenderbuffers = (delegate* unmanaged[Stdcall]<int, uint*, void>)Proc("glDeleteRenderbuffers");
        glBindRenderbuffer = (delegate* unmanaged[Stdcall]<uint, uint, void>)Proc("glBindRenderbuffer");
        glRenderbufferStorage = (delegate* unmanaged[Stdcall]<uint, uint, int, int, void>)Proc("glRenderbufferStorage");
        glFramebufferRenderbuffer = (delegate* unmanaged[Stdcall]<uint, uint, uint, uint, void>)Proc("glFramebufferRenderbuffer");
        glCheckFramebufferStatus = (delegate* unmanaged[Stdcall]<uint, uint>)Proc("glCheckFramebufferStatus");
        if (glGenFramebuffers == null || glBindFramebuffer == null)
            throw new NotSupportedException("OpenGL framebuffer objects are not available.");

        Resize(width, height);
    }

    /// <summary>コア向けの関数アドレス（拡張関数は wglGetProcAddress、1.1の関数は opengl32.dll から）</summary>
    public IntPtr Proc(string name)
    {
        var p = wglGetProcAddress(name);
        long v = p.ToInt64();
        if (v is 0 or 1 or 2 or 3 or -1) p = GetProcAddress(opengl32, name);
        return p;
    }

    /// <summary>描画先の大きさを変える（コアの最大解像度に合わせる）</summary>
    public void Resize(int width, int height)
    {
        width = Math.Max(1, width);
        height = Math.Max(1, height);
        if (width == fbWidth && height == fbHeight && fbo != 0) return;
        DeleteFramebuffer();
        fbWidth = width;
        fbHeight = height;

        uint t, f, r;
        glGenTextures(1, &t);
        colorTex = t;
        glBindTexture(GL_TEXTURE_2D, colorTex);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, (int)GL_NEAREST);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, (int)GL_NEAREST);
        glTexImage2D(GL_TEXTURE_2D, 0, (int)GL_RGBA8, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, null);
        glBindTexture(GL_TEXTURE_2D, 0);

        glGenFramebuffers(1, &f);
        fbo = f;
        glBindFramebuffer(GL_FRAMEBUFFER, fbo);
        glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, colorTex, 0);

        if (Depth || Stencil)
        {
            glGenRenderbuffers(1, &r);
            depthRb = r;
            glBindRenderbuffer(GL_RENDERBUFFER, depthRb);
            glRenderbufferStorage(GL_RENDERBUFFER, Stencil ? GL_DEPTH24_STENCIL8 : GL_DEPTH_COMPONENT24, width, height);
            glFramebufferRenderbuffer(GL_FRAMEBUFFER, Stencil ? GL_DEPTH_STENCIL_ATTACHMENT : GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, depthRb);
            glBindRenderbuffer(GL_RENDERBUFFER, 0);
        }

        if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
            throw new NotSupportedException("OpenGL framebuffer is incomplete.");
        glBindFramebuffer(GL_FRAMEBUFFER, 0);
    }

    /// <summary>コアが描いた w×h を BGRA32（上が先頭）で dest に読み出す</summary>
    public void ReadFrame(int w, int h, int[] dest, int destStride)
    {
        w = Math.Min(w, fbWidth);
        h = Math.Min(h, fbHeight);
        glBindFramebuffer(GL_READ_FRAMEBUFFER, fbo);
        glPixelStorei(GL_PACK_ALIGNMENT, 4);
        var tmp = new int[w * h];
        fixed (int* p = tmp)
            glReadPixels(0, 0, w, h, GL_BGRA, GL_UNSIGNED_BYTE, p);
        glBindFramebuffer(GL_READ_FRAMEBUFFER, 0);

        for (int y = 0; y < h; y++)
        {
            int srcRow = BottomLeftOrigin ? h - 1 - y : y;
            Array.Copy(tmp, srcRow * w, dest, y * destStride, w);
        }
        // 読み出した値はアルファが0のことがあるので不透明にする
        for (int y = 0; y < h; y++)
        {
            int o = y * destStride;
            for (int x = 0; x < w; x++) dest[o + x] |= unchecked((int)0xFF000000);
        }
    }

    private void DeleteFramebuffer()
    {
        uint v;
        if (fbo != 0) { v = fbo; glDeleteFramebuffers(1, &v); fbo = 0; }
        if (colorTex != 0) { v = colorTex; glDeleteTextures(1, &v); colorTex = 0; }
        if (depthRb != 0) { v = depthRb; glDeleteRenderbuffers(1, &v); depthRb = 0; }
    }

    public void Dispose()
    {
        try
        {
            if (rc != IntPtr.Zero)
            {
                DeleteFramebuffer();
                glFinish();
                wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
                wglDeleteContext(rc);
                rc = IntPtr.Zero;
            }
        }
        catch { }
        if (dc != IntPtr.Zero) ReleaseDC(hwnd, dc);
        if (hwnd != IntPtr.Zero) DestroyWindow(hwnd);
    }
}
