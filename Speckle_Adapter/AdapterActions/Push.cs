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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BH.oM.Adapter;
using BH.oM.Base;
using BH.oM.Data.Requests;
using SpeckleCore;
using BH.oM.Speckle;
using System.ComponentModel;
using BH.Engine.Base;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        public override List<object> Push(IEnumerable<object> objects, string tag = "", PushType pushType = PushType.AdapterDefault, ActionConfig actionConfig = null)
        {
            // Clone objects for immutability in the UI
            List<object> objectsToPush = objects.Select(x => x.DeepClone()).ToList();

            // Initialize Speckle Layer if not existing
            if (SpeckleLayer == null)
            {
                SpeckleLayer = new Layer() { Name = "Default Layer", OrderIndex = 0, StartIndex = 0, Topology = "", Guid = "c8a58593-7080-450b-96b9-b0158844644b" };
                SpeckleLayer.ObjectCount = 0;

            }

            // Initialize the SpeckleStream
            SpeckleStream.Layers = new List<Layer>() { SpeckleLayer };
            SpeckleStream.Objects = new List<SpeckleObject>(); // --> shit's immutable, yo


            /// -------------------
            /// Base push rewritten
            /// -------------------

            // //- Read config
            SpecklePushConfig pushConfig = actionConfig as SpecklePushConfig;
            if (pushConfig == null)
                pushConfig = new SpecklePushConfig();


            // //- Use "Speckle" history: produces a new stream at every push that corresponds to the old version. Enabled by default.
            if (pushConfig.EnableHistory)
                SetupHistory();

            // // - Base push rewritten to allow some additional CustomData to go in.
            if (PushByType(objectsToPush, tag, pushConfig))
                return objectsToPush;

            return new List<object>();
        }
    }
}
