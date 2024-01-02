/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using BHG = BH.oM.Geometry;
using SpeckleCoreGeometryClasses;
using BH.oM.Base;
using System.Reflection;
using BH.oM.Geometry;
using BH.Engine.Base;
using System.ComponentModel;
using BH.oM.Speckle;
using BH.oM.Graphics;
using BH.Engine.Representation;
using System.Collections.Specialized;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        public void BatchUpdateStream(SpecklePushConfig pushConfig)
        {
            List<SpeckleObject> convertedObjects = SpeckleClient.Stream.Objects;

            SpeckleCore.SpeckleInitializer.Initialize();
            SpeckleCore.LocalContext.Init();

            LocalContext.PruneExistingObjects(convertedObjects, SpeckleClient.BaseUrl);
            List<SpeckleObject> persistedObjects = new List<SpeckleObject>();

            OrderedDictionary JobQueue = new OrderedDictionary();

            if (convertedObjects.Count(obj => obj.Type == "Placeholder") != convertedObjects.Count)
            {
                // create the update payloads
                int count = 0;
                var objectUpdatePayloads = new List<List<SpeckleObject>>();
                long totalBucketSize = 0;
                long currentBucketSize = 0;
                var currentBucketObjects = new List<SpeckleObject>();
                var allObjects = new List<SpeckleObject>();
                foreach (SpeckleObject convertedObject in convertedObjects)
                {
                    if (count++ % 100 == 0)
                    {
                        //Message = "Converted " + count + " objects out of " + convertedObjects.Count() + ".";
                    }

                    // size checking & bulk object creation payloads creation
                    long size = Converter.getBytes(convertedObject).Length;
                    currentBucketSize += size;
                    totalBucketSize += size;
                    currentBucketObjects.Add(convertedObject);

                    // Object is too big?
                    if (size > 2e6)
                    {
                        BH.Engine.Reflection.Compute.RecordWarning("An object is too big for the current Speckle limitations.");
                        currentBucketObjects.Remove(convertedObject);
                    }

                    if (currentBucketSize > 3e5) // restrict max to ~300kb; 
                    {
                        //BH.Engine.Reflection.Compute.RecordNote("Reached payload limit. Making a new one, current  #: " + objectUpdatePayloads.Count);

                        objectUpdatePayloads.Add(currentBucketObjects);
                        currentBucketObjects = new List<SpeckleObject>();
                        currentBucketSize = 0;
                    }
                }

                // add in the last bucket
                if (currentBucketObjects.Count > 0)
                    objectUpdatePayloads.Add(currentBucketObjects);

                if (objectUpdatePayloads.Count > 1)
                    BH.Engine.Reflection.Compute.RecordNote($"Payload has been split in { objectUpdatePayloads.Count } batches. Total size is {totalBucketSize / 1024} kB.");

                // create bulk object creation tasks
                List<ResponseObject> responses = new List<ResponseObject>();

                foreach (var payload in objectUpdatePayloads)
                {
                    //Message = String.Format("{0}/{1}", k++, objectUpdatePayloads.Count);
                    try
                    {
                        var objResponse = SpeckleClient.ObjectCreateAsync(payload).Result;
                        responses.Add(objResponse);
                        persistedObjects.AddRange(objResponse.Resources);

                        int m = 0;
                        foreach (var oL in payload)
                            oL._id = objResponse.Resources[m++]._id;

                        // push sent objects in the cache non-blocking
                        Task.Run(() =>
                        {
                            foreach (var oL in payload)
                                if (oL.Type != "Placeholder")
                                    LocalContext.AddSentObject(oL, SpeckleClient.BaseUrl);
                        });
                    }
                    catch (Exception err)
                    {
                        BH.Engine.Reflection.Compute.RecordWarning(err.Message);
                        continue;
                    }
                }
            }
            else
            {
                persistedObjects = convertedObjects;
            }

            // create placeholders for stream update payload
            List<SpeckleObject> placeholders = new List<SpeckleObject>();

            //foreach ( var myResponse in responses )
            foreach (var obj in persistedObjects)
                placeholders.Add(new SpecklePlaceholder() { _id = obj._id });

            SpeckleClient.Stream.Objects = placeholders;

            // set some base properties (will be overwritten)
            var baseProps = new Dictionary<string, object>();
            baseProps["units"] = "m";
            baseProps["tolerance"] = "0.001";
            baseProps["angleTolerance"] = "0.01";
            SpeckleClient.Stream.BaseProperties = baseProps;

            var response = SpeckleClient.StreamUpdateAsync(SpeckleClient.StreamId, SpeckleClient.Stream).Result;
            SpeckleClient.BroadcastMessage("stream", SpeckleClient.StreamId, new { eventType = "update-global" });
        }
    }
}



