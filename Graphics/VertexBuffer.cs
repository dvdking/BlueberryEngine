using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Blueberry.Diagnostics;

namespace Blueberry.Graphics
{
    public class VertexBuffer:IDiagnosable
    {
        struct VertexDeclaration
        {
            public string name;
            public int elements;
        }

        public int VertexArrayObject { get; private set; }

        public int VertexDataBufferObject { get; private set; }

        public int IndexDataBufferObject { get; private set; }

        public BufferUsageHint UsageMode { get; set; }

        float[] vertexData;
        int[] indexData;

        public float[] VertexData { get { return vertexData; } }

        public int[] IndexData { get { return indexData; } }

        List<VertexDeclaration> declarations;

        public int VertexOffset { get { return voffset; } }

        public int IndexOffset { get { return ioffset; } }

        int voffset;
        int ioffset;

        int stride;
        public int Stride
        {
            get
            {
                return stride;
            }
        }

        public VertexBuffer()
            : this(1024)
        {
        }

        public VertexBuffer(int capacity)
        {
            declarations = new List<VertexDeclaration>();

            int tmp;
            GL.GenVertexArrays(1, out tmp);
            VertexArrayObject = tmp;
            GL.BindVertexArray(VertexArrayObject);

            GL.GenBuffers(1, out tmp);
            VertexDataBufferObject = tmp;

            GL.GenBuffers(1, out tmp);
            IndexDataBufferObject = tmp;

            UsageMode = BufferUsageHint.DynamicDraw;

            vertexData = new float[capacity];
            indexData = new int[capacity * 3];
            voffset = 0;
            ioffset = 0;
            stride = 0;
        }

        public void Dispose()
        {
            int id;

            id = VertexDataBufferObject;
            GL.DeleteBuffers(1, ref id);
            VertexDataBufferObject = -1;

            id = IndexDataBufferObject;
            GL.DeleteBuffers(1, ref id);
            IndexDataBufferObject = -1;

            GC.SuppressFinalize(this);
        }

        public void DeclareNextAttribute(string name, int elements)
        {
            declarations.Add(new VertexDeclaration() { name = name, elements = elements });
            stride += elements;
        }
        public void ClearAttributeDeclarations()
        {
            declarations.Clear();
            stride = 0;
        }
        public void AddVertex(params float[] data)
        {
            CheckForOverflowVertexBuffer();
            int n = data.Length;
            for (int i = 0; i < stride; i++)
                vertexData[voffset++] = n <= i ? 0.0f : data[i];

        }

        public void AddVertices(int count, params float[] data)
        {
            CheckForOverflowVertexBuffer(count);
            int n = data.Length;
            for (int i = 0; i < count * stride; i++)
            {
                vertexData[voffset++] = n <= i ? 0.0f : data[i];
            }
        }

        public void AddIndices(params int[] data)
        {
            CheckForOverflowIndexBuffer(data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                indexData[ioffset++] = data[i];
            }
        }

        public void UpdateBuffer()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexDataBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(voffset * sizeof(float)), vertexData, UsageMode);

            GL.BindBuffer(BufferTarget.ArrayBuffer, IndexDataBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(ioffset * sizeof(int)), indexData, UsageMode);
        }

        public void UpdateVertexBuffer()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexDataBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(voffset * sizeof(float)), vertexData, UsageMode);
        }

        public void UpdateIndexBuffer()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, IndexDataBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(ioffset * sizeof(int)), indexData, UsageMode);
        }

        public void Attach(Shader shader)
        {
            if (!Bind())
                return;
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexDataBufferObject);
            shader.Use();
            int off = 0;
            for (int i = 0; i < declarations.Count; i++)
            {
                int location = GL.GetAttribLocation(shader.Handle, declarations[i].name);
                if (location != -1)
                {
                    GL.VertexAttribPointer(location, declarations[i].elements, VertexAttribPointerType.Float, false, stride * sizeof(float), off);
                    GL.EnableVertexAttribArray(location);
                }
                off += declarations[i].elements * sizeof(float);
            }
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexDataBufferObject);
        }

        public bool Bind()
        {
            if (VertexArrayObject == -1)
                return false;

            GL.BindVertexArray(VertexArrayObject);
            return true;
        }

        public void ClearBuffer()
        {
            voffset = 0;
            ioffset = 0;
        }
        public void ClearVertices()
        {
            voffset = 0;
        }
        public void ClearIndices()
        {
            ioffset = 0;
        }

        private void CheckForOverflowVertexBuffer(int add = 1)
        {
            int sum = voffset + (stride * add);

            while (sum > vertexData.Length)
            {
                Array.Resize<float>(ref vertexData, vertexData.Length * 2);
            }
        }

        private void CheckForOverflowIndexBuffer(int add = 1)
        {
            int sum = ioffset + add;

            while (sum > indexData.Length)
            {
                Array.Resize<int>(ref indexData, indexData.Length * 2);
            }
        }

        public void DebugAction()
        {
            throw new NotImplementedException();
        }

        public string DebugInfo()
        {
            return "Vertex buffer: " + vertexData.Length +";"+
                    "Index buffer: " + indexData.Length + ";";
        }

        public string DebugName
        {
            get { return "Vertex buffer"; }
        }
    }
}