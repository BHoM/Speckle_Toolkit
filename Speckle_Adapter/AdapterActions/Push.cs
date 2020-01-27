using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BH.oM.Adapter;
using BH.oM.Base;
using BH.oM.Data.Requests;
using SpeckleCore;
using BH.oM.Speckle;
using System.ComponentModel;
using BH.Engine.Base;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter
    {
        public override List<object> Push(IEnumerable<object> objects, string tag = "", PushType pushType = PushType.AdapterDefault, ActionConfig actionConfig = null)
        {
            // Clone objects for immutability in the UI
            List<object> objectsToPush = objects.Select(x => x.DeepClone()).ToList();

            // Initialize Speckle Layer if not existing
            if (SpeckleLayer == null)
            {
                SpeckleLayer = new Layer() { Name = "Default Layer", OrderIndex = 0, StartIndex = 0, Topology = "", Guid = "c8a58593-7080-450b-96b9-b0158844644b" };
                SpeckleLayer.ObjectCount = 0;

            }

            // Initialize the SpeckleStream
            SpeckleStream.Layers = new List<Layer>() { SpeckleLayer };
            SpeckleStream.Objects = new List<SpeckleObject>(); // --> shit's immutable, yo


            /// -------------------
            /// Base push rewritten
            /// -------------------

            // //- Read config
            SpeckleActionConfig config = actionConfig as SpeckleActionConfig;
            if (config == null)
                BH.Engine.Reflection.Compute.RecordError($"The specified ActionConfig is not compatible with {this.GetType().Name}.");
            

            // //- Use "Speckle" history: produces a new stream at every push that corresponds to the old version. Enabled by default.
            if (config.EnableHistory)
                SetupHistory();

            bool success = true;
            List<IBHoMObject> bHoMObjects = null;
            List<IObject> iobjects = null;
            List<object> reminder = null;

            if (config.UseGUIDS)
            {
                // This part works using IBHoMObjects GUID.
                Engine.Speckle.Query.DispatchBHoMObjects(objectsToPush, out bHoMObjects, out iobjects, out reminder);

                List<object> objectsCreated = null;
                success = CreateUsingDiffingByBHoMGuid(objectsToPush, out objectsCreated);

                objectsToPush = objectsCreated.Count > 0 ? objectsCreated.Cast<object>().ToList() : objectsToPush;
            }
            else
            {
                // // - Base push rewritten to allow some additional CustomData to go in.
                PushByType(objectsToPush, tag, config.SetAssignedId, ref success);
            }

            return success ? objectsToPush : new List<object>();
        }


        /***************************************************/
        /**** Private Helper Methods                    ****/
        /***************************************************/

        private bool PushByType(List<object> objectsToPush, string tag, bool setAssignedId, ref bool success)
        {
            // // - Base push rewritten to allow some additional CustomData to go in.
            Type iBHoMObjectType = typeof(IBHoMObject);
            MethodInfo miToList = typeof(Enumerable).GetMethod("Cast");
            foreach (var typeGroup in objectsToPush.GroupBy(x => x.GetType()))
            {
                MethodInfo miListObject = miToList.MakeGenericMethod(new[] { typeGroup.Key });

                var list = miListObject.Invoke(typeGroup, new object[] { typeGroup });

                if (iBHoMObjectType.IsAssignableFrom(typeGroup.Key))
                {
                    // They are IBHoMObjects

                    /// Assign SpeckleStreamId to the CustomData of the IBHoMObjects
                    var iBHoMObjects = list as IEnumerable<IBHoMObject>;
                    iBHoMObjects.ToList().ForEach(o => o.CustomData["Speckle_StreamId"] = SpeckleStreamId);

                    /// Switch push type
                    success &= CreateIBHoMObjects(iBHoMObjects as dynamic, setAssignedId);//Replace(iBHoMObjects as dynamic, tag);
                }
                else
                {
                    // They are IObjects
                    success &= CreateIObjects(list as dynamic);
                }
            }
            return success;
        }

        [Description("Creates a new stream (with a different StreamId), where the current stream content is copied, before it gets modified.")]
        private void SetupHistory()
        {
            ResponseStreamClone response = null;
            Task<ResponseStreamClone> respStreamClTask = null;

            // The following line creates a new stream (with a different StreamId), where the current stream content is copied, before it gets modified.
            // The streamId of the cloned "backup" is saved among the main Stream "children" field,
            // accessible through SpeckleServerAddress/api/v1/streams/streamId
            respStreamClTask = SpeckleClient.StreamCloneAsync(SpeckleStreamId);

            try
            {
                response = respStreamClTask?.Result;
            }
            catch (Exception e) { }

            if (response == null)
                BH.Engine.Reflection.Compute.RecordWarning($"Failed configuring Speckle History. Task status: {respStreamClTask.Status.ToString()}");
        }
    }
}
