/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using BH.oM.Base;
using BH.oM.Data.Requests;
using SpeckleCore;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter : BHoMAdapter
    {
        public override List<IObject> Push(IEnumerable<IObject> objects, string tag = "", Dictionary<string, object> config = null)
        {
            //var gna = SpeckleClient.StreamsGetAllAsync("").Result;

            //gna.Resources[0].StreamId


            objects = objects.ToList(); // to avoid modifying the original objects
            List<IObject> IBHoMObjectsToPush = new List<IObject>(); //has to be IObject even if it's going to collect IBHoMObjects
            List<IObject> IObjectsToPush = new List<IObject>();

            // Initialize Speckle Layer if not existing
            if (SpeckleLayer == null)
            {
                SpeckleLayer = new Layer() { Name = "Default Layer", OrderIndex = 0, StartIndex = 0, Topology = "", Guid = "c8a58593-7080-450b-96b9-b0158844644b" };
                SpeckleLayer.ObjectCount = 0;

            }

            // Initialize the SpeckleStream
            SpeckleStream.Layers = new List<Layer>() { SpeckleLayer };
            SpeckleStream.Objects = new List<SpeckleObject>(); // --> shit's immutable, yo


            /// -------------------
            /// Base push rewritten
            /// -------------------

            // //- Read config
            object configObj;

            string pushType = "Replace";
            if (config != null && config.TryGetValue("PushType", out configObj))
                pushType = configObj.ToString();

            bool useGUIDS = false;
            if (config != null && config.TryGetValue("UseGUIDS", out configObj))
                if (configObj is bool)
                    useGUIDS = (bool)configObj;

            bool setAssignedId = true;
            if (config != null && config.TryGetValue("SetAssignedId", out configObj))
                if (configObj is bool)
                    setAssignedId = (bool)configObj;

            // //- Configure history. By default it's enabled, and it produces a new stream at every push that correspond to versions.
            configureHistory(config);

            // //- Clone objects
            List<IObject> objectsToPush = Config.CloneBeforePush ? objects.Select(x => x is BHoMObject ? ((BHoMObject)x).GetShallowClone() : x).ToList() : objects.ToList(); //ToList() necessary for the return collection to function properly for cloned objects

            bool success = true;
            List<IBHoMObject> bHoMObjects = new List<IBHoMObject>();
            List<IObject> iobjects = new List<IObject>();

            if (useGUIDS)
            {
                // This part works using IBHoMObjects GUID.
                Engine.Speckle.Query.DispatchBHoMObjects(objectsToPush, out bHoMObjects, out iobjects);

                List<IObject> objectsCreated = null;
                success = DiffingByBHoMGuid(objectsToPush, out objectsCreated);

                objectsToPush = objectsCreated.Count > 0 ? objectsCreated : objectsToPush;
            }
            else
            {
                // // - Base push rewritten to allow some additional CustomData to go in.
                BasePushRewritten(objectsToPush, pushType, tag, setAssignedId, ref success);
            }

            return success ? objectsToPush : new List<IObject>();
        }


        /***************************************************/
        /**** Private Helper Methods                    ****/
        /***************************************************/

        private void BasePushRewritten(List<IObject> objectsToPush, string pushType, string tag, bool setAssignedId, ref bool success)
        {
            // // - Base push rewritten to allow some additional CustomData to go in.
            Type iBHoMObjectType = typeof(IBHoMObject);
            MethodInfo miToList = typeof(Enumerable).GetMethod("Cast");
            foreach (var typeGroup in objectsToPush.GroupBy(x => x.GetType()))
            {
                MethodInfo miListObject = miToList.MakeGenericMethod(new[] { typeGroup.Key });

                var list = miListObject.Invoke(typeGroup, new object[] { typeGroup });

                if (iBHoMObjectType.IsAssignableFrom(typeGroup.Key))
                {
                    // They are IBHoMObjects

                    /// Assign SpeckleStreamId to the CustomData of the IBHoMObjects
                    var iBHoMObjects = list as IEnumerable<IBHoMObject>;
                    iBHoMObjects.ToList().ForEach(o => o.CustomData["Speckle_StreamId"] = SpeckleStreamId);

                    /// Switch push type
                    success &= CreateIBHoMObjects(iBHoMObjects as dynamic, setAssignedId);//Replace(iBHoMObjects as dynamic, tag);
                }
                else
                {
                    // They are IObjects
                    CreateIObjects(list as dynamic);
                }
            }
        }

        private void configureHistory(Dictionary<string, object> config = null)
        {
            ResponseStreamClone response = null;
            Task<ResponseStreamClone> respStreamClTask = null;

            object enableHistoryObj = null;

            if (config != null)
                config.TryGetValue("EnableHistory", out enableHistoryObj);

            bool? enableHistory = enableHistoryObj as bool?;
            if (enableHistory != null && !(bool)enableHistory)
                return;

            // The following line creates a new stream (with a different StreamId), where the current stream content is copied, before it gets modified.
            // The streamId of the cloned "backup" is saved among the main Stream "children" field,
            // accessible through SpeckleServerAddress/api/v1/streams/streamId
            respStreamClTask = SpeckleClient.StreamCloneAsync(SpeckleStreamId);

            try
            {
                response = respStreamClTask?.Result;
            }
            catch (Exception e) { }

            if (response == null)
                BH.Engine.Reflection.Compute.RecordWarning($"Could not set the EnableHistory option. Task status: {respStreamClTask.Status.ToString()}");
        }
    }
}
