﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LightVPN.Client.Auth.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class StringTable {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal StringTable() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LightVPN.Client.Auth.Resources.StringTable", typeof(StringTable).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
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
        ///   Looks up a localized string similar to Bad request, you may need to manually update your LightVPN client.
        /// </summary>
        internal static string API_BAD_REQUEST {
            get {
                return ResourceManager.GetString("API_BAD_REQUEST", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to We&apos;re unable to communicate with your client, you may need to manually update your LightVPN client..
        /// </summary>
        internal static string API_CLIENT_ISSUE {
            get {
                return ResourceManager.GetString("API_CLIENT_ISSUE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Forbidden, you may have been blocked by the API firewall. Please message support.
        /// </summary>
        internal static string API_FORBID {
            get {
                return ResourceManager.GetString("API_FORBID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Status code is out of range for friendly messages; status code: {0}.
        /// </summary>
        internal static string API_OUT_OF_RANGE_CODE {
            get {
                return ResourceManager.GetString("API_OUT_OF_RANGE_CODE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Slow down there! You&apos;ve been ratelimited. Try again in a minute..
        /// </summary>
        internal static string API_RATELIMITED {
            get {
                return ResourceManager.GetString("API_RATELIMITED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to We were unable to fufill your request due to a problem in the server. Please message support about this issue..
        /// </summary>
        internal static string API_SERVER_ERROR {
            get {
                return ResourceManager.GetString("API_SERVER_ERROR", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unauthorized, your session may be invalid..
        /// </summary>
        internal static string API_UNAUTH {
            get {
                return ResourceManager.GetString("API_UNAUTH", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to We were unable to contact the server to fufill your request. Please check back later and if this persists, message support..
        /// </summary>
        internal static string API_UNAVAILABLE {
            get {
                return ResourceManager.GetString("API_UNAVAILABLE", resourceCulture);
            }
        }
    }
}