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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using BHG = BH.oM.Geometry;
using SpeckleCoreGeometryClasses;
using BH.oM.Base;
using BH.Engine.Geometry;
using System.Reflection;
using BH.oM.Geometry;
using BH.Engine.Base;
using System.ComponentModel;
using BH.oM.Structure.Elements;
using BH.Engine.Structure;
using BH.Engine.Rhinoceros;
using BH.oM.Speckle;
using SpeckleCore;
using BH.oM.Diffing;
using BH.Engine.Diffing;

namespace BH.Engine.Speckle
{
    public static partial class Convert
    {
        [Description("Convert BHoM Line to a Speckle Line")]
        public static SpeckleDelta Delta(this Delta bhDelta)
        {
            try
            {
                SpeckleDelta speckleDelta = new SpeckleDelta();
                speckleDelta.Created = bhDelta.Diff.NewObjects.Cast<SpeckleObject>().ToList();
                speckleDelta.Common = bhDelta.Diff.ModifiedObjects.Select(o => new SpecklePlaceholder() { _id = ((SpeckleObject)o)._id }).ToList();
                speckleDelta.Deleted = bhDelta.Diff.OldObjects.Select(o => new SpecklePlaceholder() { _id = ((SpeckleObject)o)._id }).ToList();

                return speckleDelta;
            }
            catch (Exception e)
            {
                BH.Engine.Reflection.Compute.RecordError("Could not prepare the Delta payload.");
            }

            return null;
        }
    }
}
