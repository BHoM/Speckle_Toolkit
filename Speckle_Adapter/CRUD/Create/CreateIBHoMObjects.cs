﻿/*
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
        protected bool CreateIBHoMObjects(IEnumerable<IBHoMObject> BHoMObjects, SpecklePushConfig config)
        {
            // Convert the objects into the appropriate SpeckleObject (Point, Line, etc.) using the available converters.
            List<SpeckleObject> speckleObjects = BHoMObjects.Select(bhomObj => bhomObj.IFromBHoM()).ToList();

            if (speckleObjects.Where(obj => obj == null).Count() == speckleObjects.Count())
                return false;

            // Add objects to the stream
            SpeckleLayer.ObjectCount += BHoMObjects.Count();
            SpeckleStream.Objects.AddRange(speckleObjects);

            // Assign any other property to the speckle objects before updating the stream
            var objList = BHoMObjects.ToList();

            for (int i = 0; i < SpeckleStream.Objects.Count; i++)
            {
                if (objList.Count() <= i)
                    break;

                // Set `speckleObject.Name` as the BHoMObject type name.
                SpeckleStream.Objects[i].Name = string.IsNullOrEmpty(objList[i].Name) ? objList[i].GetType().ToString() : objList[i].Name;

                // Set the speckleObject type as the BHoMObject type name (not working)
                // SpeckleStream.Objects[i].Type = string.IsNullOrEmpty(objList[i].Name) ? objList[i].GetType().ToString() : objList[i].Name; 
            }

            // Send the objects
            var updateResponse = SpeckleClient.StreamUpdateAsync(SpeckleStreamId, SpeckleStream).Result;
            SpeckleClient.BroadcastMessage("stream", SpeckleStreamId, new { eventType = "update-global" });



            return true;
        }

    }
}
