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

namespace BH.Engine.Speckle
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Convert BHoM Point to a Speckle Point")]
        public static SpecklePoint FromBHoM(this BHG.Point bhomPoint)
        {
            if (bhomPoint == null) return default(SpecklePoint);

            SpecklePoint specklePoint = new SpecklePoint(bhomPoint.X, bhomPoint.Y, bhomPoint.Z);

            specklePoint.GenerateHash();
            return specklePoint;
        }

        [Description("Convert BHoM Vector to a Speckle Vector")]
        public static SpeckleVector FromBHoM(this BHG.Vector bhomVector)
        {
            if (bhomVector == null) return default(SpeckleVector);

            SpeckleVector speckleVector = new SpeckleVector(bhomVector.X, bhomVector.Y, bhomVector.X);

            speckleVector.GenerateHash();
            return speckleVector;
        }

        [Description("Convert BHoM Line to a Speckle Line")]
        public static SpeckleLine FromBHoM(this BHG.Line bhomLine)
        {
            if (bhomLine == null) return default(SpeckleLine);

            SpeckleLine speckleLine = new SpeckleLine(
                (new BHG.Point[] { bhomLine.Start, bhomLine.End }).ToFlatArray()
                );

            speckleLine.GenerateHash();

            return speckleLine;
        }

        [Description("Convert BHoM Mesh to a Speckle Mesh")]
        public static SpeckleMesh FromBHoM(this BHG.Mesh bhomMesh)
        {
            double[] vertices = bhomMesh.Vertices.ToFlatArray();
            int[] faces = bhomMesh.Faces.SelectMany(face =>
            {
                if (face.D != -1) return new int[] { 1, face.A, face.B, face.C, face.D };
                return new int[] { 0, face.A, face.B, face.C };
            }).ToArray();
            var defaultColour = System.Drawing.Color.FromArgb(255, 100, 100, 100);
            var colors = Enumerable.Repeat(defaultColour.ToArgb(), vertices.Count()).ToArray();

            SpeckleMesh speckleMesh = new SpeckleMesh(vertices, faces, colors, null);

            speckleMesh.GenerateHash();
            return speckleMesh;
        }

        public static SpeckleObject FromBHoM(this Node node)
        {
            var mesh = node.MeshRepresentation();
            if (mesh == null)
                return null;

            var speckleMesh = (SpeckleMesh)SpeckleCore.Converter.Serialise(mesh);
            speckleMesh.Colors = new List<int>() { 0, 0, 0 };

            var def = (SpeckleAbstract)SpeckleCore.Converter.Serialise(node);
            def.Properties["displayValue"] = speckleMesh;

            return speckleMesh;
        }

        public static SpeckleObject FromBHoM(this Bar bar)
        {
            if (bar.SectionProperty == null)
            {
                IGeometry geom = bar.IGeometry();
                return FromBHoM(geom);
            }

            bool extrudeSimple = false; // to be exposed within config options.

            if (extrudeSimple)
            {
                IGeometry simpleExtrusion = bar.Extrude(true).First();
                return IFromBHoM(simpleExtrusion as dynamic);
            }

            // Gets the BH.oM.Geometry.Extrusion out of the Bar. If the profile is made of two curves (e.g. I section), selects only the outermost.
            var barOutermostExtrusion = bar.Extrude(false).Cast<Extrusion>().OrderBy(extr => extr.Curve.IArea()).First();

            // Obtains the Rhino extrusion.
            var rhinoExtrusion = Rhino.Geometry.Extrusion.CreateExtrusion(barOutermostExtrusion.Curve.IToRhino(), (Rhino.Geometry.Vector3d)barOutermostExtrusion.Direction.IToRhino());
            Rhino.Geometry.Mesh mesh = Rhino.Geometry.Mesh.CreateFromSurface(rhinoExtrusion, Rhino.Geometry.MeshingParameters.Minimal);

            // Add the endnodes representations.
            mesh.Append(bar.StartNode.MeshRepresentation());
            mesh.Append(bar.EndNode.MeshRepresentation());

            return (SpeckleObject)SpeckleCore.Converter.Serialise(mesh);
        }

        public static SpeckleObject FromBHoM(this IBHoMObject bhomObject)
        {
            SpeckleObject speckleObject = null;

            // See if we can get a BHoM geometry out of the BHoMObject to represent the object in SpeckleViewer.
            IGeometry geom = null;
            geom = bhomObject.IGeometry();

            if (geom != null)
            {
                // Converts the BHoM Geometry into a SpeckleObject, dynamically dispatching to the method for the right type.
                speckleObject = IFromBHoM(geom as dynamic);
            }
            else
            {
                // BHoMObject does not have a geometrical representation in BHoM.
                // It must be converted to an "Abstract" SpeckleObject,
                // which won't have any visualisation in the SpeckleAdmin Viewer.
                speckleObject = (SpeckleObject)SpeckleCore.Converter.Serialise(bhomObject);
            }

            return speckleObject;
        }

        public static SpeckleObject FromBHoM(this IGeometry geom)
        {
            return IFromBHoM(geom);
        }

        /// <summary>
        /// Extension method to convert bhom meshes to speckle meshes. 
        /// Will get called automatically in the speckle "Serialise" method.
        /// https://github.com/speckleworks/SpeckleCore/blob/9545e96f04d85f46203a99c21c76eeea0ea03dae/SpeckleCore/Conversion/ConverterSerialisation.cs#L94
        /// </summary>
        //public static object FromBHoM(this BH.oM.Geometry.Mesh bhomMesh)
        //{
        //    object specklemesh = null;

        //    // Write conversion here


        //    return specklemesh;
        //}

        //public static void test()
        //{
        //    BH.oM.Geometry.Mesh bhomMesh = new oM.Geometry.Mesh();

        //    object speckleMesh = bhomMesh.FromBHoM();

        //}


        /***************************************************/
        /**** Private Helper Methods                    ****/
        /***************************************************/
        // Helper Methods for SpeckleCoreGeometry

        private static double[] ToArray(this BHG.Point pt)
        {
            return new double[] { pt.X, pt.Y, pt.Z };
        }

        private static double[] ToFlatArray(this IEnumerable<BHG.Point> points)
        {
            return points.SelectMany(pt => pt.ToArray()).ToArray();
        }
    }
}
