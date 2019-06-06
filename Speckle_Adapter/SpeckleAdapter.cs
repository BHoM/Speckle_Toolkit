using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BH.oM.Base;
using BH.oM.DataManipulation.Queries;
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
            Config.CloneBeforePush = false;      //Set to true to clone the objects before they are being pushed through the software. Required if any modifications at all, as adding a software ID is done to the objects
            Config.UseAdapterId = false;

            AdapterId = BH.Engine.Speckle.Convert.AdapterId;
            SpeckleStreamId = speckleStreamId;
            SpeckleAccount = speckleAccount;
            SpeckleStreamParent = new SpeckleStream() { StreamId = SpeckleStreamId };

            SpeckleClient = new SpeckleApiClient() { BaseUrl = SpeckleAccount.RestApi, AuthToken = SpeckleAccount.Token, Stream = SpeckleStreamParent }; // hacky, but i don't want to rebuild stuff and fiddle dll loading etc.
            SpeckleClient.SetupWebsocket();
        }


        public override List<IObject> Push(IEnumerable<IObject> objects, string tag = "", Dictionary<string, object> config = null)
        {
            List<IObject> IBHoMObjectsToPush = new List<IObject>(); //has to be IObject even if it's going to collect IBHoMObjects
            List<IObject> IObjectsToPush = new List<IObject>();

            // Initialize Speckle Layer if not existing
            if (SpeckleLayerDefault == null)
            {
                SpeckleLayerDefault = new Layer() { Name = "Default Layer", OrderIndex = 0, StartIndex = 0, Topology = "", Guid = "c8a58593-7080-450b-96b9-b0158844644b" };
                SpeckleLayerDefault.ObjectCount = 0;

            }

            // Unless another DefaultStream is specified, use the ParentStream created in the Adapter Constructor
            if (SpeckleStreamDefault == null)
                SpeckleStreamDefault = SpeckleStreamParent;

            // Initialize the DefaultStream
            SpeckleStreamDefault.Layers = new List<Layer>() { SpeckleLayerDefault };
            SpeckleStreamDefault.Objects = new List<SpeckleObject>(); // --> shit's immutable, yo

            // Parse objects to assign additional properties to them
            foreach (var o in objects)
            {
                dynamic bhomObject = o as IBHoMObject;
                if (bhomObject != null)
                {
                    // Assign SpeckleStreamId to the CustomData of the IBHoMObjects
                    bhomObject.CustomData["Speckle_StreamId"] = SpeckleStreamId;
                    IBHoMObjectsToPush.Add(bhomObject);
                }
                else
                {
                    dynamic IObject = o as IObject;
                    if (IObject != null)
                        IObjectsToPush.Add(o);
                }
            }

            // Check the config for options
            // Configure history. By default it's enabled, and it produces "child" streams at every push that correspond to versions.
            configureHistory(config);

            base.Push(IBHoMObjectsToPush, tag, config);
            Create(IObjectsToPush as dynamic);

            return IObjectsToPush.Concat(IBHoMObjectsToPush).ToList();
        }


        public override IEnumerable<object> Pull(IQuery query, Dictionary<string, object> config = null)
        {
            var response = SpeckleClient.StreamGetObjectsAsync(SpeckleClient.Stream.StreamId, "").Result;
            return Converter.Deserialise(response.Resources);
        }

        /***************************************************/
        /**** Private Helper Methods                    ****/
        /***************************************************/
        private void configureHistory(Dictionary<string, object> config = null)
        {
            ResponseStreamClone response;

            object enableHistoryObj = null;

            if (config != null)
                config.TryGetValue("EnableHistory", out enableHistoryObj);

            bool? enableHistory = enableHistoryObj as bool?;

            if (enableHistory != null && enableHistory == true)
                response = SpeckleClient.StreamCloneAsync(SpeckleClient.Stream.StreamId).Result; //this line enables history
        }


        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/
        public SpeckleApiClient SpeckleClient { get; private set; }
        public string SpeckleStreamId { get; private set; }
        public Account SpeckleAccount { get; private set; }
        public SpeckleStream SpeckleStreamParent { get; private set; }
        public SpeckleStream SpeckleStreamDefault { get; private set; }
        public SpeckleCore.Layer SpeckleLayerDefault { get; private set; }

    }
}
