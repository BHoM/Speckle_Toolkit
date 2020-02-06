/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BH.oM.Adapter;
using BH.oM.Base;
using BH.oM.Data.Requests;
using SpeckleCore;
using BH.oM.Speckle;
using System.ComponentModel;
using BH.Engine.Base;
using System.Collections.Concurrent;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        public override List<object> Push(IEnumerable<object> objects, string tag = "", PushType pushType = PushType.AdapterDefault, ActionConfig actionConfig = null)
        {
            // Clone objects for immutability in the UI
            List<object> objectsToPush = objects.Select(x => x.DeepClone()).ToList();

            // Initialize Speckle Layer if not existing
            if (SpeckleLayer == null)
            {
                SpeckleLayer = new Layer() { Name = "Default Layer", OrderIndex = 0, StartIndex = 0, Topology = "", Guid = "c8a58593-7080-450b-96b9-b0158844644b" };
                SpeckleLayer.ObjectCount = 0;
            }

            // Initialize the SpeckleStream
            SpeckleStream.Layers = new List<Layer>() { SpeckleLayer };
            SpeckleStream.Objects = new List<SpeckleObject>(); // stream is immutable

            // //- Read config
            SpecklePushConfig pushConfig = (actionConfig as SpecklePushConfig) ?? new SpecklePushConfig();

            // //- Use "Speckle" history: produces a new stream at every push that corresponds to the old version. Enabled by default.
            if (pushConfig.EnableHistory)
                SetupHistory();

            List<SpeckleObject> speckleObjects = new List<SpeckleObject>();
            ConcurrentBag<SpeckleObject> speckleObjectsBag = new ConcurrentBag<SpeckleObject>();
            ConcurrentStack<Tuple<IObject, SpeckleObject>> dict = new ConcurrentStack<Tuple<IObject, SpeckleObject>>();

            // Actual creation and add to the stream
            for (int i = 0; i < objectsToPush.Count(); i++)
            {
                SpeckleObject sObj = Create(objectsToPush[i] as dynamic, pushConfig); // Dynamic dispatch to most appropriate method
                dict.Push(new Tuple<IObject, SpeckleObject>(objectsToPush[i] as IObject, sObj));
                speckleObjects.Add(sObj);
            }

            List<Task> tasks = new List<Task>();

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sw.Start();

            if (true)
            {
                // Save BHoM data inside the speckleObjects.
                for (int i = 0; i < objectsToPush.Count(); i++)
                {
                    Task t = Task.Run(
                    () =>
                    {
                        SpeckleObject so = null;
                        Tuple<IObject, SpeckleObject> kv = null;
                        dict.TryPop(out kv);

                        so = kv.Item2;

                        if (typeof(IObject).IsAssignableFrom(kv.Item1.GetType()))
                        {
                            Engine.Speckle.Modify.SetBHoMData(ref so, kv.Item1);
                        }

                        speckleObjectsBag.Add(so);
                    });

                    tasks.Add(t);
                }

                Task.WaitAll(tasks.ToArray());

                // Add objects to the stream
                SpeckleLayer.ObjectCount += 1;
                speckleObjectsBag.ToList().ForEach(o => SpeckleStream.Objects.Add(o));
            }

            sw.Stop();
            TimeSpan asd1 = sw.Elapsed;


            sw.Reset();
            sw.Start();

            if (true)
            {
                    // Save BHoM data inside the speckleObjects.
                    Parallel.For(0, objectsToPush.Count(), (int i) =>
                    {
                    object obj = objectsToPush[i];
                    SpeckleObject sObj = speckleObjects[i];

                    if (typeof(IObject).IsAssignableFrom(objectsToPush[i].GetType()))
                    {
                        Engine.Speckle.Modify.SetBHoMData(ref sObj, objectsToPush[i] as IObject);
                    }

                    speckleObjects[i] = sObj;
                });
            }

            sw.Stop();
            TimeSpan asd2 = sw.Elapsed;


            sw.Reset();
            sw.Start();
            for (int i = 0; i < objectsToPush.Count() - 1; i++)
            {
                SpeckleObject sObj = speckleObjects[i];

                if (typeof(IObject).IsAssignableFrom(objectsToPush[i].GetType()))
                {
                    Engine.Speckle.Modify.SetBHoMData(ref sObj, objectsToPush[i] as IObject);
                }

                speckleObjects[i] = sObj;
            }
            sw.Stop();
            TimeSpan asd3 = sw.Elapsed;


            // Add objects to the stream
            SpeckleLayer.ObjectCount += 1;
            speckleObjects.ForEach(o => SpeckleStream.Objects.Add(o));

            // Send the stream
            var updateResponse = SpeckleClient.StreamUpdateAsync(SpeckleStreamId, SpeckleStream).Result;
            SpeckleClient.BroadcastMessage("stream", SpeckleStreamId, new { eventType = "update-global" });

            return objectsToPush;
        }
    }
}
