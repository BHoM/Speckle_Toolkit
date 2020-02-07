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
        public static void SetBHoMData(SpeckleObject speckleObject, IObject bhObject, bool useSpeckleSerialiser = false)
        {
            speckleObject.Properties = new Dictionary<string, object>();

            // Serialise the IBHoMObject data and append it to the `Properties` of the SpeckleObject.
            if (useSpeckleSerialiser)
            {
                // Speckle "Serialiser" is incredibly slow.
                object speckleSerialised = Compute.SpeckleAbstract(bhObject);//SpeckleCore.Converter.Serialise(bhObject); // This "serialises" with Speckle: the BHoM data becomes a "SpeckleAbstract" object
                //speckleSerialised = BH.Engine.Serialiser.Convert.ToZip(speckleSerialised); // (optional) zip the data
                speckleObject.Properties.Add("BHoM", speckleSerialised);
            }
            else
            {
                // ~100 times faster, but we lose the ability of filtering/grouping objects per property in the SpeckleViewer.
                string BHoMDataJson = BH.Engine.Serialiser.Convert.ToJson(bhObject); // Serialize with our method.
                speckleObject.Properties.Add("BHoM", BHoMDataJson);
            }

           

        }
    }
}
