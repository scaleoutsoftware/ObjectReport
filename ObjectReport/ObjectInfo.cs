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
using Soss.Client;

namespace ObjectReport
{
    /// <summary>
    /// Contains metadata related to an object stored in the StateServer service.
    /// </summary>
    public class ObjectInfo
    {
        public StateServerKey Key { get; set; }
        public int SizeInBytes { get; set; }
    }
}
