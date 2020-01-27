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

namespace BH.Engine.Speckle
{
    public static partial class Query
    {
        public static void DispatchByBaseInterface(IEnumerable<IObject> objects, out List<IBHoMObject> ibhomObjects, out List<IObject> iObjects)
        {
            ibhomObjects = new List<IBHoMObject>();
            iObjects = new List<IObject>();

            List<object> reminder;
            DispatchByBaseInterface(objects, out ibhomObjects, out iObjects, out reminder);
        }

        public static void DispatchByBaseInterface(IEnumerable<object> objects, out List<IBHoMObject> ibhomObjects, out List<IObject> iObjects, out List<object> reminder)
        {
            ibhomObjects = new List<IBHoMObject>();
            iObjects = new List<IObject>();
            reminder = new List<object>();

            Type iBHoMObjectType = typeof(IBHoMObject);
            Type iObjectType = typeof(IObject);

            MethodInfo miToList = typeof(Enumerable).GetMethod("Cast");

            foreach (var typeGroup in objects.GroupBy(x => x.GetType()))
            {
                MethodInfo miListObject = miToList.MakeGenericMethod(new[] { typeGroup.Key });

                var castedObjects = miListObject.Invoke(typeGroup, new object[] { typeGroup });

                if (iBHoMObjectType.IsAssignableFrom(typeGroup.Key))
                {
                    // They're iBHoMObjects
                    ibhomObjects.AddRange((IEnumerable<IBHoMObject>)castedObjects);
                }
                else if (iObjectType.IsAssignableFrom(typeGroup.Key))
                {
                    // They're iObjects
                    iObjects.AddRange((IEnumerable<IObject>)castedObjects);
                }
                else
                {
                    // They're something else
                    reminder.AddRange((IEnumerable<object>)castedObjects);
                }
            }
        }
    }
}
