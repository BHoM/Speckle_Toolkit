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
using BH.oM.Data;
using BH.oM.Data.Collections;
using BH.oM.Geometry;
using SpeckleCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BH.Engine.Speckle;
using BH.oM.Speckle;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        protected SpeckleObject ToSpeckle(IBHoMObject bhomObject, SpecklePushConfig config)
        {
            // Assign SpeckleStreamId to the Fragments of the IBHoMObjects
            SetAdapterId(bhomObject, SpeckleClient.Stream.StreamId);

            // SpeckleObject "container". Top level has geometry representation of BHoM Object, so it can be visualised in the SpeckleViewer. 
            SpeckleObject speckleObject = SpeckleRepresentation(bhomObject, config.RendermeshOptions);

            // If no Speckle representation is found, it will be sent as an "Abstract" SpeckleObject (no visualisation).
            if (speckleObject == null)
                speckleObject = (SpeckleObject)SpeckleCore.Converter.Serialise(bhomObject);

            // Save BHoMObject data inside the speckleObject.
            Modify.SetBHoMData(speckleObject, bhomObject, config.UseSpeckleSerialiser);

            speckleObject.SetDiffingHash(bhomObject, config);

            // If the BHoMObject has a "RevisionName" string field in the Customdata, 
            // use that value to create a Layer for it, and set the SpeckleObject's layer.
            //object revisionValue = null;
            //string revisionName = "";
            //if (bhomObject.CustomData.TryGetValue("RevisionName", out revisionValue))
            //{
            //    revisionName = revisionValue as string;
            //    if (!string.IsNullOrWhiteSpace(revisionName))
            //        speckleObject.Properties["revisionName"] = revisionName;
            //}

            return speckleObject;
        }
    }
}
