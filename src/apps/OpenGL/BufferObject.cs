using System;
using Silk.NET.OpenGL;

// ReSharper disable InconsistentNaming
namespace Friflo.Engine.OpenGL;

public class BufferObject<TDataType> : IDisposable
    where TDataType : unmanaged
{
    private readonly BufferTargetARB _bufferType;
    private readonly GL _gl;
    private readonly uint _handle;

    public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
    {
        _gl = gl;
        _bufferType = bufferType;
        //Clear existing error code.
        GLEnum error;
        do
        {
            error = _gl.GetError();
        } while (error != GLEnum.NoError);
        _handle = _gl.GenBuffer();
        Bind();
        GlErrorException.ThrowIfError(gl);
        fixed (void* d = data)
        {
            _gl.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
        }
        GlErrorException.ThrowIfError(gl);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(_handle);
    }

    public void Bind()
    {
        _gl.BindBuffer(_bufferType, _handle);
    }
}
