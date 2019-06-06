﻿using System;
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

    //Method being called for any object already existing in the model in terms of comparers is found.
    //Default implementation first deletes these objects, then creates new ones, if not applicable for the software, override this method

    protected override bool UpdateObjects<T>( IEnumerable<T> objects )
    {
      return base.UpdateObjects<T>( objects );
    }

    /***************************************************/
  }
}
