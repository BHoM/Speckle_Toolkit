using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Adapter;
using BH.Engine.Speckle;
using BH.oM.Base;
using BH.oM.DataManipulation.Queries;
using SpeckleCore;

namespace BH.Adapter.Speckle
{
  public partial class SpeckleAdapter : BHoMAdapter
  {

    public SpeckleApiClient myClient;
    public Account myAccount;

    //Add any applicable constructors here, such as linking to a specific file or anything else as well as linking to that file through the (if existing) com link via the API
    public SpeckleAdapter( SpeckleCore.Account speckleAccount, string streamId )
    {
      AdapterId = BH.Engine.Speckle.Convert.AdapterId;   //Set the "AdapterId" to "SoftwareName_id". Generally stored as a constant string in the convert class in the SoftwareName_Engine

      Config.SeparateProperties = true;   //Set to true to push dependant properties of objects before the main objects are being pushed. Example: push nodes before pushing bars
      Config.MergeWithComparer = false;    //Set to true to use EqualityComparers to merge objects. Example: merge nodes in the same location
      Config.ProcessInMemory = false;     //Set to false to to update objects in the toolkit during the push
      Config.CloneBeforePush = false;      //Set to true to clone the objects before they are being pushed through the software. Required if any modifications at all, as adding a software ID is done to the objects
      Config.UseAdapterId = false;

      myAccount = speckleAccount;
      myClient = new SpeckleApiClient() { BaseUrl = myAccount.RestApi, AuthToken=myAccount.Token, Stream = new SpeckleStream() { StreamId = streamId } }; // hacky, but i don't want to rebuild stuff and fiddle dll loading etc.

      myClient.SetupWebsocket();
    }

    // Super naive implementation to get the ball rolling - in theory, this should implement the orchestration methods from gh/dyn senders (which actually should be moved in the core)
    public override List<IObject> Push( IEnumerable<IObject> objects, string tag = "", Dictionary<string, object> config = null )
    {

      var converted = SpeckleCore.Converter.Serialise( objects );
      var defaultLayer = new Layer() { Name = "Default Layer", OrderIndex = 0, StartIndex = 0, ObjectCount = converted.Count, Topology = "", Guid = "c8a58593-7080-450b-96b9-b0158844644b" };
      var myStream = new SpeckleStream() { Objects = converted, Layers = new List<Layer>() { defaultLayer } };

      var cloneResponse = myClient.StreamCloneAsync( myClient.Stream.StreamId ).Result;
      var updateResponse = myClient.StreamUpdateAsync( myClient.Stream.StreamId, myStream ).Result;
      myClient.BroadcastMessage( "stream", myClient.Stream.StreamId, new { eventType = "update-global" } );
      return objects.ToList();
    }

    // Again, super naive implementation but does what it says on the tin
    public override IEnumerable<object> Pull( IQuery query, Dictionary<string, object> config = null )
    {
      var response = myClient.StreamGetObjectsAsync( myClient.Stream.StreamId, "" ).Result;
      return Converter.Deserialise( response.Resources );
    }

    protected override bool Create<T>( IEnumerable<T> objects, bool replaceAll = false )
    {
      throw new NotImplementedException();
    }

    protected override IEnumerable<IBHoMObject> Read( Type type, IList ids )
    {
      throw new NotImplementedException();
    }

  }
}
