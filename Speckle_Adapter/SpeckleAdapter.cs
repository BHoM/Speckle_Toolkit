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
using BH.oM.Base;
using BH.oM.Data.Requests;
using SpeckleCore;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter : BHoMAdapter
    {
        public SpeckleAdapter(SpeckleCore.Account speckleAccount, string speckleStreamId, string speckleStreamName = "Anonymous stream")
        {
            if (string.IsNullOrWhiteSpace(speckleStreamId))
            {
                BH.Engine.Reflection.Compute.RecordError("StreamId can't be null or empty.");
                return;
            }

            AdapterIdName = BH.Adapter.Speckle.Convert.AdapterIdName;

            SpeckleStream SpeckleStream = new SpeckleStream() { StreamId = speckleStreamId, Name = speckleStreamName };

            SpeckleClient = new SpeckleApiClient() { BaseUrl = speckleAccount.RestApi, AuthToken = speckleAccount.Token, Stream = SpeckleStream, StreamId = SpeckleStream.StreamId};
            SpeckleClient.SetupWebsocket();
        }
        
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/
        public SpeckleApiClient SpeckleClient { get; private set; }
    }
}
