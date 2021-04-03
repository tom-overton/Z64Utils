﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing;

namespace F3DZEX.Render
{

    [Serializable]
    public class ShaderException : Exception
    {
        public ShaderException() { }
        public ShaderException(string message) : base(message) { }
        public ShaderException(string message, Exception inner) : base(message, inner) { }
        protected ShaderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class ShaderHandler
    {
        private int _program;
        private Dictionary<string, int> _uniformLocations;

        private class TempResUsage<T> : IDisposable
        {
            public T value;

            Action<TempResUsage<T>> _dispose;

            public TempResUsage(Action<TempResUsage<T>> create = null, Action<TempResUsage<T>> dispose = null)
            {
                create?.Invoke(this);
                _dispose = dispose;
            }

            public void Dispose()
            {
                _dispose?.Invoke(this);
            }
        }

        public ShaderHandler(string vertPath, string fragPath, string geomPath = null)
        {
            List<int> shaders = new List<int>();

            shaders.Add(CompileShader(vertPath, ShaderType.VertexShader));

            if (!string.IsNullOrEmpty(geomPath))
                shaders.Add(CompileShader(geomPath, ShaderType.GeometryShader));

            shaders.Add(CompileShader(fragPath, ShaderType.FragmentShader));

            LinkShaders(shaders.ToArray());

            _uniformLocations = new Dictionary<string, int>();
        }

        private int CompileShader(string path, ShaderType type)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, File.ReadAllText(path));
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);

            GL.GetShaderInfoLog(shader, out string info);
            if (!string.IsNullOrEmpty(info))
                throw new ShaderException($"Failed to compile \"{path}\" : \n{info}");

            return shader;
        }

        private void LinkShaders(params int[] shaders)
        {
            _program = GL.CreateProgram();

            foreach (var shader in shaders)
                GL.AttachShader(_program, shader);

            GL.LinkProgram(_program);

            GL.GetProgram(_program, GetProgramParameterName.LinkStatus, out int status);
            GL.GetProgramInfoLog(_program, out string info);
            if (!string.IsNullOrEmpty(info))
                throw new ShaderException($"Failed to link shaders : \r{info}");

            foreach (var shader in shaders)
            {
                GL.DetachShader(_program, shader);
                GL.DeleteShader(shader);
            }
        }

        public void Use()
        {
            GL.UseProgram(_program);
        }

        private TempResUsage<int> TempUse()
        {
            var ret = new TempResUsage<int>(
                t => t.value = GL.GetInteger(GetPName.CurrentProgram),
                t => GL.UseProgram(t.value)
            );
            Use();
            return ret;
        }

        private int GetUniformLocation(string name)
        {
            if (!_uniformLocations.TryGetValue(name, out int location))
            {
                location = GL.GetUniformLocation(_program, name);
                _uniformLocations.Add(name, location);
            }

            return location;
        }

        public void Send(string name, float data)
        {
            using (TempUse())
                GL.Uniform1(GetUniformLocation(name), data);
        }
        public void Send(string name, int data)
        {
            using (TempUse())
                GL.Uniform1(GetUniformLocation(name), data);
        }
        public void Send(string name, float x, float y)
        {
            using (TempUse())
                GL.Uniform2(GetUniformLocation(name), x, y);
        }
        public void Send(string name, float x, float y, float z)
        {
            using (TempUse())
                GL.Uniform3(GetUniformLocation(name), x, y, z);
        }
        public void Send(string name, float x, float y, float z, float w)
        {
            using (TempUse())
                GL.Uniform4(GetUniformLocation(name), x, y, z, w);
        }
        public void Send(string name, Matrix4 mtx)
        {
            using (TempUse())
                GL.UniformMatrix4(GetUniformLocation(name), false, ref mtx);
        }
        public void Send(string name, Color color)
        {
            using (TempUse())
                GL.Uniform4(GetUniformLocation(name), color);
        }
        public void Send(string name, bool x)
        {
            using (TempUse())
                GL.Uniform1(GetUniformLocation(name), x ? 1 : 0);
        }
    }
}
