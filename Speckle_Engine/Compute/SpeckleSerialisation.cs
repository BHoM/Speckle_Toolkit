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
using BH.oM.Speckle;
using System.Collections;

namespace BH.Engine.Speckle
{
    public static partial class Compute
    {
        [Description("Attempts to compute a SpeckleObject representation of the BHoMObject, so it can be visualised in the SpeckleViewer.")]
        public static SpeckleObject SpeckleSerialisation(this IObject bhomObject)
        {
            return Serialise(bhomObject);
        }

        private static SpeckleAbstract Serialise(object source, int recursionDepth = 0, string path = "")
        {
            SpeckleAbstract result = new SpeckleAbstract();
            result._type = source.GetType().Name;
            result._assembly = source.GetType().Assembly.FullName;

            Dictionary<string, object> dict = new Dictionary<string, object>();

            var properties = source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var prop in properties)
            {
                if (!prop.CanWrite)
                    continue;

                try
                {
                    var value = prop.GetValue(source);

                    if (value == null)
                        continue;

                    dict[prop.Name] = WriteValue(value, recursionDepth, path + "/" + prop.Name);
                }
                catch { }
            }

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