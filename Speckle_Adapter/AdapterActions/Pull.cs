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
        public override IEnumerable<object> Pull(IRequest query, Dictionary<string, object> config = null)
        {
            var response = SpeckleClient.StreamGetObjectsAsync(SpeckleStreamId, "").Result;

            List<IBHoMObject> bHoMObjects = new List<IBHoMObject>();
            List<IObject> iObjects = new List<IObject>();
            List<object> reminder = new List<object>();

            bool assignSpeckleIdToBHoMObjects = true;

            if (query == null)
            {

                if (!BH.Engine.Speckle.Convert.ToBHoM(response, out bHoMObjects, out iObjects, out reminder, assignSpeckleIdToBHoMObjects))
                    BH.Engine.Reflection.Compute.RecordError("Failed to deserialize and cast the Server response into BHoM objects.");

                return bHoMObjects.Concat(iObjects).Concat(reminder);
            }
            else
            {
                /// -------------------
                /// Base Pull rewritten
                /// -------------------

                // Make sure this is a FilterQuery
                FilterRequest filter = query as FilterRequest;
                if (filter == null)
                {
                    Engine.Reflection.Compute.RecordWarning("Please specify a FilterQuery");
                    return new List<object>();
                }

                List<string> speckleIds = QueryToSpeckleIds(filter);

                // Read the IBHoMObjects
                BH.Engine.Speckle.Convert.ToBHoM(response, out bHoMObjects, out iObjects, out reminder, assignSpeckleIdToBHoMObjects, speckleIds);

                // Filter by tag if any 
                bHoMObjects = filter.Tag == "" ? bHoMObjects : bHoMObjects.Where(x => x.Tags.Contains(filter.Tag)).ToList();

                // Return stuff
                if (typeof(IBHoMObject).IsAssignableFrom(filter.Type))
                    return bHoMObjects;
                else if (typeof(IObject).IsAssignableFrom(filter.Type))
                    return iObjects;
                else
                    return bHoMObjects.Concat(iObjects).Concat(reminder);

            }

            return new List<object>();
        }
    }
}
