using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Poly.Serialization.Tests
{
    [TestClass]
    public partial class SerializationTest
    {
        private static PolySerializationContext context;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            context = new PolySerializationContext();
        }
        [ClassCleanup]
        public static void ClassCleanup()
        {
        }
        [TestInitialize]
        public void TestInitialize()
        {
        }
        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void SerializeTest()
        {
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

            Assert.AreEqual(origin.Value0, result.Value0);
            Assert.AreEqual(origin.Value1, result.Value1);
            //Assert.AreEqual(origin.Value2.Count, result.Value2.Count);
            Assert.AreEqual(origin.Value3, result.Value3);
        }
    }

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
}