using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace OoBDev.MongoDB.Extensions;

/// <summary>
/// Represents a convention for configuring BSON serialization of object IDs.
/// </summary>
public class BsonObjectIdConvention : ConventionBase, IMemberMapConvention
{
    // public methods

    /// <summary>
    /// Applies a modification to the member map.
    /// </summary>
    /// <param name="memberMap">The member map.</param>
    public void Apply(BsonMemberMap memberMap)
    {
        var type = memberMap.ClassMap.ClassType;
        var idConvention =
            (
            type.Name.EndsWith("Model") ? type.Name[..^5] :
            type.Name.EndsWith("Collection") ? type.Name[..^10] :
            type.Name
            ) + "Id";

        if (string.Equals(memberMap.ElementName, "_id", StringComparison.InvariantCultureIgnoreCase) ||
            string.Equals(memberMap.ElementName, idConvention, StringComparison.InvariantCultureIgnoreCase) ||
            memberMap.MemberInfo is PropertyInfo prop && prop.GetCustomAttribute<KeyAttribute>() != null)
        {
            new BsonIdAttribute().Apply(memberMap);
            new BsonRepresentationAttribute(BsonType.ObjectId).Apply(memberMap);

            //Note: might want this ... new BsonIgnoreIfDefaultAttribute(true).Apply(memberMap); or have a way for this to be injectable
        }
    }
}
