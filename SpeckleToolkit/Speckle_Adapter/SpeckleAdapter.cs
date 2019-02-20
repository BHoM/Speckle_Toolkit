using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Adapter;
using BH.Engine.Speckle;
using BH.oM.Base;
using SpeckleCore;

namespace BH.Adapter.Speckle
{
  public partial class SpeckleAdapter : BHoMAdapter
  {

    public SpeckleApiClient myClient;
    public Account myAccount;

    /***************************************************/
    /**** Constructors                              ****/
    /***************************************************/

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

      myClient = new SpeckleApiClient(myAccount.RestApi, false);


    }

    protected override bool Create<T>( IEnumerable<T> objects, bool replaceAll = false )
    {

      var x = objects;

      throw new NotImplementedException();
    }

    protected override IEnumerable<IBHoMObject> Read( Type type, IList ids )
    {
      throw new NotImplementedException();
    }



    /***************************************************/
    /**** Private  Fields                           ****/
    /***************************************************/

    //Add any comlink object as a private field here, example named:

    //private SoftwareComLink m_softwareNameCom;


    /***************************************************/


  }
}
