// <copyright file="JsonSerializerTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Emu.Models.Notices;
    using Emu.Serialization;
    using FluentAssertions;
    using LanguageExt;
    using Error = LanguageExt.Common.Error;

    public class JsonSerializerTests
    {
        [Fact]
        public void CanSerializeFins()
        {
            var sample = new Test { Prop = new AnotherTest() { A = 1, B = "hello" } };

            var serializer = new JsonSerializer();
            var result = serializer.Serialize(sample.AsArray());

            var expected = """
                [
                  {
                    "Prop": {
                      "State": true,
                      "Succ": {
                        "A": 1,
                        "B": "hello"
                      }
                    }
                  }
                ]
                """;

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CanSerializeFinErrors()
        {
            var sample = new Test { Prop = Error.New("some error") };

            var serializer = new JsonSerializer();
            var result = serializer.Serialize(sample.AsArray());

            var expected = """
                [
                  {
                    "Prop": {
                      "State": false,
                      "Fail": {
                        "Message": "some error",
                        "Code": 0
                      }
                    }
                  }
                ]
                """;

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CanDeserializeFins()
        {
            var json = """
                [
                  {
                    "Prop": {
                      "State": true,
                      "Succ": {
                        "A": 1,
                        "B": "hello"
                      }
                    }
                  }
                ]
                """;

            var expected = new[] { new Test { Prop = new AnotherTest() { A = 1, B = "hello" } } };

            var serializer = new JsonSerializer();
            var result = serializer.Deserialize<Test>(new StringReader(json));

            result.Should().HaveCount(1);
            var item = result.First();
            item.Should().NotBeNull();
            item.Prop.IsSucc.Should().BeTrue();
            item.Prop.ThrowIfFail().A.Should().Be(1);
            item.Prop.ThrowIfFail().B.Should().Be("hello");
        }

        [Fact]
        public void CanDeserializeFinErrors()
        {
            var json = """
                [
                  {
                    "Prop": {
                      "State": false,
                      "Fail": {
                        "Message": "Some error",
                        "Code": 0
                      }
                    }
                  }
                ]
                """;

            var expected = new[] { new Test { Prop = Error.New("some error") } };

            var serializer = new JsonSerializer();
            var result = serializer.Deserialize<Test>(new StringReader(json));

            result.Should().HaveCount(1);
            var item = result.First();
            item.Should().NotBeNull();
            item.Prop.IsSucc.Should().BeFalse();
            ((Error)item.Prop).Message.Should().Be("Some error");
            ((Error)item.Prop).Code.Should().Be(0);
        }

        public class AnotherTest
        {
            public int A { get; set; }

            public string B { get; set; }
        }

        public class Test
        {
            public Fin<AnotherTest> Prop { get; set; }
        }
    }
}
