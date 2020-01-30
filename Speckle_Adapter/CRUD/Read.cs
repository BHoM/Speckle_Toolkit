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
using BH.oM.Base;
using BH.oM.Data.Requests;
using SpeckleCore;
using BH.Engine.Speckle;
using BH.oM.Adapter;
using BH.oM.Speckle;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        protected IEnumerable<object> ReadAll(bool assignSpeckleIdToBHoMObjects = true)
        {
            ResponseObject response = SpeckleClient.StreamGetObjectsAsync(SpeckleStreamId, "").Result;

            List<IBHoMObject> bHoMObjects = new List<IBHoMObject>();
            List<IObject> iObjects = new List<IObject>();
            List<object> reminder = new List<object>();

            if (!BH.Engine.Speckle.Convert.ToBHoM(response.Resources, out bHoMObjects, out iObjects, out reminder, assignSpeckleIdToBHoMObjects))
                BH.Engine.Reflection.Compute.RecordError("Failed to deserialize and cast the Server response into BHoM objects.");

            return bHoMObjects.Concat(iObjects).Concat(reminder);
        }

        protected bool Read(SpeckleRequest speckleRequest, out List<IBHoMObject> bHoMObjects, out List<IObject> iObjects, out List<object> reminder)
        {
            ResponseObject response = null;

            if (!string.IsNullOrEmpty(speckleRequest.SpeckleQuery))
                response = SpeckleClient.StreamGetObjectsAsync(SpeckleStreamId, speckleRequest.SpeckleQuery).Result;

            response = SpeckleClient.StreamGetObjectsAsync(SpeckleStreamId, "").Result;

            // Extract the speckleIds for selection from the SpeckleRequest.
            List<string> speckleIds = speckleRequest.SpeckleGUIDs;
            speckleIds = speckleIds?.Count != 0 ? speckleIds : null;

            // Convert the response to the appropriate object types.
            BH.Engine.Speckle.Convert.ToBHoM(response.Resources, out bHoMObjects, out iObjects, out reminder, speckleIds?.Count != 0, speckleIds);

            // Filter by tag if any 
            bHoMObjects = speckleRequest.Tag == "" ? bHoMObjects : bHoMObjects.Where(x => x.Tags.Contains(speckleRequest.Tag)).ToList();

            return true;
        }
    }
}
