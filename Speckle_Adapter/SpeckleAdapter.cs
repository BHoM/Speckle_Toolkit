using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Data.Requests;
using SpeckleCore;

namespace BH.Adapter.Speckle
{
    public partial class SpeckleAdapter : BHoMAdapter
    {
        public SpeckleAdapter(SpeckleCore.Account speckleAccount, string speckleStreamId)
        {
            Config.SeparateProperties = true;   //Set to true to push dependant properties of objects before the main objects are being pushed. Example: push nodes before pushing bars
            Config.MergeWithComparer = false;    //Set to true to use EqualityComparers to merge objects. Example: merge nodes in the same location
            Config.ProcessInMemory = false;     //Set to false to to update objects in the toolkit during the push
            Config.CloneBeforePush = true;      //Set to true to clone the objects before they are being pushed through the software. Required if any modifications at all, as adding a software ID is done to the objects
            Config.UseAdapterId = false;

            AdapterId = BH.Engine.Speckle.Convert.AdapterId;

            SpeckleAccount = speckleAccount;
            SpeckleStream = new SpeckleStream() { StreamId = SpeckleStreamId };

            SpeckleClient = new SpeckleApiClient() { BaseUrl = SpeckleAccount.RestApi, AuthToken = SpeckleAccount.Token, Stream = SpeckleStream }; // hacky, but i don't want to rebuild stuff and fiddle dll loading etc.
            SpeckleClient.SetupWebsocket();


            //if (string.IsNullOrWhiteSpace(speckleStreamId))
            SpeckleStreamId = speckleStreamId;
        }


        public override List<IObject> Push(IEnumerable<IObject> objects, string tag = "", Dictionary<string, object> config = null)
        {
            //var gna = SpeckleClient.StreamsGetAllAsync("").Result;

            //gna.Resources[0].StreamId


            objects = objects.ToList(); // to avoid modifying the original objects
            List<IObject> IBHoMObjectsToPush = new List<IObject>(); //has to be IObject even if it's going to collect IBHoMObjects
            List<IObject> IObjectsToPush = new List<IObject>();

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

            // //- Configure history. By default it's enabled, and it produces a new stream at every push that correspond to versions.
            configureHistory(config);

            // //- Clone objects
            List<IObject> objectsToPush = Config.CloneBeforePush ? objects.Select(x => x is BHoMObject ? ((BHoMObject)x).GetShallowClone() : x).ToList() : objects.ToList(); //ToList() necessary for the return collection to function properly for cloned objects

            bool success = true;
            List<IBHoMObject> bHoMObjects = new List<IBHoMObject>();
            List<IObject> iobjects = new List<IObject>();

            if (useGUIDS)
            {
                // This part works using IBHoMObjects GUID.
                Engine.Speckle.Query.DispatchBHoMObjects(objectsToPush, out bHoMObjects, out iobjects);

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


        public override IEnumerable<object> Pull(IRequest query, Dictionary<string, object> config = null)
        {
            var response = SpeckleClient.StreamGetObjectsAsync(SpeckleStreamId, "").Result;

            List<IBHoMObject> bHoMObjects = new List<IBHoMObject>();
            List<IObject> iObjects = new List<IObject>();
            List<object> reminder = new List<object>();

            bool assignSpeckleIdToBHoMObjects = true;

            if (query == null)
            {

                if (!BH.Engine.Speckle.Convert.ToBHoM(response, out bHoMObjects, out iObjects, out reminder, assignSpeckleIdToBHoMObjects))
                    BH.Engine.Reflection.Compute.RecordError("Failed to deserialize and cast the Server response into BHoM objects.");

                return bHoMObjects.Concat(iObjects).Concat(reminder);
            }
            else
            {
                /// -------------------
                /// Base Pull rewritten
                /// -------------------

                // Make sure this is a FilterQuery
                FilterRequest filter = query as FilterRequest;
                if (filter == null)
                {
                    Engine.Reflection.Compute.RecordWarning("Please specify a FilterQuery");
                    return new List<object>();
                }

                List<string> speckleIds = QueryToSpeckleIds(filter);

                // Read the IBHoMObjects
                BH.Engine.Speckle.Convert.ToBHoM(response, out bHoMObjects, out iObjects, out reminder, assignSpeckleIdToBHoMObjects, speckleIds);

                // Filter by tag if any 
                bHoMObjects = filter.Tag == "" ? bHoMObjects : bHoMObjects.Where(x => x.Tags.Contains(filter.Tag)).ToList();

                // Return stuff
                if (typeof(IBHoMObject).IsAssignableFrom(filter.Type))
                    return bHoMObjects;
                else if (typeof(IObject).IsAssignableFrom(filter.Type))
                    return iObjects;
                else
                    return bHoMObjects.Concat(iObjects).Concat(reminder);

            }

            return new List<object>();
        }

        /***************************************************/
        /**** Private Helper Methods                    ****/
        /***************************************************/

        private void BasePushRewritten(List<IObject> objectsToPush, string pushType, string tag, bool setAssignedId, ref bool success)
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
                    CreateIObjects(list as dynamic);
                }
            }
        }

        private void configureHistory(Dictionary<string, object> config = null)
        {
            ResponseStreamClone response = null;
            Task<ResponseStreamClone> respStreamClTask = null;

            object enableHistoryObj = null;

            if (config != null)
                config.TryGetValue("EnableHistory", out enableHistoryObj);

            bool? enableHistory = enableHistoryObj as bool?;

            if (!(bool)enableHistory)
                return;

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
                BH.Engine.Reflection.Compute.RecordWarning($"Could not set the EnableHistory option. Task status: {respStreamClTask.Status.ToString()}");
        }



        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/
        public SpeckleApiClient SpeckleClient { get; private set; }
        public string SpeckleStreamId { get; private set; }
        public Account SpeckleAccount { get; private set; }
        public SpeckleStream SpeckleStream { get; private set; }
        public SpeckleCore.Layer SpeckleLayer { get; private set; }

    }
}
