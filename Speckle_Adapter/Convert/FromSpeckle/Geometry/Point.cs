/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BHG = BH.oM.Geometry;
using System.ComponentModel;
using SCG = SpeckleCoreGeometryClasses;

namespace BH.Adapter.Speckle
{
    public static partial class Convert
    {
        // -------------------------------------------------------------------------------- //
        // NOTE
        // These FromSpeckle methods are not automatically called by any method in the Toolkit,
        // as the deserialisation already brings back the BHoM object.
        // Kept for reference and for manual use in the UI.
        // -------------------------------------------------------------------------------- //

        [Description("Convert Speckle Point to BHoM Point")]
        public static BHG.Point FromSpeckle(this SCG.SpecklePoint specklePoint)
        {
            return new BHG.Point { X = specklePoint.Value[0], Y = specklePoint.Value[1], Z = specklePoint.Value[2] };
        }
    }
}


