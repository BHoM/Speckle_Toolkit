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

namespace BH.Engine.Speckle
{
    public static partial class Convert
    {


        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        //Add methods for converting From BHoM to the specific software types, if possible to do without any BHoM calls
        //Example:
        //public static SpeckleNode FromBHoM(this Node node)
        //{
        //    //Insert code for convertion
        //}

        /***************************************************/
        // Helper Methods for SpeckleCoreGeometry
        public static double[] ToArray(this BHG.Point pt)
        {
            return new double[] { pt.X, pt.Y, pt.Z };
        }

        public static double[] ToFlatArray(this IEnumerable<BHG.Point> points)
        {
            return points.SelectMany(pt => pt.ToArray()).ToArray();
        }

        //public static HashSet<string> WarningCollection = new HashSet<string>();

        public static SpeckleObject FromBHoM(this IBHoMObject bhomObject, Dictionary<Type, MethodInfo> getGeometryMethods)
        {
            // Retrieve the BHoM geometry to represent the object in SpeckleViewer.
            IGeometry geom = bhomObject.GetGeometry(getGeometryMethods);

            if (geom == null)
            {
                // BHoMObject does not have a geometrical representation in BHoM.
                // It must be converted to an "Abstract" SpeckleObject.
                return (SpeckleObject)SpeckleCore.Converter.Serialise(bhomObject);
            }

            // Creates the SpeckleObject with the BHoM Geometry, dynamically dispatching to the method for the right type.
            SpeckleObject speckleObject = FromBHoM(geom as dynamic); // This will be our "wrapper" object for the rest of the BHoM stuff.

            // Convert ("Serialise") the whole BHoM object into a SpeckleObject,
            // to get the "Property" property where Speckle stores the Dictionary with all extra metadata.
            SpeckleObject bhomObj_serialized = (SpeckleObject)SpeckleCore.Converter.Serialise(bhomObject);
            speckleObject.Properties = bhomObj_serialized.Properties; // Copy the dictionary with all extra metadata into our "wrapper" speckleObject.

            return speckleObject;
        }


        /// <summary>
        /// Convert BHoM Point -> Speckle Point
        /// </summary>
        /// <returns>SpecklePoint object</returns>
        public static SpecklePoint FromBHoM(this BHG.Point bhomPoint)
        {
            if (bhomPoint == null) return default(SpecklePoint);

            SpecklePoint specklePoint = new SpecklePoint(bhomPoint.X, bhomPoint.Y, bhomPoint.Z);

            specklePoint.GenerateHash();
            return specklePoint;
        }

        /// <summary>
        /// Convert BHoM Vector -> Speckle Vector
        /// </summary>
        /// <returns>SpeckleVector object</returns>
        public static SpeckleVector FromBHoM(this BHG.Vector bhomVector)
        {
            if (bhomVector == null) return default(SpeckleVector);

            SpeckleVector speckleVector = new SpeckleVector(bhomVector.X, bhomVector.Y, bhomVector.X);

            speckleVector.GenerateHash();
            return speckleVector;
        }

        /// <summary>
        /// Convert BHoM Line -> Speckle Line
        /// </summary>
        /// <returns>SpeckleLine object</returns>
        public static SpeckleLine FromBHoM(this BHG.Line bhomLine)
        {
            if (bhomLine == null) return default(SpeckleLine);

            SpeckleLine speckleLine = new SpeckleLine(
                (new BHG.Point[] { bhomLine.Start, bhomLine.End }).ToFlatArray()
                );

            speckleLine.GenerateHash();
            return speckleLine;
        }

        /// <summary>
        /// Convert BHoM Mesh -> Speckle Mesh
        /// </summary>
        /// <returns>SpeckleMesh object</returns>
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

    }
}
