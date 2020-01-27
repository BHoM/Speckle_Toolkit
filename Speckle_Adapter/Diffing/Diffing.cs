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

using BH.oM.Base;
using BH.oM.Data;
using BH.Adapter.Speckle.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections;
using SpeckleCore;
using BH.oM.Data.Collections;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        /// This method is actually useless -- 
        /// written assuming IBHoMObjects had static GUID (instead they are re-instantiated at any modification).
        private bool DiffingByBHoMGuid(List<IObject> objectsToBePushed, out List<IObject> objectsCreated)
        {
            objectsCreated = new List<IObject>();
            List<IObject> objectsToBeCreated = new List<IObject>();

            // Dispatch objects to be pushed
            List <IBHoMObject> bhomObjectsToBePushed = null;
            List<IObject> iObjectsToBePushed = null;

            BH.Engine.Speckle.Query.DispatchByBaseInterface(objectsToBePushed, out bhomObjectsToBePushed, out iObjectsToBePushed);

            // Receive and dispatch objects already in speckle
            ResponseObject response = SpeckleClient.StreamGetObjectsAsync(SpeckleStreamId, "").Result;

            List<IBHoMObject> bhomObjectsInSpeckle = null;
            List<IObject> iObjectsInSpeckle = null;
            List<object> reminderObjectsInSpeckle = null;

            BH.Engine.Speckle.Convert.ToBHoM(response.Resources, out bhomObjectsInSpeckle, out iObjectsInSpeckle, out reminderObjectsInSpeckle, false);

            // If speckle doesn't contain anything, push everything
            if (response.Resources.Count == 0)
            {
                objectsToBeCreated = bhomObjectsToBePushed.Concat(iObjectsToBePushed).ToList();
                if (CreateIObjects(objectsToBeCreated))
                    objectsCreated = objectsToBeCreated;

                return true;
            }

            // Diffing for IBHoMObjects
            VennDiagram<IBHoMObject> guidDiagram = Engine.Data.Create.VennDiagram(objectsToBePushed.Where(o => o as IBHoMObject != null).Cast<IBHoMObject>(), bhomObjectsInSpeckle, new IBHoMGUIDComparer());

            List<IBHoMObject> newObjects = guidDiagram.OnlySet1.ToList(); // Not having a counterpart in the Speckle Server
            List<IBHoMObject> toBeDeleted = guidDiagram.OnlySet2.ToList(); // Objects in the Speckle server that do not exist anymore locally. Just not push them.
            List<IBHoMObject> toBeDiffedByProperty = guidDiagram.Intersection.Select(o => o.Item1).ToList(); // Objects that already exist on the Speckle Server, based on their BHoM GUID. 


            // // - Insert code here for toBeDiffedByProperty to use custom comparers to diff by property.

            // At the moment, not having any custom comparer, the toBeDiffedByProperty are all just re-created (like assuming they've all changed, even if they didn't).
            objectsToBeCreated = newObjects.Concat(toBeDiffedByProperty).Concat(objectsToBePushed.Where(o => o as IBHoMObject == null)).ToList();

            if (objectsToBeCreated.Count == 0)
            {
                BH.Engine.Reflection.Compute.RecordNote("Speckle already contains every BHoM currently being pushed. They have not been pushed.");
            }

            // IObjects always have to be recreated as they have no GUID
            List<int> objectsToPushHashes = new List<int>();
            objectsToPushHashes = objectsToBePushed.Select(o => o.ToString().GetHashCode()).ToList();

            foreach (var o in iObjectsToBePushed) //.Concat(reminderObjectsInSpeckle)
            {
                if (!objectsToPushHashes.Any(hash => hash == o.ToString().GetHashCode()))
                    objectsToBeCreated.Add(o);
            }


            if (CreateIObjects(objectsToBeCreated))
                objectsCreated = objectsToBeCreated;

            return true;
        }



    }
}
