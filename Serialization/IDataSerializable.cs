using System;

namespace Poly.Serialization
{
    public interface IDataSerializable
    {
        void Serialize(ref DataWriter writer);
        void Deserialize(ref DataReader reader);
    }
}