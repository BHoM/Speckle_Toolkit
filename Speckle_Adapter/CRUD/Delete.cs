using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Adapter.Speckle
{
  public partial class SpeckleAdapter
  {
    /***************************************************/
    /**** Adapter overload method                   ****/
    /***************************************************/

    protected override int Delete( Type type, IEnumerable<object> ids )
    {
      //Insert code here to enable deletion of specific types of objects with specific ids
      return 0;
    }

    /***************************************************/
  }
}
