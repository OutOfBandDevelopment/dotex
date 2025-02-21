using OoBDev.System.ComponentModel;
using OoBDev.System.Reflection;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OoBDev.System.Tests.Reflection;

[TestClass]
public class EnumExtensionsTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AsModelsTest()
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            //TODO: fix this better
            Assert.Inconclusive();
            return;
        }

        var values = EnumExtensions.AsModels<AttributeTargets>();

        foreach (var e in values)
            TestContext?.WriteLine(e.ToString());

        var result = values.Aggregate(new StringBuilder(), (sb, v) => sb.Append(v).AppendLine()).ToString();

        var expected = @"EnumModel { Id = 1, Name = Assembly, Code = ASSEMBLY, Description = , Order = 0, Value = Assembly, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = Assembly }
EnumModel { Id = 2, Name = Module, Code = MODULE, Description = , Order = 0, Value = Module, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = Module }
EnumModel { Id = 4, Name = Class, Code = CLASS, Description = , Order = 0, Value = Class, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = Class }
EnumModel { Id = 8, Name = Struct, Code = STRUCT, Description = , Order = 0, Value = Struct, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = Struct }
EnumModel { Id = 16, Name = Enum, Code = ENUM, Description = , Order = 0, Value = Enum, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = Enum }
EnumModel { Id = 32, Name = Constructor, Code = CONSTRUCTOR, Description = , Order = 0, Value = Constructor, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = Constructor }
EnumModel { Id = 64, Name = Method, Code = METHOD, Description = , Order = 0, Value = Method, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = Method }
EnumModel { Id = 128, Name = Property, Code = PROPERTY, Description = , Order = 0, Value = Property, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = Property }
EnumModel { Id = 256, Name = Field, Code = FIELD, Description = , Order = 0, Value = Field, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = Field }
EnumModel { Id = 512, Name = Event, Code = EVENT, Description = , Order = 0, Value = Event, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = Event }
EnumModel { Id = 1024, Name = Interface, Code = INTERFACE, Description = , Order = 0, Value = Interface, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = Interface }
EnumModel { Id = 2048, Name = Parameter, Code = PARAMETER, Description = , Order = 0, Value = Parameter, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = Parameter }
EnumModel { Id = 4096, Name = Delegate, Code = DELEGATE, Description = , Order = 0, Value = Delegate, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = Delegate }
EnumModel { Id = 8192, Name = ReturnValue, Code = RETURNVALUE, Description = , Order = 0, Value = ReturnValue, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = ReturnValue }
EnumModel { Id = 16384, Name = GenericParameter, Code = GENERICPARAMETER, Description = , Order = 0, Value = GenericParameter, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = GenericParameter }
EnumModel { Id = 32767, Name = All, Code = ALL, Description = , Order = 0, Value = All, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = All }
";
        Assert.AreEqual(expected, result);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AsModelsTest2()
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            //TODO: fix this better
            Assert.Inconclusive();
            return;
        }

        var values = EnumExtensions.AsModels<TestEnum>();

        foreach (var e in values)
            TestContext?.WriteLine(e.ToString());

        var result = values.Aggregate(new StringBuilder(), (sb, v) => sb.Append(v).AppendLine()).ToString();

        var expected = @"EnumModel { Id = 0, Name = Val0, Code = VAL0, Description = , Order = 0, Value = Val0, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = Val0 }
EnumModel { Id = 1, Name = Val1, Code = VAL1, Description = , Order = 0, Value = Val1, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = Val1 }
EnumModel { Id = 2, Name = Val2, Code = VAL2, Description = , Order = 0, Value = Val2, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = Val2 }
EnumModel { Id = 4, Name = WithEnumValue, Code = name, Description = , Order = 0, Value = WithEnumValue, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = WithEnumValue }
EnumModel { Id = 8, Name = WithMemberName, Code = name2, Description = , Order = 0, Value = WithMemberName, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = WithMemberName }
EnumModel { Id = 16, Name = WithDescription, Code = WITHDESCRIPTION, Description = test description, Order = 0, Value = WithDescription, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = WithDescription }
EnumModel { Id = 32, Name = test name, Code = test short, Description = test display, Order = 0, Value = WithDisplay, IsEndState = False, IsExcludeFromUnique = False, PossibleNames = System.String[], Value = WithDisplay }
";
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(null, null)]
    [DataRow("Val1", TestEnum.Val1)]
    [DataRow("Val2", TestEnum.Val2)]
    [DataRow("Val2, Val1", TestEnum.Val2 | TestEnum.Val1)]
    [DataRow("Val2,Val1", TestEnum.Val2 | TestEnum.Val1)]
    [DataRow("Val2 , Val1", TestEnum.Val2 | TestEnum.Val1)]
    [DataRow("Val2 ,Val1", TestEnum.Val2 | TestEnum.Val1)]
    [DataRow("Val2|Val1", TestEnum.Val2 | TestEnum.Val1)]
    [DataRow("-1", (TestEnum)(-1))]
    [DataRow("0", TestEnum.Val0)]
    [DataRow("1", TestEnum.Val1)]
    [DataRow("2", TestEnum.Val2)]
    [DataRow("3", TestEnum.Val2 | TestEnum.Val1)]
    [DataRow("4", (TestEnum)4)]
    [DataRow("no data", null)]
    [DataRow("name", TestEnum.WithEnumValue)]
    [DataRow("name2", TestEnum.WithMemberName)]
    [DataRow("test description", TestEnum.WithDescription)]
    [DataRow("test display", TestEnum.WithDisplay)]
    [DataRow("test name", TestEnum.WithDisplay)]
    [DataRow("test short", TestEnum.WithDisplay)]
    [DataRow("name,name2|test name", TestEnum.WithEnumValue | TestEnum.WithMemberName | TestEnum.WithDisplay)]
    public void ToEnumTest(string input, TestEnum? expected)
    {
        var value = input.ToEnum<TestEnum>();
        Assert.AreEqual(expected, value);
    }

    [Flags]
    public enum TestEnum
    {
        Val0 = 0,
        Val1 = 1,
        Val2 = 2,
        [EnumValue("name")]
        WithEnumValue = 4,
        [EnumMember(Value = "name2")]
        WithMemberName = 8,
        [global::System.ComponentModel.Description("test description")]
        WithDescription = 16,

        [global::System.ComponentModel.DataAnnotations.Display(
            Description = "test display",
            Name = "test name",
            ShortName = "test short"
            )]
        WithDisplay = 32,
    }
}
