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
using BH.oM.Diffing;
using BH.Engine.Diffing;


namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        public override List<object> Push(IEnumerable<object> objects, string tag = "", PushType pushType = PushType.AdapterDefault, ActionConfig actionConfig = null)
        {
            // //- Read config
            SpecklePushConfig pushConfig = (actionConfig as SpecklePushConfig) ?? new SpecklePushConfig();

            // Clone objects for immutability in the UI
            List<object> objectsToPush = objects.Select(x => x.DeepClone()).ToList();

            // AECDeltas variables
            Revision revision = null;
            Delta delta = null;

            if (pushConfig.EnableAECDeltas)
            {
                // Check if input objects actually contain only one object of a "special" type (Revision, Delta)
                if (objectsToPush.Count == 1)
                {
                    // If input objects contain only a single Revision object, it will be pushed as a revision-based Delta payload
                    revision = objectsToPush.FirstOrDefault() as Revision;
                    if (revision != null)
                        objectsToPush = revision.Objects.Cast<object>().ToList();

                    // If input objects contain only a single Delta object, it will be pushed as a diff-based Delta payload
                    // (Not yet supported)
                    delta = objectsToPush.FirstOrDefault() as Delta;
                    if (delta != null)
                        BH.Engine.Reflection.Compute.RecordError($"{this.GetType().Name} does not support upload of diff-based Deltas yet.");
                }
            }


            // Initialize Speckle Layer if not existing
            if (SpeckleLayer == null)
                SpeckleLayer = new Layer() { Name = "Default Layer", OrderIndex = 0, StartIndex = 0, Topology = "", Guid = "c8a58593-7080-450b-96b9-b0158844644b" };

            SpeckleLayer.ObjectCount = 0;

            // Initialize the SpeckleStream
            SpeckleStream.Layers = new List<Layer>() { SpeckleLayer };
            SpeckleStream.Objects = new List<SpeckleObject>(); // stream is immutable

            // //- Use "Speckle" history: produces a new stream at every push that corresponds to the old version. Enabled by default.
            if (pushConfig.EnableHistory)
                SetupHistory();

            // Actual creation
            List<SpeckleObject> allSpeckleObjects = new List<SpeckleObject>();
            for (int i = 0; i < objectsToPush.Count(); i++)
            {
                SpeckleObject speckleObject = Create(objectsToPush[i] as dynamic, pushConfig); // Dynamic dispatch to most appropriate method

                allSpeckleObjects.Add(speckleObject);
            }

            // Add the objects to the correct Payload type
            SpeckleDelta speckledeltaPayload = null;
            if (!pushConfig.EnableAECDeltas)
            {
                // Add objects to the stream
                SpeckleLayer.ObjectCount += allSpeckleObjects.Count;
                SpeckleStream.Objects.AddRange(allSpeckleObjects);
            }
            else
            {
                // Add the objects to a revision-based Delta
                //revision = new Revision(allSpeckleObjects, SpeckleStreamId);
                //delta = BH.Engine.Diffing.Create.RevisionBasedDelta(revision, pushConfig.DiffConfig, pushConfig.Comment);
                //SpeckleStream.Objects.Add(delta);
            }

            // Send the payload
            try
            {
                ResponseBase response = null;

                if (!pushConfig.EnableAECDeltas)
                {
                    // Issue: with `StreamUpdateAsync` Speckle doesn't seem to send anything if the Stream is initially empty.
                    // You need to Push twice if the Stream is empty.
                    response = SpeckleClient.StreamUpdateAsync(SpeckleStreamId, SpeckleStream).Result;
                }
                else
                {
                    //response = SpeckleClient.StreamApplyDeltaAsync(SpeckleStreamId, BH.Engine.Speckle.Convert.Delta(delta)).Result;
                    SpeckleDelta speckledelta = new SpeckleDelta();

                    SpeckleDelta speckleDelta = new SpeckleDelta() { revision_A = SpeckleStreamId };
                    speckleDelta.Created = allSpeckleObjects;                    

                    response = SpeckleClient.StreamApplyDeltaAsync(SpeckleStreamId, speckleDelta).Result;
                }

                // In all cases, we broadcast the same event (update of a stream)
                SpeckleClient.BroadcastMessage("stream", SpeckleStreamId, new { eventType = "update-global" });
            }
            catch (Exception e)
            {
                BH.Engine.Reflection.Compute.RecordError($"Upload to Speckle failed. Message returned:\n{e.InnerException}");
                BH.Engine.Reflection.Compute.RecordError($"The server might be busy, or a heavy model upload might have triggered the Timeout.\n" +
                    $"Try sending again. Try pushing the objects with a SpecklePushConfig having DisplayOptions set to false for some of the objects, e.g. Bars.");
                return new List<object>();
            }

            return objectsToPush;
        }
    }
}
