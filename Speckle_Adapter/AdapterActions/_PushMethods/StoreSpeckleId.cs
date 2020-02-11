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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Data.Collections;
using BH.oM.Data.Requests;
using BH.oM.Geometry;
using BH.oM.Speckle;
using SpeckleCore;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        // NOTE: This used to work; does not work anymore since I switched to BH.Engine deserialisation. Initially I was using Speckle Deserialise. 
        // The issue is that our deserialisation "recreates" the objects without preserving the original GUID.
        // Disabled for now.

        //[Description("Downloads the IBHoMobjects as exported in Speckle, gets their SpeckleID and stores it the BHoMObject CustomData.")]
        //private void StoreSpeckleId(List<IBHoMObject> BHoMObjects)
        //{
        //    ResponseObject response = SpeckleClient.StreamGetObjectsAsync(SpeckleStreamId, "").Result;
        //    IEnumerable<IBHoMObject> objectsInSpeckle = BH.Engine.Speckle.Convert.ToBHoM(response.Resources, true).OfType<IBHoMObject>().ToList();

        //    VennDiagram<IBHoMObject> correspondenceDiagram = Engine.Data.Create.VennDiagram(BHoMObjects, objectsInSpeckle, new IBHoMGUIDComparer());
        //    correspondenceDiagram.Intersection.ForEach(o => o.Item1.CustomData[AdapterIdName] = o.Item2.CustomData[AdapterIdName]);
        //}
    }
}
