/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Base;
using System.Reflection;
using BH.oM.Geometry;
using BH.Engine.Base;
using System.ComponentModel;
using BH.oM.Speckle;
using System.Collections;


namespace BH.Engine.Speckle
{
    public static partial class Compute
    {
        [Description("Creates a SpeckleAbstract object that is used in the SpeckleViewer to visualise, expand, filter and group the BHoM objects by their properties." +
            "This format is understood by SpeckleViewer and allows for query/grouping in the online interface.")]
        public static SpeckleAbstract SpeckleAbstract(this IObject bhomObject)
        {
            SpeckleAbstract speckleAbstract = Serialise(bhomObject);

            return speckleAbstract;
        }

        // Speckle Serialise method reduced to the "essential" to suit our needs.
        private static SpeckleAbstract Serialise(object source, int recursionDepth = 0, string path = "")
        {
            SpeckleAbstract result = new SpeckleAbstract();
            Type sourceType = source.GetType();

            // REMOVE SOME SPECKLE PROPERTIES TO REDUCE BLOATING
            // We remove the `Type` property to reduce bloating in the Viewer. 
            // Consider that this will cause Speckle "deserialisation" to fail, if attempted.
            // (We don't ever deserialise this SpeckleAbstract, as for that we additionally send the zipped JSON of the BHoMObject).
            //result.Type = sourceType.Namespace + "." + sourceType.Name; // will break Speckle deserialisation, if attempted

            // Also we do *not* serialise the following:
            result._type = sourceType.Namespace + "." + sourceType.Name;
            //result._assembly = source.GetType().Assembly.GetName().Name;

            Dictionary<string, object> dict = new Dictionary<string, object>();

            var properties = sourceType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var prop in properties)
            {
                if (!prop.CanWrite)
                    continue;

                try
                {
                    var value = prop.GetValue(source);

                    if (value == null)
                        continue;

                    // Property exclusions
                    if (prop.Name == nameof(IBHoMObject.BHoM_Guid))
                        continue;

                    dict[prop.Name] = WriteValue(value, recursionDepth, path + "/" + prop.Name);
                }
                catch { }
            }

            // // - We generally do not need to be sending also the fields of our BHoM Objects. Properties should be enough in all cases.
            // // - Leaving this here for future reference in case it's needed.
            //var fields = source.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            //foreach (var field in fields)
            //{
            //    if (field.IsNotSerialized)
            //        continue;
            //    try
            //    {
            //        var value = field.GetValue(source);
            //        if (value == null)
            //            continue;
            //        dict[field.Name] = WriteValue(value, recursionDepth, traversed, path + "/" + field.Name);
            //    }
            //    catch { }
            //}

            result.Properties = dict;

            // HASHING
            // We never use the SpeckleAbstract serialisation for anything except visualising the properties in the viewer.
            // Do not bloat the properties with the hash.
            result.GeometryHash = null; 
            result.Hash = null;
            // Original hash generation:
            //result.Hash = result.GeometryHash = result.GetMd5FromObject(result.GetMd5FromObject(result._assembly) + result.GetMd5FromObject(result._type) + result.GetMd5FromObject(result.Properties));

            return result;
        }

        private static object WriteValue(object myObject, int recursionDepth, string path = "")
        {
            if (myObject == null || recursionDepth > 8) return null;

            if (myObject is Enum) return System.Convert.ChangeType((Enum)myObject, ((Enum)myObject).GetTypeCode());

            if (myObject.GetType().IsPrimitive || myObject is string)
                return myObject;

            if (myObject is Guid)

                return myObject.ToString();


            if (myObject is IEnumerable && !(myObject is IDictionary))
            {
                var rlist = new List<object>(); int index = 0;

                foreach (var x in (IEnumerable)myObject)
                {
                    var obj = WriteValue(x, recursionDepth + 1, path + "/[" + index++ + "]");
                    if (obj != null)
                        rlist.Add(obj);
                }
                return rlist;
            }

            if (myObject is IDictionary)
            {
                var myDict = myObject as IDictionary;
                var returnDict = new Dictionary<string, object>();
                foreach (DictionaryEntry x in myDict)
                {
                    var y = x.Key;
                    returnDict.Add(x.Key.ToString(), WriteValue(x.Value, recursionDepth, path + "/{" + x.Key.ToString() + "}"));
                }
                return returnDict;
            }

            if (!myObject.GetType().AssemblyQualifiedName.Contains("System"))
                return Serialise(myObject, recursionDepth + 1, path);

            return null;
        }
    }
}
