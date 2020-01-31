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
        [Description("Return only BHoMObjects that have the specified BHoMGUID in their CustomData property; or all objects if no id is specified.")]
        public static IEnumerable<object> FilterByBHoMGUID(IEnumerable<object> objects, List<string> bhomGuids = null)
        {
            // BHoMGUID is only available for BHoMObjects.
            if (bhomGuids != null && bhomGuids.Count != 0)
                return objects
                .OfType<IBHoMObject>()
                .Where(o => bhomGuids.Any(id => id == o.BHoM_Guid.ToString())); //Inefficient o(n*m) -- implement some kind of sorting https://stackoverflow.com/a/25184658/3873799
            else
                return objects;
        }
    }
}
