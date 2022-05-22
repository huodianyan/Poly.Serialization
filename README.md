# Poly.Serialization
a lightweight Serializer and Deserializer for Unity and any C# (or .Net) project.

## Features
- Zero dependencies
- Minimal core (< 1000 lines)
- Lightweight and fast
- Strongly Typed and C# Code as schema, no needs to other IDL like .proto, .fbs...
- Adapted to all C# game engine

## Installation

## Overview

```csharp

public class TestSerializable : IPolySerializable
{
    public int IntValue;
    public string StringValue;

    public override bool Equals(object obj)
    {
        var other = obj as TestSerializable;
        if (other == null) return false;
        return IntValue == other.IntValue && StringValue == other.StringValue;
    }
    public override int GetHashCode()
    {
        return unchecked(IntValue + StringValue.GetHashCode());
    }
    public void Deserialize(ref PolyReader reader)
    {
        IntValue = reader.ReadPackedInt();
        StringValue = reader.ReadString();
    }
    public void Serialize(ref PolyWriter writer)
    {
        writer.WritePackedInt(IntValue);
        writer.WriteString(StringValue);
    }
}
[PolyFormattable]
public class TestFormattable
{
    [PolyIndex(1)]
    public string Value1 { get; set; }
    [PolyIndex(0)]
    public int Value0 { get; set; }
    //[PolyIndex(2)]
    //public IList<int> Value2 { get; set; }
    [PolyIndex(2)]
    public TestSerializable Value3 { get; set; }
}

...

var context = new PolySerializationContext();
var origin = new TestFormattable
{
    Value0 = 100,
    Value1 = "huo",
    //Value2 = new List<int>() { 1, 2, 3 },
    Value3 = new TestSerializable { IntValue = 111, StringValue = "dian" }
};
var data = new byte[64];
var writer = new PolyWriter(data, 0, 0, context);
writer.WriteObject(origin);

var segment = writer.DataSegment;

var reader = new PolyReader(segment);
var result = reader.ReadObject<TestFormattable>();

```

## License
The software is released under the terms of the [MIT license](./LICENSE.md).

## FAQ

## References

### Documents
- [BitConverter Class](https://docs.microsoft.com/zh-cn/dotnet/api/system.bitconverter?view=net-6.0)

### Projects
- [neuecc/ZeroFormatter](https://github.com/neuecc/ZeroFormatter)
- [grofit/LazyData](https://github.com/grofit/LazyData)

### Benchmarks
