using BH.oM.Base;
using SpeckleCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHG = BH.oM.Geometry;
using SCG = SpeckleCoreGeometryClasses;

namespace BH.Engine.Speckle
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        // Mass point converter from SpeckleCoreGeometry, but with list rather than array        
        /// <summary>
        /// Mass point converter adapted from SpeckleCoreGeometry
        /// </summary>
        /// <param name = "arr" > Flat array of coordinates from Speckle </ param >
        /// <returns>List of BHoM Points</returns>
        public static List<BHG.Point> ToPoints(this IEnumerable<double> arr)
        {
            if (arr.Count() % 3 != 0) throw new Exception("Array malformed: length%3 != 0.");

            List<BHG.Point> points = new List<BHG.Point>();
            var asArray = arr.ToArray();
            for (int i = 2, k = 0; i < arr.Count(); i += 3)
                points[k++] = new BHG.Point { X = asArray[i - 2], Y = asArray[i - 1], Z = asArray[i] };

            return points;
        }

        public static IEnumerable<IBHoMObject> ToBHoM(ResponseObject response, bool setAssignedId = true, List<string> speckleIds = null)
        {
            List<IBHoMObject> bhomObjects = new List<IBHoMObject>();
            List<IObject> iObjects;
            List<object> reminder;

            ToBHoM(response, out bhomObjects, out iObjects, out reminder, setAssignedId, speckleIds);

            return bhomObjects;
        }

        public static bool ToBHoM(ResponseObject response, 
                                    out List<IBHoMObject> bHoMObjects, out List<IObject> iObjects, out List<object> reminder, 
                                    bool assignSpeckleIdToBHoMObjects = true, List<string> speckleIds = null)
        {
            bHoMObjects = new List<IBHoMObject>();
            iObjects = new List<IObject>();
            reminder = new List<object>();

            for (int i = 0; i < response.Resources.Count; i++)
            {
                var resource = Converter.Deserialise(response.Resources[i]);
                var gasd = Converter.Deserialise(response.Resources[i].Properties.Values.OfType<SpeckleObject>());
                var nestedBHoMProperties = response.Resources[i].Properties;

                // If we are filtering by speckleId, skip the object if its speckleId doesn't match.
                if (speckleIds != null && speckleIds.Count > 0)
                    if (!speckleIds.Any(id => id == response.Resources[i]._id)) // note: slow, o(n²)
                        continue;

                IBHoMObject iBHoMObject = resource as IBHoMObject;
                if (iBHoMObject != null)
                {
                    if (assignSpeckleIdToBHoMObjects)
                        iBHoMObject.CustomData[AdapterId] = response.Resources[i]._id;

                    bHoMObjects.Add(iBHoMObject);
                    continue;
                }
                var iObject = resource as IObject;

                if (iObject != null)
                {
                    iObjects.Add(iObject);
                    continue;
                }

                reminder.Add(resource);
            }

            return true;
        }


        /// <summary>
        /// Extension method to convert bhom meshes to speckle meshes. 
        /// Will get called automatically in the speckle "Deserialise" method.
        /// https://github.com/speckleworks/SpeckleCore/blob/9545e96f04d85f46203a99c21c76eeea0ea03dae/SpeckleCore/Conversion/ConverterDeserialisation.cs#L64
        /// </summary>
        //public static BH.oM.Geometry.Mesh ToNative (this Specklemesh speckleMesh)
        //{
        //    BH.oM.Geometry.Mesh bhomMesh = new BH.oM.Geometry.Mesh();

        //    // Conversion stuff

        //    return bhomMesh;
        //}

        /// <summary>
        /// Convert Speckle Point -> BHoM Point
        /// </summary>
        /// <returns>BHoM Point</returns>
        public static BHG.Point ToBHoM(this SCG.SpecklePoint specklePoint)
        {
            return new BHG.Point { X = specklePoint.Value[0], Y = specklePoint.Value[1], Z = specklePoint.Value[2] };
        }

        /// <summary>
        /// Convert Speckle Vector -> BHoM Vector
        /// </summary>
        /// <returns>BHoM Vector</returns>
        public static BHG.Vector ToBHoM(this SCG.SpeckleVector speckleVector)
        {
            return new BHG.Vector { X = speckleVector.Value[0], Y = speckleVector.Value[1], Z = speckleVector.Value[2] };
        }

        /// <summary>
        /// Convert Speckle Line -> BHoM Line
        /// </summary>
        /// <returns>BHoM Line</returns>
        public static BHG.Line ToBHoM(this SCG.SpeckleLine speckleLine)
        {
            List<BHG.Point> points = speckleLine.Value.ToPoints();
            return new BHG.Line { Start = points[0], End = points[1] };
        }

        /// <summary>
        /// Convert Speckle Mesh -> BHoM Mesh
        /// </summary>
        /// <returns>BHoM Mesh</returns>
        public static BHG.Mesh ToBHoM(this SCG.SpeckleMesh speckleMesh)
        {
            List<BHG.Point> vertices = speckleMesh.Vertices.ToPoints();
            List<int> sfaces = speckleMesh.Faces;
            List<BHG.Face> faces = new List<BHG.Face>();
            for (int i = 0; i < sfaces.Count;)
            {
                if (sfaces[i] == 1) // triangle face
                {
                    faces.Add(new BHG.Face { A = sfaces[i + 1], B = sfaces[i + 2], C = sfaces[i + 3], D = -1 });
                    i = i + 4;
                }
                if (sfaces[i] == 0) // quad face
                {
                    faces.Add(new BHG.Face { A = sfaces[i + 1], B = sfaces[i + 2], C = sfaces[i + 3], D = sfaces[i + 4] });
                    i = i + 5;
                }
            }

            return new BHG.Mesh { Vertices = vertices, Faces = faces };
        }
    }
}
