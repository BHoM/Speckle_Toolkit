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
        protected bool CreateObjects(IEnumerable<object> objects, SpecklePushConfig config)
        {
            // Convert the objects into "Abstract" SpeckleObjects 
            List<SpeckleObject> objs_serialized = SpeckleCore.Converter.Serialise(objects);

            // Add objects to the stream
            SpeckleLayer.ObjectCount += objects.Count();
            SpeckleStream.Objects.AddRange(objs_serialized);

            // Send the objects
            var updateResponse = SpeckleClient.StreamUpdateAsync(SpeckleStreamId, SpeckleStream).Result;
            SpeckleClient.BroadcastMessage("stream", SpeckleStreamId, new { eventType = "update-global" });

            return true;
        }
    }
}
