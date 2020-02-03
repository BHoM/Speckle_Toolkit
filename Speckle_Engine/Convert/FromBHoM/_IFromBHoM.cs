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
        [Description("Wraps the content of a BHoMObject into a SpeckleObject.\n" +
            "Its geometry (if any) is exposed in the top level, so it can be visualised in Speckle.\n" +
            "All BHoMObject properties are json-serialised and saved into the `speckleObject.Properties` field.")]
        public static SpeckleObject IFromBHoM(this IBHoMObject bhomObject)
        {
            // This will be our return object.
            // On the highest level, it will simply be a SpeckleObject containing geometry that can be visualised in the SpeckleViewer.
            SpeckleObject speckleObject = null;
            speckleObject = SpeckleRepresentation(bhomObject); // Attempts to obtain a Speckle representation.

            if (speckleObject == null)
            {
                // No Speckle Representation found.
                // BHoMObject will be sent as "Abstract" SpeckleObject, with no visualisation in the SpeckleAdmin Viewer.
                speckleObject = (SpeckleObject)SpeckleCore.Converter.Serialise(bhomObject);
            }

            // Serialise the IBHoMObject data and append it to the `Properties` of the SpeckleObject.
            speckleObject.Properties = new Dictionary<string, object>();
            speckleObject.Properties.Add("BHoM", SpeckleCore.Converter.Serialise(bhomObject)); // This appends the BHoM data as a "SpeckleAbstract" object.

            if (false) // for testing only
                AddZippedBHoMData(ref speckleObject, bhomObject);

            speckleObject.GenerateHash(); // Not sure if needed

            return speckleObject;
        }


        [Description("Wraps the content of a BHoM IGeometry into a SpeckleObject.\n" +
           "A Rhino geometry representation is extracted and exposed in the top level, so it can be visualised in Speckle.\n" +
           "All the other IGeometry properties are json-serialised and saved into the `speckleObject.Properties` field.")]
        public static SpeckleObject IFromBHoM(this IObject iObject)
        {
            // This will be our return object.
            // On the highest level, it will simply be a SpeckleObject containing geometry that can be visualised in the SpeckleViewer.
            SpeckleObject speckleObject = null;

            if (typeof(IGeometry).IsAssignableFrom(iObject.GetType()))
                speckleObject = BH.Engine.Speckle.Convert.FromBHoM((IGeometry)iObject as dynamic); // DYNAMIC DISPATCH
            else
                speckleObject = (SpeckleObject)SpeckleCore.Converter.Serialise(iObject); // These will be exported as `Abstract` Speckle Objects.

            // Serialise the IObject data and append it to the `Properties` of the SpeckleObject.
            speckleObject.Properties = new Dictionary<string, object>() { { "BHoM", SpeckleCore.Converter.Serialise(iObject) } };

            speckleObject.GenerateHash(); // Not sure if needed

            return speckleObject;
        }


        // For testing only
        private static void AddZippedBHoMData(ref SpeckleObject speckleObject, IBHoMObject bhomObject)
        {
            // Serialise the BHoMobject into a Json and append it to the Properties Dictionary of the SpeckleObject. Key is "BHoMZipped".
            string BHoMDataJson = BH.Engine.Serialiser.Convert.ToJson(bhomObject); //serialize
            BHoMDataJson = BH.Engine.Serialiser.Convert.ToZip(BHoMDataJson); //zip 
            speckleObject.Properties.Add("BHoMZipped", BHoMDataJson);
        }
    }
}