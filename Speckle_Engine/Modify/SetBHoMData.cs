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
using SpeckleCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Speckle
{
    public static partial class Modify
    {
        [Description("Saves all BHoM Data inside the Speckle object.")]
        public static void SetBHoMData(ref SpeckleObject speckleObject, IObject bhObject)
        {
            // Serialise the IBHoMObject data and append it to the `Properties` of the SpeckleObject.
            speckleObject.Properties = new Dictionary<string, object>() {
                {
                   "BHoM",
                   SpeckleCore.Converter.Serialise(bhObject) // This appends the BHoM data as a "SpeckleAbstract" object.
                }
            };

            // Set `speckleObject.Name` as the BHoMObject type name.
            speckleObject.Name = bhObject.GetType().Name;

            if (false && bhObject is BHoMObject) // for testing only
                AddZippedBHoMData(ref speckleObject, bhObject);

            speckleObject.GenerateHash(); // Not sure if needed
        }

        // For testing only
        private static void AddZippedBHoMData(ref SpeckleObject speckleObject, IObject bhObject)
        {
            // Serialise the BHoMobject into a Json and append it to the Properties Dictionary of the SpeckleObject. Key is "BHoMZipped".
            string BHoMDataJson = BH.Engine.Serialiser.Convert.ToJson(bhObject); //serialize
            BHoMDataJson = BH.Engine.Serialiser.Convert.ToZip(BHoMDataJson); //zip 
            speckleObject.Properties.Add("BHoMZipped", BHoMDataJson);
        }
    }
}
