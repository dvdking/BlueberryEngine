using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Blueberry.Diagnostics;

namespace Blueberry.Graphics
{
    public unsafe class VertexBuffer:IDiagnosable, IDisposable
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

        private IntPtr _indexDataPointer;
        internal int* IndexData;
        private int indexDataLength;
        
        
        private IntPtr _vertexDataPointer;
		internal float* VertexData;
        private int vertexDataLength;
        
        public int VertexDataLength { get { return vertexDataLength; } }
        public int IndexDataLength { get { return indexDataLength; } }

        private List<VertexDeclaration> declarations;

        public int VertexOffset { get { return voffset; } }

        public int IndexOffset { get { return ioffset; } }

        int voffset;
        int ioffset;

        int stride;
        public int Stride { get { return stride; } }
        
        public VertexBuffer()
            : this(1024, 1024)
        {
        }

        public VertexBuffer(int capacity)
            : this(capacity, capacity)
        {
        }

        public VertexBuffer(int vertexCapacity, int indexCapacity)
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
			
            vertexDataLength = vertexCapacity;
            indexDataLength = indexCapacity;
            
            _indexDataPointer = Marshal.AllocHGlobal(indexDataLength * sizeof(int));
            IndexData = (int*)_indexDataPointer.ToPointer();
            
            _vertexDataPointer = Marshal.AllocHGlobal(vertexDataLength * sizeof(float));
            VertexData = (float*)_vertexDataPointer.ToPointer();
            
            voffset = 0;
            ioffset = 0;
            stride = 0;
        }

        public void Dispose()
        {
            int id;
            GL.Finish();
            id = VertexArrayObject;
            GL.DeleteVertexArrays(1, ref id);
            
            id = VertexDataBufferObject;
            GL.DeleteBuffers(1, ref id);
            VertexDataBufferObject = -1;

            id = IndexDataBufferObject;
            GL.DeleteBuffers(1, ref id);
            IndexDataBufferObject = -1;
			
            Marshal.FreeHGlobal(_vertexDataPointer);
            Marshal.FreeHGlobal(_indexDataPointer);
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
            	*(VertexData + voffset++) = n <= i ? 0.0f : data[i];

        }

        public void AddVertices(int count, params float[] data)
        {
            CheckForOverflowVertexBuffer(count);
            int n = data.Length;
            for (int i = 0; i < count * stride; i++)
            {
            	*(VertexData + voffset++) = n <= i ? 0.0f : data[i];
            }
        }

        public void AddIndices(params int[] data)
        {
            CheckForOverflowIndexBuffer(data.Length);
            for (int i = 0; i < data.Length; i++)
            {
            	*(IndexData + ioffset++) = data[i];
            }
        }

        public void UpdateBuffer()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexDataBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(voffset * sizeof(float)), _vertexDataPointer, UsageMode);

            GL.BindBuffer(BufferTarget.ArrayBuffer, IndexDataBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(ioffset * sizeof(int)), _indexDataPointer, UsageMode);
        }

        public void UpdateVertexBuffer()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexDataBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(voffset * sizeof(float)), _vertexDataPointer, UsageMode);
        }

        public void UpdateIndexBuffer()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, IndexDataBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(ioffset * sizeof(int)), _indexDataPointer, UsageMode);
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
		
        /// <summary>
        /// This method displaces index offset pointer and return actual pointer to fill buffer from
        /// </summary>
        /// <param name="countToFill"></param>
        internal int* GetIndexPointerToFill(int countToFill)
        {
        	CheckForOverflowIndexBuffer(countToFill);
        	int* ptr = IndexData + ioffset;
        	ioffset += countToFill;
        	return ptr;
        }
        internal float* GetVertexPointerToFill(int countToFill)
        {
        	CheckForOverflowVertexBuffer(countToFill);
        	float* ptr = VertexData + voffset;
        	voffset += countToFill * Stride;
        	return ptr;
        }
        internal void CheckForOverflowVertexBuffer(int add = 1)
        {
            int sum = voffset + (stride * add);

            while (sum > vertexDataLength)
            {
            	vertexDataLength = vertexDataLength * 2;
            	_vertexDataPointer = Marshal.ReAllocHGlobal(_vertexDataPointer, (IntPtr)(vertexDataLength * sizeof(float)));
            	VertexData = (float*)_vertexDataPointer.ToPointer();
            }
        }

        internal void CheckForOverflowIndexBuffer(int add = 1)
        {
            int sum = ioffset + add;

            while (sum > indexDataLength)
            {
            	indexDataLength = indexDataLength * 2;
            	_indexDataPointer = Marshal.ReAllocHGlobal(_indexDataPointer, (IntPtr)(indexDataLength * sizeof(int)));
            	IndexData = (int*)_indexDataPointer.ToPointer();
            }
        }

        public string DebugInfo(int i)
        {
        	switch (i) 
        	{
        		case 0: return "Vbuffer: " + vertexDataLength;
        		case 1: return "Ibuffer: " + indexDataLength;
        		default: return ";";
        	}
        }

        public string DebugName
        {
            get { return "Vertex buffer"; }
        }
    }
}