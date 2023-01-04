/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

using BH.oM.Diffing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Graphics;

namespace BH.oM.Speckle
{
    public class SpecklePushConfig : BH.oM.Adapter.ActionConfig
    {
        [Description("Decides the level of detail of the geometrical representations of BHoMObjects in the SpeckleViewer. Affects the upload/download time and general performance.")]
        public virtual RenderMeshOptions RendermeshOptions { get; set; } = new RenderMeshOptions();

        [Description("Enables Speckle history.\n" +
            "Speckle clones the previous version of the stream and saves it in the `children` property of the main stream. The head of the stream is the latest version.")]
        public virtual bool EnableHistory { get; set; } = true;

        [Description("(ADVANCED FEATURE)\n" +
            "Using the Speckle Serialiser enables to group the BHoM Object per their properties in the SpeckleViewer." +
            "However, this is ~100 slower than using our JSON serialiser.")]
        public virtual bool UseSpeckleSerialiser { get; set; } = false;

        [Description("(ADVANCED FEATURE)\n" +
            "If true, the objects' hash will be randomly defined and unique, so at every Push will see them as entirely new objects, even if they stay the same.")]
        public virtual bool UniqueRandomHash { get; set; } = false;

        [Description("(ADVANCED FEATURE)\n" +
           "DiffConfig for the Hash calculation. The objects' hash in Speckle is set based on these configurations.")]
        public override DiffConfig DiffConfig { get; set; } = new DiffConfig();

        //[Description("After the Push, the objects are downloaded to read their SpeckleId, which is then stored in their Fragments property.")]
        //// This does not work since I switched to BH.Engine deserialisation in the Pull. Issue is that our deserialisation "recreates" the objects without preserving the original GUID.
        //public bool StoreSpeckleId { get; set; } = true;
    }
}



