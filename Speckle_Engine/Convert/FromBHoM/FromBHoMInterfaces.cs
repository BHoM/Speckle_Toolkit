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

namespace BH.Engine.Speckle
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Wraps the content of a BHoMObject into a SpeckleObject.\n" +
            "Its geometry (if any) is exposed in the top level, so it can be visualised in Speckle.\n" +
            "All BHoMObject properties are json-serialised and saved into the `speckleObject.Properties` field.")]
        public static SpeckleObject FromBHoM(this IBHoMObject bhomObject)
        {
            // Retrieve the BHoM geometry to represent the object in SpeckleViewer.
            IGeometry geom = bhomObject.IGeometry();

            if (geom == null)
            {
                // BHoMObject does not have a geometrical representation in BHoM.
                // It must be converted to an "Abstract" SpeckleObject.
                return (SpeckleObject)SpeckleCore.Converter.Serialise(bhomObject);
            }

            // Creates the SpeckleObject with the BHoM Geometry, dynamically dispatching to the method for the right type.
            SpeckleObject speckleObject = FromBHoM(geom as dynamic); // This will be our "wrapper" object for the rest of the BHoM stuff.

            // Serialise the BHoMobject into a Json and append it to the Properties Dictionary of the SpeckleObject. Key is "BHoMData".
            speckleObject.Properties = new Dictionary<string, object>() { { "BHoMData", BH.Engine.Serialiser.Convert.ToJson(bhomObject) } };

            return speckleObject;
        }

        [Description("Wraps the content of a BHoM IGeometry into a SpeckleObject.\n" +
           "A Rhino geometry representation is extracted and exposed in the top level, so it can be visualised in Speckle.\n" +
           "All the other IGeometry properties are json-serialised and saved into the `speckleObject.Properties` field.")]
        public static SpeckleObject FromBHoM(this IGeometry iGeometry)
        {
            var rhinoGeom = Engine.Rhinoceros.Convert.IToRhino(iGeometry);

            // Creates the SpeckleObject with the Rhino Geometry. 
            var speckleObj_rhinoGeom = (SpeckleObject)SpeckleCore.Converter.Serialise(rhinoGeom); // This will be our "wrapper" object for the rest of the IObject stuff.

            // Serialise the iGeometry into a Json and append it to the additional properties of the SpeckleObject.
            BH.Engine.Serialiser.Convert.ToJson(iGeometry);
            speckleObj_rhinoGeom.Properties = new Dictionary<string, object>() { { iGeometry.GetType().Name, BH.Engine.Serialiser.Convert.ToJson(iGeometry) } };

            return speckleObj_rhinoGeom;
        }
    }
}
