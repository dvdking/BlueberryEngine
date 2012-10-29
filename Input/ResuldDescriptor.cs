using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Blueberry.XInput
{
    /// <summary>
    /// Descriptor used to provide detailed message for a particular Result.
    /// 
    /// </summary>
    [Serializable]
    internal sealed class ResultDescriptor
    {
        private static readonly object LockDescriptor = new object();
        private static readonly List<Type> RegisteredDescriptorProvider = new List<Type>();
        private static readonly Dictionary<Result, ResultDescriptor> Descriptors = new Dictionary<Result, ResultDescriptor>();
        private const string UnknownText = "Unknown";

        /// <summary>
        /// Gets the result.
        /// 
        /// </summary>
        public Result Result { get; private set; }

        /// <summary>
        /// Gets the module
        /// 
        /// </summary>
        public string Module { get; private set; }

        /// <summary>
        /// Gets the native API code 
        /// 
        /// </summary>
        public string NativeApiCode { get; private set; }

        /// <summary>
        /// Gets the API code 
        /// 
        /// </summary>
        public string ApiCode { get; private set; }

        /// <summary>
        /// Gets the description of the result code if any.
        /// 
        /// </summary>
        public string Description { get; set; }

        static ResultDescriptor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Blueberry.ResultDescriptor"/> class.
        /// 
        /// </summary>
        /// <param name="code">The HRESULT error code.</param><param name="module">The module (ex: SharpDX.Direct2D1).</param><param name="apiCode">The API code (ex: D2D1_ERR_...).</param><param name="description">The description of the result code if any.</param>
        public ResultDescriptor(Result code, string module, string nativeApiCode, string apiCode, string description = null)
        {
            this.Result = code;
            this.Module = module;
            this.NativeApiCode = nativeApiCode;
            this.ApiCode = apiCode;
            this.Description = description;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="T:Blueberry.ResultDescriptor"/> to <see cref="T:SharpDX.Result"/>.
        /// 
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>
        /// The result of the conversion.
        /// 
        /// </returns>
        public static implicit operator Result(ResultDescriptor result)
        {
            return result.Result;
        }

        /// <summary>
        /// Implements the operator ==.
        /// 
        /// </summary>
        /// <param name="left">The left.</param><param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(ResultDescriptor left, Result right)
        {
            if (left == null)
                return false;
            else
                return left.Result.Code == right.Code;
        }

        /// <summary>
        /// Implements the operator !=.
        /// 
        /// </summary>
        /// <param name="left">The left.</param><param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(ResultDescriptor left, Result right)
        {
            if (left == null)
                return false;
            else
                return left.Result.Code != right.Code;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:Blueberry.ResultDescriptor"/> is equal to this instance.
        /// 
        /// </summary>
        /// <param name="other">The <see cref="T:Blueberry.ResultDescriptor"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="T:Blueberry.ResultDescriptor"/> is equal to this instance; otherwise, <c>false</c>.
        /// 
        /// </returns>
        public bool Equals(ResultDescriptor other)
        {
            if (object.ReferenceEquals((object)null, (object)other))
                return false;
            if (object.ReferenceEquals((object)this, (object)other))
                return true;
            else
                return other.Result.Equals(this.Result);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to this instance.
        /// 
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="T:System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// 
        /// </returns>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals((object)null, obj))
                return false;
            if (object.ReferenceEquals((object)this, obj))
                return true;
            if (obj.GetType() != typeof(ResultDescriptor))
                return false;
            else
                return this.Equals((ResultDescriptor)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return this.Result.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("HRESULT: [0x{0:X}], Module: [{1}], ApiCode: [{2}/{3}], Message: {4}", (object)this.Result.Code, (object)this.Module, (object)this.NativeApiCode, (object)this.ApiCode, (object)this.Description);
        }

        /// <summary>
        /// Registers a <see cref="T:Blueberry.ResultDescriptor"/> provider.
        /// 
        /// </summary>
        /// <param name="descriptorsProviderType">Type of the descriptors provider.</param>
        /// <remarks>
        /// Providers are usually registered at module init when SharpDX assemblies are loaded.
        /// 
        /// </remarks>
        public static void RegisterProvider(Type descriptorsProviderType)
        {
            lock (ResultDescriptor.LockDescriptor)
            {
                if (ResultDescriptor.RegisteredDescriptorProvider.Contains(descriptorsProviderType))
                    return;
                ResultDescriptor.RegisteredDescriptorProvider.Add(descriptorsProviderType);
            }
        }

        /// <summary>
        /// Finds the specified result descriptor.
        /// 
        /// </summary>
        /// <param name="result">The result code.</param>
        /// <returns>
        /// A descriptor for the specified result
        /// </returns>
        public static ResultDescriptor Find(Result result)
        {
            ResultDescriptor resultDescriptor;
            lock (ResultDescriptor.LockDescriptor)
            {
                if (ResultDescriptor.RegisteredDescriptorProvider.Count > 0)
                {
                    foreach (Type item_0 in ResultDescriptor.RegisteredDescriptorProvider)
                        ResultDescriptor.AddDescriptorsFromType(item_0);
                    ResultDescriptor.RegisteredDescriptorProvider.Clear();
                }
                if (!ResultDescriptor.Descriptors.TryGetValue(result, out resultDescriptor))
                    resultDescriptor = new ResultDescriptor(result, "Unknown", "Unknown", "Unknown", (string)null);
                if (resultDescriptor.Description == null)
                {
                    string local_2 = ResultDescriptor.GetDescriptionFromResultCode(result.Code);
                    resultDescriptor.Description = local_2 ?? "Unknown";
                }
            }
            return resultDescriptor;
        }

        private static void AddDescriptorsFromType(Type type)
        {
            foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (fieldInfo.FieldType == typeof(ResultDescriptor))
                {
                    ResultDescriptor resultDescriptor = (ResultDescriptor)fieldInfo.GetValue((object)null);
                    if (!ResultDescriptor.Descriptors.ContainsKey(resultDescriptor.Result))
                        ResultDescriptor.Descriptors.Add(resultDescriptor.Result, resultDescriptor);
                }
            }
        }

        private static string GetDescriptionFromResultCode(int resultCode)
        {
            IntPtr lpBuffer = IntPtr.Zero;
            int num = (int)ResultDescriptor.FormatMessageW(4864, IntPtr.Zero, resultCode, 0, ref lpBuffer, 0, IntPtr.Zero);
            string str = Marshal.PtrToStringUni(lpBuffer);
            Marshal.FreeHGlobal(lpBuffer);
            return str;
        }

        [DllImport("kernel32.dll")]
        private static extern uint FormatMessageW(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, ref IntPtr lpBuffer, int nSize, IntPtr Arguments);
    }
}
