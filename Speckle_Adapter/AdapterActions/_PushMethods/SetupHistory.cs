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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Geometry;
using SpeckleCore;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter : BHoMAdapter
    {
        [Description("Creates a new stream (with a different StreamId), where the current stream content is copied, before it gets modified.")]
        private void SetupHistory()
        {
            ResponseStreamClone cloneResult = null;
            Task<ResponseStreamClone> respStreamClTask = null;

            // The following line creates a new stream (with a different StreamId), where the current stream content is copied, before it gets modified.
            // The streamId of the cloned "backup" is saved among the main Stream "children" field,
            // accessible through SpeckleServerAddress/api/v1/streams/streamId
            respStreamClTask = SpeckleClient.StreamCloneAsync(SpeckleClient.Stream.StreamId);

            try
            {
                cloneResult = respStreamClTask?.Result;
                SpeckleClient.Stream.Children.Add(cloneResult.Clone.StreamId);
            }
            catch (Exception e)
            {
                BH.Engine.Reflection.Compute.RecordWarning($"Failed configuring Speckle History. Error: {e.InnerException}");
            }

            if (cloneResult == null)
                BH.Engine.Reflection.Compute.RecordWarning($"Failed configuring Speckle History. Task status: {respStreamClTask.Status.ToString()}");
        }
    }
}