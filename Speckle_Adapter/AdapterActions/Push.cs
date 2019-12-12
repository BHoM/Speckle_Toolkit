using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Geometry;
using SpeckleCore;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter : BHoMAdapter
    {
        public override List<IObject> Push(IEnumerable<IObject> objects, string tag = "", Dictionary<string, object> config = null)
        {
            // Initialize Speckle Layer if not existing
            if (SpeckleLayer == null)
            {
                SpeckleLayer = new Layer() { Name = "Default Layer", OrderIndex = 0, StartIndex = 0, Topology = "", Guid = "c8a58593-7080-450b-96b9-b0158844644b" };
                SpeckleLayer.ObjectCount = 0;
            }

            // Initialize the SpeckleStream
            SpeckleStream.Layers = new List<Layer>() { SpeckleLayer };
            SpeckleStream.Objects = new List<SpeckleObject>(); // --> shit's immutable, yo

            // //- Read config
            object configObj;

            string pushType = "Replace";
            if (config != null && config.TryGetValue("PushType", out configObj))
                pushType = configObj.ToString();

            bool useGUIDS = false;
            if (config != null && config.TryGetValue("UseGUIDS", out configObj))
                if (configObj is bool)
                    useGUIDS = (bool)configObj;

            bool setAssignedId = true;
            if (config != null && config.TryGetValue("SetAssignedId", out configObj))
                if (configObj is bool)
                    setAssignedId = (bool)configObj;

            // // - Configure history. By default it's enabled, and it produces a new stream at every push that correspond to versions.
            configureHistory(config);

            // // - Clone objects
            List<IObject> objectsToPush = Config.CloneBeforePush ? objects.Select(x => x is BHoMObject ? ((BHoMObject)x).GetShallowClone() : x).ToList() : objects.ToList(); //ToList() necessary for the return collection to function properly for cloned objects

            // // - Call the appropriate push method.
            bool success = true;
            List<IBHoMObject> bHoMObjects = new List<IBHoMObject>();
            List<IObject> iobjects = new List<IObject>();

            if (useGUIDS)
            {
                // This part works using IBHoMObjects GUID.
                Engine.Speckle.Query.DispatchByBaseInterface(objectsToPush, out bHoMObjects, out iobjects);

                List<IObject> objectsCreated = null;
                success = DiffingByBHoMGuid(objectsToPush, out objectsCreated);

                objectsToPush = objectsCreated.Count > 0 ? objectsCreated : objectsToPush;
            }
            else
            {
                // // - Base push rewritten to allow some additional CustomData to go in.
                BasePushRewritten(objectsToPush, pushType, tag, setAssignedId, ref success);
            }

            return success ? objectsToPush : new List<IObject>();
        }
    }
}
