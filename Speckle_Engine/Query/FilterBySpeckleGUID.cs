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

using BH.Engine.Adapter;
using BH.oM.Base;
using BH.oM.Speckle;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Speckle
{
    public static partial class Query
    {
        [Description("Return only BHoMObjects that have the specified Speckle id in their Fragments; or all objects if no id is specified.")]
        public static IEnumerable<object> FilterBySpeckleGUID(IEnumerable<object> objects, List<string> speckleIds = null, string speckleAdapterIdName = "Speckle_id")
        {
            // SpeckleGUID is stored in Fragments, which is only available for BHoMObjects.
            if (speckleIds != null && speckleIds.Count != 0)
                return objects
                    .OfType<IBHoMObject>()
                    .Where(o => speckleIds.Any(id => id == o.AdapterId(typeof(SpeckleId)).ToString()));
            else
                return objects;
        }
    }
}




