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
using System.Threading.Tasks;
using Soss.Client;
using MathNet.Numerics.Statistics;

namespace ObjectReport
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                bool multithreadedRetrieval = false;
                if (args.Length > 0 && args[0].ToLower().StartsWith("-m"))
                    multithreadedRetrieval = true;

                // Get a collection of all keys in the ScaleOut service:
                Console.Write("Querying SOSS server... ");
                var queryResult = ApplicationNamespace.GlobalNamespace.Query(null);
                Console.WriteLine("done.");

                // Retrieve information about the ScaleOut object associated w/ each key:
                Console.Write("Retrieving object metadata... ");
                IEnumerable<ObjectInfo> infoColl = null;
                if (multithreadedRetrieval)
                    infoColl = RetrieveObjectInfoMultithreaded(queryResult);
                else
                    infoColl = RetrieveObjectInfo(queryResult);
                Console.WriteLine("done.");

                // Lookup dictionary to convert application IDs to friendly namespace names:
                var nsLookup = Interop.SossNamespaceInfo.GetNamespaceLookup();

                // Holds summaries of memory usage by namespace (used by report):
                var statsByNamespace = new List<NamespaceSummary>();

                // Group the object info by namespace and calculate stats:
                var infoGroupedByNamespace = infoColl.GroupBy(o => o.Key.AppId);
                foreach (var namespaceGroup in infoGroupedByNamespace)
                {
                    statsByNamespace.Add(new NamespaceSummary(namespaceGroup, nsLookup));
                }

                // Generate report and write to file.
                HtmlReportTemplate report = new HtmlReportTemplate(statsByNamespace);
                string reportOutput = report.TransformText();
                string reportFileName = $"{DateTime.Now:yyyyMMdd-HHmmss}.html";
                System.IO.File.WriteAllText(reportFileName, reportOutput);
                Console.WriteLine($"Wrote {reportFileName}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static IEnumerable<ObjectInfo> RetrieveObjectInfo(QueryResult keys)
        {
            List<ObjectInfo> ret = new List<ObjectInfo>(keys.Count);
            foreach (StateServerKey key in keys)
            {
                var objInfo = RetrieveInfo(key);
                if (objInfo != null)
                    ret.Add(objInfo);
            }
            return ret;
        }

        static IEnumerable<ObjectInfo> RetrieveObjectInfoMultithreaded(QueryResult keys)
        {
            List<ObjectInfo> ret = new List<ObjectInfo>(keys.Count);
            Parallel.ForEach(keys.Cast<StateServerKey>(), key =>
            {
                var objInfo = RetrieveInfo(key);
                if (objInfo != null)
                {
                    lock (ret)
                    {
                        ret.Add(objInfo);
                    }
                }

            });
            return ret;
        }

        static ObjectInfo RetrieveInfo(StateServerKey key)
        {
            // Tells the server that operations performed with this key shouldn't reset timeouts:
            var mgtKey = key;
            mgtKey.IsManagementKey = true;
            // ScaleOut 5.5 (and higher) fixes key structs to be immutable. Use this line instead:
            //var mgtKey = key.ToManagementKey();

            // Retrieve metadata info from the SOSS service:
            DataAccessor da = DataAccessor.CreateDataAccessor(mgtKey, lockWhenReading: false);
            ReadMetadataResult res = da.ReadMetadata(ReadOptions.ObjectMayBeLocked | ReadOptions.ObjectMayNotExist);

            if (res.Status == StateServerResult.ObjectNotFound)
            {
                // Object was removed/expired between the time of initial key retrieval and metadata retrieval.
                return null;
            }

            ExtendedObjectMetadata meta = res.Metadata;

            ObjectInfo objInfo = new ObjectInfo()
            {
                Key = key,
                SizeInBytes = meta.ObjectSize
            };

            // TODO: If we need to capture a custom property/field on the object itself, add a reference
            // to its assembly (so it can be deserialized) and then retrieve the object here. Once we have
            // an instance of it, any properties we're interested in can be added on as a new property of
            // the ObjectInfo we're returning.
            // For example:
            /*
            try
            {
                MyClass myObject = (MyClass)da.ReadObject(false);
                if (myObject != null)
                {
                    ret.InterestingProperty = myObject.InterestingProperty;
                }
            }
            catch
            {
            }
            */

            return objInfo;
        }
    }
}
