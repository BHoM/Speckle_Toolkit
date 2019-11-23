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

        public static bool ToBHoM(ResponseObject response, out List<IBHoMObject> bHoMObjects, out List<IObject> iObjects, out List<object> reminder, bool assignSpeckleIdToBHoMObjects = true, List<string> speckleIds = null)
        {
            bHoMObjects = new List<IBHoMObject>();
            iObjects = new List<IObject>();
            reminder = new List<object>();
            


            for (int i = 0; i < response.Resources.Count; i++)
            {
                var resource = Converter.Deserialise(response.Resources[i]);

                if (speckleIds != null && speckleIds.Count > 0)
                    if (!speckleIds.Any(id => id == response.Resources[i]._id))
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
    }
}
