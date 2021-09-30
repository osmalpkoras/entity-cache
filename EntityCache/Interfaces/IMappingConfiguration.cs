using System;
using System.Collections.Generic;

namespace EntityCache.Interfaces
{
    // TODO: hier muss ich noch die typenparameter übergeben, damit ganz klar ist, für welchen IDataCache und DbContext die Konfiguration gilt
    // damit beim DataCache Konstruktor die Typen ebenfalls klar sind
    public interface IMappingConfiguration
    {
        List<ITypeMapping> TypeMappings { get; }

        List<IEntityCollectionMapping> EntityMappings { get; }
        IEntityCollectionMapping GetEntityMappingByCachedEntityType(Type type);

        /// <summary>
        ///     Use this method if you want to retrieve a type mapping for an entity, as EF creates subclasses of entity types at
        ///     runtime.
        /// </summary>
        /// <param name="propertyMapping"></param>
        /// <returns></returns>
        IEntityCollectionMapping GetEntityMappingForPropertyMapping(IPropertyMapping propertyMapping);

        ITypeMapping GetTypeMappingByCachedEntityType(Type type);
    }
}