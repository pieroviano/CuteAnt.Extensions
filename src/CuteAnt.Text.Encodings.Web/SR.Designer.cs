﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace CuteAnt.Text.Encodings.Web {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///    强类型资源类，用于查找本地化字符串，等等。
    /// </summary>
    // 此类已由 StronglyTypedResourceBuilder 自动生成
    // 通过 ResGen 或 Visual Studio 之类的工具提供的类。
    // 若要添加或删除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (使用 /str 选项)，或重新生成 VS 项目。
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class SR {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        internal SR() {
        }
        
        /// <summary>
        ///    返回此类使用的缓存 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CuteAnt.Text.Encodings.Web.SR", typeof(SR).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///    重写所有项的当前线程的 CurrentUICulture 属性
        ///    使用此强类型资源类进行资源查找。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///    查找与 Object is not a array with the same initialization state as the array to compare it to. 类似的本地化字符串。
        /// </summary>
        internal static string ArrayInitializedStateNotEqual {
            get {
                return ResourceManager.GetString("ArrayInitializedStateNotEqual", resourceCulture);
            }
        }
        
        /// <summary>
        ///    查找与 Object is not a array with the same number of elements as the array to compare it to. 类似的本地化字符串。
        /// </summary>
        internal static string ArrayLengthsNotEqual {
            get {
                return ResourceManager.GetString("ArrayLengthsNotEqual", resourceCulture);
            }
        }
        
        /// <summary>
        ///    查找与 Cannot find the old value 类似的本地化字符串。
        /// </summary>
        internal static string CannotFindOldValue {
            get {
                return ResourceManager.GetString("CannotFindOldValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///    查找与 Capacity was less than the current Count of elements. 类似的本地化字符串。
        /// </summary>
        internal static string CapacityMustBeGreaterThanOrEqualToCount {
            get {
                return ResourceManager.GetString("CapacityMustBeGreaterThanOrEqualToCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///    查找与 MoveToImmutable can only be performed when Count equals Capacity. 类似的本地化字符串。
        /// </summary>
        internal static string CapacityMustEqualCountOnMove {
            get {
                return ResourceManager.GetString("CapacityMustEqualCountOnMove", resourceCulture);
            }
        }
        
        /// <summary>
        ///    查找与 Collection was modified; enumeration operation may not execute. 类似的本地化字符串。
        /// </summary>
        internal static string CollectionModifiedDuringEnumeration {
            get {
                return ResourceManager.GetString("CollectionModifiedDuringEnumeration", resourceCulture);
            }
        }
        
        /// <summary>
        ///    查找与 An element with the same key but a different value already exists. Key: {0} 类似的本地化字符串。
        /// </summary>
        internal static string DuplicateKey {
            get {
                return ResourceManager.GetString("DuplicateKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///    查找与 This operation does not apply to an empty instance. 类似的本地化字符串。
        /// </summary>
        internal static string InvalidEmptyOperation {
            get {
                return ResourceManager.GetString("InvalidEmptyOperation", resourceCulture);
            }
        }
        
        /// <summary>
        ///    查找与 This operation cannot be performed on a default instance of ImmutableArray&lt;T&gt;.  Consider initializing the array, or checking the ImmutableArray&lt;T&gt;.IsDefault property. 类似的本地化字符串。
        /// </summary>
        internal static string InvalidOperationOnDefaultArray {
            get {
                return ResourceManager.GetString("InvalidOperationOnDefaultArray", resourceCulture);
            }
        }
    }
}
