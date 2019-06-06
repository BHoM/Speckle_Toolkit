using System;
using System.Collections;
using System.Collections.Generic;
using BH.oM.Base;

namespace BH.Adapter.Speckle
{
  public partial class SpeckleAdapter
  {

    /***************************************************/
    /**** BHoM Adapter Interface                    ****/
    /***************************************************/

    //Standard implementation for dependency types (change the dictionary below to override):

    protected override List<Type> DependencyTypes<T>( )
    {
      //Type type = typeof( T );

      //if ( m_DependencyTypes.ContainsKey( type ) )
      //  return m_DependencyTypes[ type ];

      //else if ( type.BaseType != null && m_DependencyTypes.ContainsKey( type.BaseType ) )
      //  return m_DependencyTypes[ type.BaseType ];

      //else
      //{
      //  foreach ( Type interType in type.GetInterfaces() )
      //  {
      //    if ( m_DependencyTypes.ContainsKey( interType ) )
      //      return m_DependencyTypes[ interType ];
      //  }
      //}


      return new List<Type>();
    }

    /***************************************************/
    /**** Private Fields                            ****/
    /***************************************************/

    //private static Dictionary<Type, List<Type>> m_DependencyTypes = new Dictionary<Type, List<Type>>
    //    {
    //        {typeof(Bar), new List<Type> { typeof(ISectionProperty), typeof(Node) } },
    //        {typeof(ISectionProperty), new List<Type> { typeof(Material) } },
    //        {typeof(RigidLink), new List<Type> { typeof(LinkConstraint), typeof(Node) } },
    //        {typeof(MeshFace), new List<Type> { typeof(Property2D), typeof(Node) } },
    //        {typeof(Property2D), new List<Type> { typeof(Material) } },
    //        {typeof(PanelPlanar), new List<Type> { typeof(Property2D) } }
    //    };


    /***************************************************/
  }
}
