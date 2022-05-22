using System;

namespace Poly.Serialization
{
    public interface IPolySerializable
    {
        void Serialize(ref PolyWriter writer);
        void Deserialize(ref PolyReader reader);
    }
}