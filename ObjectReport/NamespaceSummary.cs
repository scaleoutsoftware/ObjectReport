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
using MathNet.Numerics.Statistics;

namespace ObjectReport
{
    /// <summary>
    /// Generates memory usage statistics for a ScaleOut namespace, available in the Stats property.
    /// </summary>
    public class NamespaceSummary
    {
        public string NamespaceName { get; private set; }
        public IList<Statistic> Stats { get; } = new List<Statistic>();

        public NamespaceSummary(IGrouping<uint, ObjectInfo> nsGroup, IDictionary<uint, string> nsLookup)
        {
            try
            {
                NamespaceName = nsLookup[nsGroup.Key];
            }
            catch (KeyNotFoundException)
            {
                NamespaceName = nsGroup.Key.ToString();
            }
            GenerateStatistics(nsGroup);
        }

        private void GenerateStatistics(IGrouping<uint, ObjectInfo> nsGroup)
        {
            Stats.Add(new Statistic()
            {
                StatName = "Object count",
                StatValue = nsGroup.Count().ToString("N0")
            });

            Stats.Add(new Statistic()
            {
                StatName = "Total object data",
                StatValue = nsGroup.Sum(oi => (long)oi.SizeInBytes).ToString("N0") + " bytes"
            });
            
            Tuple<double, double> meanStdDev = Statistics.MeanStandardDeviation(nsGroup.Select(oi => (double)oi.SizeInBytes));

            Stats.Add(new Statistic()
            {
                StatName = "Average object size",
                StatValue = meanStdDev.Item1.ToString("N2") + " bytes"
            });

            Stats.Add(new Statistic()
            {
                StatName = "StdDev",
                StatValue = meanStdDev.Item2.ToString("N2")
            });

            Stats.Add(new Statistic()
            {
                StatName = "Median",
                StatValue = Statistics.Median(nsGroup.Select(oi => (double)oi.SizeInBytes)).ToString("N0") + " bytes"
            });

            Stats.Add(new Statistic()
            {
                StatName = "Smallest object",
                StatValue = nsGroup.Min(oi => oi.SizeInBytes).ToString("N0") + " bytes"
            });

            Stats.Add(new Statistic()
            {
                StatName = "Largest object",
                StatValue = nsGroup.Max(oi => oi.SizeInBytes).ToString("N0") + " bytes"
            });
        }

        
    }

    public class Statistic
    {
        public string StatName { get; set; }
        public string StatValue { get; set; }
    }
}
