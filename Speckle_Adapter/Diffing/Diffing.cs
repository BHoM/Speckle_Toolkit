using BH.oM.Base;
using BH.oM.DataStructure;
using BH.Adapter.Speckle.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections;
using SpeckleCore;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {

        private bool DiffingByBHoMGuid(List<IObject> objectsToBePushed, out List<IObject> objectsCreated)
        {
            // This part is actually useless -- written assuming IBHoMObjects had static GUID (instead they are re-instantiated at any modification).

            objectsCreated = new List<IObject>();
            ResponseObject response = SpeckleClient.StreamGetObjectsAsync(SpeckleClient.Stream.StreamId, "").Result;

            List<IBHoMObject> bhomObjectsInSpeckle = null;
            List<IObject> iObjectsInSpeckle = null;
            List<object> reminderObjectsInSpeckle = null;

            BH.Engine.Speckle.Convert.ResponseToBHoM(response, out bhomObjectsInSpeckle, out iObjectsInSpeckle, out reminderObjectsInSpeckle, false);

            VennDiagram<IBHoMObject> guidDiagram = Engine.DataStructure.Create.VennDiagram(objectsToBePushed.Where(o => o as IBHoMObject != null).Cast<IBHoMObject>(), bhomObjectsInSpeckle, new IBHoMGUIDComparer());

            List<IBHoMObject> newObjects = guidDiagram.OnlySet1.ToList(); // Not having a counterpart in the Speckle Server
            List<IBHoMObject> toBeDeleted = guidDiagram.OnlySet2.ToList(); // Objects in the Speckle server that do not exist anymore locally. Just not push them.
            List<IBHoMObject> toBeDiffedByProperty = guidDiagram.Intersection.Select(o => o.Item1).ToList(); // Objects that already exist on the Speckle Server, based on their BHoM GUID. 
            
            // // - Insert code here for toBeDiffedByProperty to use custom comparers to diff by property.

            // At the moment, not having any custom comparer, the toBeDiffedByProperty are all just re-created (like assuming they've all changed, even if they didn't).
            List<IObject> objectsToBeCreated = newObjects.Concat(toBeDiffedByProperty).Concat(objectsToBePushed.Where(o => o as IBHoMObject == null)).ToList();

            if (CreateAnyObject(objectsToBeCreated))
                objectsCreated = objectsToBeCreated;

            return true;
        }



    }
}
