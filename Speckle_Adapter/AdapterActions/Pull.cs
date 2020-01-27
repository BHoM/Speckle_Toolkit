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
using BH.oM.Base;
using BH.oM.Data.Requests;
using SpeckleCore;
using BH.Engine.Speckle;
using BH.oM.Adapter;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        public override IEnumerable<object> Pull(IRequest request, PullType pullType = PullType.AdapterDefault, ActionConfig actionConfig = null)
        {
            if (request == null)
                return ReadAll();

            // Make sure this is a FilterRequest
            FilterRequest filterRequest = request as FilterRequest;
            if (filterRequest == null)
            {
                Engine.Reflection.Compute.RecordWarning("Please specify a FilterRequest");
                return new List<object>();
            }

            List<IBHoMObject> bHoMObjects = new List<IBHoMObject>();
            List<IObject> iObjects = new List<IObject>();
            List<object> reminder = new List<object>();

            Read(filterRequest, out bHoMObjects, out iObjects, out reminder);

            // Return stuff
            if (typeof(IBHoMObject).IsAssignableFrom(filterRequest.Type))
                return bHoMObjects;
            else if (typeof(IObject).IsAssignableFrom(filterRequest.Type))
                return iObjects;
            else
                return bHoMObjects.Concat(iObjects).Concat(reminder);
        }
    }
}
