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

namespace BH.Engine.Speckle
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Attempts to retrieve a geometry out of the bhomObject, and use it to visualise the object in the SpeckleViewer.")]
        // This method is mainly used as a fallback case for the dynamic dispatch done in IFromBHoM:
        // if the type is a child of BHoMObject, and no specific convert the child type it is found, this gets called.
        public static SpeckleObject FromBHoM(this BHoMObject bhomObject)
        {
            SpeckleObject speckleObject = null;

            // See if we can get a BHoM geometry out of the BHoMObject to represent the object in SpeckleViewer.
            IGeometry geom = null;
            geom = bhomObject.IGeometry();

            if (geom != null)
            {
                // Converts the BHoM Geometry into a SpeckleObject, dynamically dispatching to the method for the right type.
                speckleObject = IFromBHoM(geom as dynamic);
            }
            else
            {
                // BHoMObject does not have a geometrical representation in BHoM.
                // It must be converted to an "Abstract" SpeckleObject,
                // which won't have any visualisation in the SpeckleAdmin Viewer.
                speckleObject = (SpeckleObject)SpeckleCore.Converter.Serialise(bhomObject);
            }

            return speckleObject;
        }

    }
}
