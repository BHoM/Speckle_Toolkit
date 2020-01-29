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

using BH.oM.Base;
using BH.oM.Data;
using BH.oM.Data.Collections;
using BH.oM.Geometry;
using SpeckleCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BH.Engine.Rhinoceros;
using BH.Engine.Speckle;
using BH.oM.Speckle;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        protected bool CreateIObjects(List<IObject> objects, SpeckleActionConfig config)
        {
            List<SpeckleObject> allObjects = new List<SpeckleObject>();

            // If they are IGeometry, convert them to their Rhino representation that Speckle understands.
            foreach (var obj in objects)
            {
                if (typeof(IGeometry).IsAssignableFrom(obj.GetType()))
                    allObjects.Add(BH.Engine.Speckle.Convert.IFromBHoM((IGeometry)obj));
                else
                    allObjects.Add((SpeckleObject)SpeckleCore.Converter.Serialise(obj)); // These will be exported as `Abstract` Speckle Objects.
            }

            // Add the speckleObjects to the Stream
            SpeckleLayer.ObjectCount += allObjects.Count();
            SpeckleStream.Objects.AddRange(allObjects);

            // Update the stream
            var updateResponse = SpeckleClient.StreamUpdateAsync(SpeckleStreamId, SpeckleStream).Result;
            SpeckleClient.BroadcastMessage("stream", SpeckleStreamId, new { eventType = "update-global" });

            /// Read the objects as exported in speckle
            /// so we can assign the Speckle-generated id into the BHoMobjects
            if (config.SetAssignedId)
            {
                ResponseObject response = SpeckleClient.StreamGetObjectsAsync(SpeckleStreamId, "").Result;

                List<IBHoMObject> bHoMObjects_inSpeckle = new List<IBHoMObject>();
                IEnumerable<IBHoMObject> iBhomObjsInSpeckle = BH.Engine.Speckle.Convert.ToBHoM(response.Resources, true);

                VennDiagram<IBHoMObject> correspondenceDiagram = Engine.Data.Create.VennDiagram(objects.Where(o => o as IBHoMObject != null).Cast<IBHoMObject>(), iBhomObjsInSpeckle, new IBHoMGUIDComparer());

                if (correspondenceDiagram.Intersection.Count != objects.Count())
                {
                    //Engine.Reflection.Compute.RecordError("Push failed.\nNumber of objects created in Speckle do not correspond to the number of objects pushed.");
                    //return false;
                }

                correspondenceDiagram.Intersection.ForEach(o => o.Item1.CustomData[AdapterIdName] = o.Item2.CustomData[AdapterIdName]);
            }

            return true;
        }
    }
}
