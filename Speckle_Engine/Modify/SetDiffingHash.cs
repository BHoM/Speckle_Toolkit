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

using BH.oM.Base;
using BH.oM.Speckle;
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
        [Description("Sets the Speckle Object `Hash` property as the BHoM Diffing hash. See the method's body comments for more information.")]
        public static void SetDiffingHash(this SpeckleObject speckleObject, IObject sourceObj, SpecklePushConfig config)
        {
            // SETTING THE HASH(ES)
            // SpeckleObjects have 2 hash properties: `Hash` and `GeometryHash`.
            // `Hash` is the "main" one, on which Speckle bases its diffing.
            // Therefore, what we want is that `Hash` should be set based only on the BHoM class properties - 
            // not on any Speckle Object property, because we use the SpeckleObject only as a Representational container for our BHoMObject.
            // This way objects from BHoM can be diffed by the Speckle Server the same way we would diff them on the client side.
            
            // HOWEVER
            // This poses an issue with the fact tha speckle will not update the view in SpeckleViewer if only changes to the object REPRESENTATION are made.
            // E.g. If at some point I change how I want to see the bars, from 'extruded' to 'simple line', 
            // then SpeckleViewer will not update the view (bars will stay extruded);
            // only newly pushed bars will be simple lines.

            // This is because Speckle really does not make any use of the `GeometryHash`:
            speckleObject.GeometryHash = speckleObject.Hash; // this is unfortunately useless; Speckle doesn't use it.

            // Set the "main" speckle hash equal to the diffing hash, so Speckle can do the diffing as we expect.
            string diffingHash = BH.Engine.Diffing.Compute.DiffingHash(sourceObj, config.DiffConfig);
            speckleObject.Hash = diffingHash;

            // FOR DEVELOPMENT ONLY:
            if (config.UniqueRandomHash)
                speckleObject.Hash += speckleObject.GeometryHash + System.DateTime.Now.Ticks;

        }
    }
}


