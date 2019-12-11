using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using BHG = BH.oM.Geometry;
using SpeckleCoreGeometryClasses;


namespace BH.Engine.Speckle
{
    // initialise to make sure all Speckle kits are loaded
    public class Initialiser : ISpeckleInitializer
    {
        public Initialiser() { }
    }

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
        // Helper Methods from SpeckleCoreGeometry
        public static double[] ToArray(this BHG.Point pt)
        {
            return new double[] { pt.X, pt.Y, pt.Z };
        }

        public static double[] ToFlatArray(this IEnumerable<BHG.Point> points)
        {
            return points.SelectMany(pt => pt.ToArray()).ToArray();
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
