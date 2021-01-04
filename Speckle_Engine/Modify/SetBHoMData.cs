/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
                // Speckle "Serialiser" is very slow with BHoM classes.
                object serialisedBHoMData = SpeckleCore.Converter.Serialise(bhObject);
                speckleObject.Properties.Add("BHoM", serialisedBHoMData);
            }
            else
            {
                // Our ad-hoc Speckle Serialiser. Essentially the same with all non-needed Reflection things removed. x~100 faster.
                object serialisedRepresentation = Compute.SpeckleAbstract(bhObject);

                // You need to add the object "Serialised" representation as Speckle Viewer wants it,
                // in order for you to have object filtering/grouping/etc in the Viewer.
                speckleObject.Properties.Add(bhObject.GetType().Name, serialisedRepresentation);

                // This is the actual BHoM data.
                object serialisedBHoMData = BH.Engine.Serialiser.Convert.ToJson(bhObject);
                serialisedBHoMData = BH.Engine.Serialiser.Convert.ToZip(serialisedBHoMData as string); // zip the data so speckle doesn't try to process it, altering it.
                speckleObject.Properties.Add("BHoM", serialisedBHoMData);
            }

        }
    }
}

