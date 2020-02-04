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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Speckle
{
    public class SpecklePushConfig : BH.oM.Adapter.ActionConfig
    {
        [Description("Decide the level of detail of the geometrical representation of BHoMObjects in the SpeckleViewer. Affects the upload/download time and general performance.")]
        public SpeckleDisplayOptions DisplayOption { get; set; } = new SpeckleDisplayOptions();

        [Description("Enables Speckle history.\n" +
            "Speckle does history by cloning the stream and saving it between the children of the main stream. The head of the stream is the latest version.")]
        public bool EnableHistory { get; set; } = true;

        [Description("After the Push, the objects are downloaded to read their SpeckleId, which is then stored in their CustomData property.\n" +
            "The CustomData dictionary is only available for BHoMObjects.")]
        // This does not work since I switched to BH.Engine deserialisation in the Pull. Issue is that our deserialisation "recreates" the objects without preserving the original GUID.
        public bool StoreSpeckleId { get; set; } = true;
    }
}
