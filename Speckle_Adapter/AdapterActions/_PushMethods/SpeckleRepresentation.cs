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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using BHG = BH.oM.Geometry;
using SpeckleCoreGeometryClasses;
using BH.oM.Base;
using System.Reflection;
using BH.oM.Geometry;
using BH.Engine.Base;
using System.ComponentModel;
using BH.oM.Speckle;
using BH.oM.Graphics;
using BH.Engine.Representation;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        [Description("Attempts to compute a SpeckleObject representation of the BHoMObject, so it can be visualised in the SpeckleViewer.")]
        public static SpeckleObject SpeckleRepresentation(IBHoMObject bhomObject, RenderMeshOptions renderMeshOptions = null)
        {
            renderMeshOptions = renderMeshOptions ?? new RenderMeshOptions();

            SpeckleObject speckleRepresentation = null;

            if (bhomObject is CustomObject)
                return null;

            // See if the object contains a custom Mesh Representation in its Fragments.
            Mesh meshRepresentation = null;
            RenderMesh renderMesh = null;
            if (bhomObject != null)
            {
                RenderMesh renderMeshObj = null;
                bhomObject.TryGetRendermesh(out renderMeshObj);

                if (renderMeshObj != null)
                    meshRepresentation = new Mesh() { Faces = renderMeshObj.Faces, Vertices = renderMeshObj.Vertices.Select(v => v.Point).ToList() };
            }

            if (renderMesh != null)
            {
                meshRepresentation = new Mesh() { Faces = renderMesh.Faces, Vertices = renderMesh.Vertices.Select(v => new oM.Geometry.Point() { X = v.Point.X, Y = v.Point.Y, Z = v.Point.Z }).ToList() };
                return meshRepresentation.ToSpeckle();
            }

            if (meshRepresentation != null)
                return meshRepresentation.ToSpeckle();

            // See if there is a custom BHoM Geometry representation for that BHoMObject.
            // If so, attempt to convert it to Speckle.
            IGeometry geometricalRepresentation = null;
            try
            {
                geometricalRepresentation = BH.Engine.Representation.Compute.IGeometricalRepresentation(bhomObject, renderMeshOptions.RepresentationOptions);
            }
            catch { }

            // If found, attempt to convert the BHoM Geometrical representation of the object to Speckle geometry.
            if (geometricalRepresentation != null)
                speckleRepresentation = geometricalRepresentation.IToSpeckle();

            // If the convert of the base geometry to Speckle Geometry didn't work, 
            // try to obtain a BHoM RenderMesh of the geometrical representation, then convert it to a Speckle mesh.
            if (speckleRepresentation == null)
            {
                try
                {
                    // Do not use the interface method IRenderMesh here. That one re-computes the geometrical representation.
                    renderMesh = BH.Engine.Representation.Compute.RenderMesh(geometricalRepresentation as dynamic);
                }
                catch { }

                if (renderMesh != null)
                    speckleRepresentation = Speckle.Convert.ToSpeckle(renderMesh);
            }

            return speckleRepresentation;
        }
    }
}


