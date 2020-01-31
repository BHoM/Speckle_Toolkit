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
using Rhino;
using BH.Engine.Rhinoceros;

namespace BH.Engine.Speckle
{
    public static partial class Convert
    {
        // ------------------------------------------------------------------------------ //
        // DYNAMIC DISPATCHERS
        // These methods take a generic IBHoMObject or IObject
        // and use a dynamic dispatch to determine the most appropriate FromBHoM convert.
        // ------------------------------------------------------------------------------ //

        [Description("Wraps the content of a BHoMObject into a SpeckleObject.\n" +
            "Its geometry (if any) is exposed in the top level, so it can be visualised in Speckle.\n" +
            "All BHoMObject properties are json-serialised and saved into the `speckleObject.Properties` field.")]
        public static SpeckleObject IFromBHoM(this IBHoMObject bhomObject)
        {
            // This will be our return object.
            // On the higher level, it will contain geometry to be represented in SpeckleViewer.
            // Nested into its `.Properties` it will also "wrap" any other BHoM data.
            SpeckleObject speckleObject = null;

            // DYNAMICALLY DISPATCH to the most appropriate convert method
            speckleObject = FromBHoM(bhomObject as dynamic);

            if (speckleObject == null)
                return null;

            speckleObject.Properties = new Dictionary<string, object>();

            // Add the BHoMObject to the SpeckleObject Properties Dictionary via the Speckle Serialisation.
            // This appends the BHoM data as a "SpeckleAbstract" object.
            speckleObject.Properties.Add("BHoM", SpeckleCore.Converter.Serialise(bhomObject));

            // -------------------------- //  
            // For testing only           // 
            // -------------------------- // 

            // Serialise the BHoMobject into a Json and append it to the Properties Dictionary of the SpeckleObject. Key is "BHoMZipped".
            string BHoMDataJson = BH.Engine.Serialiser.Convert.ToJson(bhomObject); //serialize
            BHoMDataJson = BH.Engine.Serialiser.Convert.ToZip(BHoMDataJson); //zip 
            speckleObject.Properties.Add("BHoMZipped", BHoMDataJson);

            // -------------------------- //

            return speckleObject;
        }

        [Description("Wraps the content of a BHoM IGeometry into a SpeckleObject.\n" +
           "A Rhino geometry representation is extracted and exposed in the top level, so it can be visualised in Speckle.\n" +
           "All the other IGeometry properties are json-serialised and saved into the `speckleObject.Properties` field.")]
        public static SpeckleObject IFromBHoM(this IGeometry iGeometry)
        {
            var rhinoGeom = Engine.Rhinoceros.Convert.IToRhino(iGeometry);

            // Creates the SpeckleObject with the Rhino Geometry. 
            var speckleObj_rhinoGeom = (SpeckleObject)SpeckleCore.Converter.Serialise(rhinoGeom); // This will be our "wrapper" object for the rest of the IObject stuff.

            // Serialise the iGeometry into a Json and append it to the additional properties of the SpeckleObject.
            BH.Engine.Serialiser.Convert.ToJson(iGeometry);
            speckleObj_rhinoGeom.Properties = new Dictionary<string, object>() { { iGeometry.GetType().Name, BH.Engine.Serialiser.Convert.ToJson(iGeometry) } };

            return speckleObj_rhinoGeom;
        }
    }
}
