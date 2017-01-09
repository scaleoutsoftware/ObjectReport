/* Copyright 2016 ScaleOut Software, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Soss.Client;

namespace ObjectReport.Interop
{
    /// <summary>
    /// C# version of the unmanaged SOSSLIB_APPNAME_DESCR structure from soss_svccli.h,
    /// suitable for p/invoke into native soss_svccli.dll C API.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SOSSLIB_APPNAME_DESCR
    {
        /// <summary>
        /// Application name
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 281)] // (Note that 281 is SOSSLIB_DEF_MAX_APPNM_LEN+1)
        public string app_name;

        /// <summary>
        /// Application name length
        /// </summary>
        public UInt32 app_name_len;

        /// <summary>
        /// Application identifier
        /// </summary>
        public UInt32 app_id;
    }

    internal static class NativeMethods
    {
        [DllImport("soss_svccli.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern UInt32 Sosslib_open();

        [DllImport("soss_svccli.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Int32 Sosslib_close(UInt32 hdl);

        [DllImport("soss_svccli.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Int32 Sosslib_read_appname_list(UInt32 hdl, out UInt32 pnum_names);

        [DllImport("soss_svccli.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Int32 Sosslib_get_appname_data([In, Out]SOSSLIB_APPNAME_DESCR[] plist, UInt32 num_names);
    }

    /// <summary>
    /// Helper class for retrieving list of app names in the SOSS service.
    /// </summary>
    static class SossNamespaceInfo
    {
        private const UInt32 SOSSLIB_NULL_HANDLE = 0xFFFFFFFF;
        private const UInt32 SOSSLIB_MAX_APP_ID = 0xFFFFFFF;

        /// <summary>
        /// Returns a lookup dictionary of namespaces, where an internal application ID
        /// (a uint) maps to a friendly namespace name.
        /// </summary>
        /// <returns>IDictionary of appIDs mapped to namespace names.</returns>
        public static IDictionary<UInt32, string> GetNamespaceLookup()
        {
            var descriptors = GetNamespaceDescriptors();
            var lookup = new Dictionary<UInt32, string>(descriptors.Length);
            foreach (var descriptor in descriptors)
            {
                if (descriptor.app_id > SOSSLIB_MAX_APP_ID)
                    continue; // ignoring system namespaces.

                lookup.Add(descriptor.app_id, descriptor.app_name);
            }

            return lookup;
        }

        /// <summary>
        /// Retrieves native descriptors using natvie soss_svccli.dll calls.
        /// </summary>
        /// <returns>Array of SOSSLIB_APPNAME_DESCR structures.</returns>
        private static SOSSLIB_APPNAME_DESCR[] GetNamespaceDescriptors()
        {
            UInt32 handle = SOSSLIB_NULL_HANDLE;
            try
            {
                handle = NativeMethods.Sosslib_open();
                if (handle == SOSSLIB_NULL_HANDLE) throw new LocalServiceUnavailableException();

                UInt32 nsCount;
                Int32 readRes = NativeMethods.Sosslib_read_appname_list(handle, out nsCount);

                if (readRes != 1)
                    throw new StateServerException("read", readRes, "[app names]");

                SOSSLIB_APPNAME_DESCR[] descriptors = new SOSSLIB_APPNAME_DESCR[nsCount];
                Int32 dataRet = NativeMethods.Sosslib_get_appname_data(descriptors, nsCount);

                if (dataRet != 1)
                    throw new StateServerException("read data", dataRet, "[app name data]");

                return descriptors;
            }
            finally
            {
                if (handle != SOSSLIB_NULL_HANDLE)
                    NativeMethods.Sosslib_close(handle);
            }
        }
    }

}
