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

using BH.oM.Data.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Speckle
{
    [Description("Used to Pull only query objects that satifisfy the given conditions.")]
    public class SpeckleRequest : IRequest
    {
        [Description("SpeckleGUID is the id that Speckle assigned to the objects when uploaded. Only objects with the specified SpeckleGUID will be returned by the Pull.")]
        public List<string> SpeckleGUIDs { get; set; } // e.g. https://hestia.speckle.works/api/v1/streams/s8wPVeeqe/objects?_id=5df9fccf6c95664adc770e4c (does not work)

        [Description("SpeckleHash is the id that Speckle assigned to the objects when uploaded. Only objects with the specified SpeckleGUID will be returned by the Pull.")]
        public List<string> SpeckleHash { get; set; } // e.g. https://hestia.speckle.works/api/v1/streams/s8wPVeeqe/objects?hash=eea8586e084c18d795d127c65a9b30ca

        [Description("Maximum number of objects downloaded from the Speckle Stream.")]
        public int? Limit { get; set; } = null;

        [Description("String containing any query interpretable by Speckle. See https://speckle.systems/docs/developers/api-specs/ -> Query.\n" +
            "If specified, this field takes precedence over all others. Empty by default.")]
        public string SpeckleQuery { get; set; } = "";
    }
}
