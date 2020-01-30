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
using BH.oM.Speckle;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        public override IEnumerable<object> Pull(IRequest request, PullType pullType = PullType.AdapterDefault, ActionConfig actionConfig = null)
        {
            // If the request exists, make sure it's a SpeckleRequest
            SpeckleRequest speckleRequest = request as SpeckleRequest;
            if (request != null && request.GetType() != typeof(FilterRequest) && speckleRequest == null)
            {
                Engine.Reflection.Compute.RecordError($"SpeckleAdapter supports only {typeof(SpeckleRequest).Name}.");
                return new List<object>();
            }

            return Read(speckleRequest);
        }
    }
}
