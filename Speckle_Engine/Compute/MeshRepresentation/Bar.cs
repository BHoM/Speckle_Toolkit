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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using BHG = BH.oM.Geometry;
using SpeckleCoreGeometryClasses;
using BH.oM.Base;
using BH.Engine.Geometry;
using System.Reflection;
using BH.oM.Geometry;
using BH.Engine.Base;
using System.ComponentModel;
using BH.oM.Structure.Elements;
using BH.Engine.Structure;
using BH.Engine.Rhinoceros;
using BH.oM.Structure.Constraints;

namespace BH.Engine.Speckle
{
    public static partial class Compute
    {
        [Description("Returns a BHoM mesh representation for the BHoM Bar based on its SectionProperty.")]
        public static BH.oM.Geometry.Mesh MeshRepresentation(this Bar bar)
        {
            return RhinoMeshRepresentation(bar).IToBHoM() as BH.oM.Geometry.Mesh;
        }

        [Description("Returns a Rhino mesh representation for the BHoM Bar based on its SectionProperty.")]
        private static Rhino.Geometry.Mesh RhinoMeshRepresentation(this Bar bar)
        {
            // Gets the BH.oM.Geometry.Extrusion out of the Bar. If the profile is made of two curves (e.g. I section), selects only the outermost.
            var barOutermostExtrusion = bar.Extrude(false).Cast<Extrusion>().OrderBy(extr => extr.Curve.IArea()).First();

            // Obtains the Rhino extrusion.
            var rhinoExtrusion = Rhino.Geometry.Extrusion.CreateExtrusion(barOutermostExtrusion.Curve.IToRhino(), (Rhino.Geometry.Vector3d)barOutermostExtrusion.Direction.IToRhino());
            Rhino.Geometry.Mesh mesh = Rhino.Geometry.Mesh.CreateFromSurface(rhinoExtrusion, Rhino.Geometry.MeshingParameters.Minimal);

            // Add the endnodes representations.
            mesh.Append(bar.StartNode.RhinoMeshRepresentation());
            mesh.Append(bar.EndNode.RhinoMeshRepresentation());

            return mesh;
        }
    }
}

