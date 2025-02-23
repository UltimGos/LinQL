#pragma warning disable CA1852 // Seal internal types
#nullable enable
namespace LinQL.Tests.Translation;

using System;
using System.Linq.Expressions;
using LinQL.Description;
using LinQL.Expressions;
using LinQL.Translation;
using Xunit;

public class TranslationProviderTests
{
    [Fact]
    public void ScalarOnRoot()
        => this.Test<ScarlarOnRootType, object>(x => x.GetNumber());

    [Fact]
    public void Simple()
        => this.Test<SimpleScalarType, object>(x => new { x.Number, x.Text });

    [Fact]
    public void NestedMemberFlatten()
        => this.Test<NestedClassType, object>(x => new { x.Float, x.Nested!.Number, x.Nested.Text });

    [Fact]
    public void NestedMember()
        => this.Test<NestedClassType, object>(x => new { x.Float, Nested = new { x.Nested!.Number } });

    [Fact]
    public void SimpleOperation()
        => this.Test<SimpleOperationType, object>(x => new { x.Float, x.GetNumber().Number });

    [Fact]
    public void OperationWithConstantParameters()
        => this.Test<OperationWithScalarParametersType, object>(x => new { x.Float, x.GetNumber("123").Number });

    [Fact]
    public void OperationWithObjectParameters()
        => this.Test<OperationWithTypeParametersType, object>(x => new { x.Float, x.GetNumber(new SimpleScalarType()).Number });

    [Fact]
    public void OperationWithVariableMemberParameters()
    {
        var input = new SimpleScalarType { Number = 123, Text = "321" };

        this.Test<OperationWithScalarParametersType, object>(x => new { x.GetNumber(input.Text).Number });
    }

    [Fact]
    public void OperationWithVariableParameters()
    {
        var input = new SimpleScalarType { Number = 123, Text = "321" };

        this.Test<OperationWithTypeParametersType, object>(x => new { x.GetNumber(input).Number });
    }

    [Fact]
    public void OperationWithNullableVariableParameters()
    {
        var input = new SimpleScalarType { Number = 123, Text = "321" };

        this.Test<OperationWithNullableTypeParametersType, object>(x => new { x.GetNumber(input)!.Number });
    }

    [Fact]
    public void NestedOperationWithVariableParameters()
    {
        var input = new SimpleScalarType { Number = 123, Text = "321" };

        this.Test<NestedOperationType, object>(x => new
        {
            x.Operation.GetNumber(input.Text).Number,
            x.Text,
        });
    }

    [Fact]
    public void NestedOperationInOperation()
    {
        var input = new SimpleScalarType { Number = 123, Text = "321" };

        this.Test<NestedOperationInOperationType, object>(x => new
        {
            x.Operation(input.Number).Operation.GetNumber(input.Text).Number,
            x.Text,
        });
    }

    [Fact]
    public void SelectAllNestedFields() => this.Test<NestedClassType, object>(x => x.Nested!);

    [Fact]
    public void SelectAllWithHelperNestedFields() => this.Test<NestedClassType, object>(x => x.Nested!.SelectAll());

    [Fact]
    public void SelectAllNestedFieldsFromOperation() => this.Test<NestedOperationType, object>(x => x.Operation.GetNumber("123")!);

    [Fact]
    public void SelectAllNestedFieldsWithHelperFromOperation() => this.Test<NestedOperationType, object>(x => x.Operation.GetNumber("123")!.SelectAll());

    [Fact]
    public void SelectFromArray() => this.Test<NestedArrayType, object>(x => x.Types.Select(x => new
    {
        x.Text,
        x.Number,
    }));

    [Fact]
    public void RenameField() => this.Test<RenamedType, object>(x => x.Text);

    [Fact]
    public void RenameOperation() => this.Test<RenamedType, object>(x => x.GetObject());

    [Fact]
    public void SimpleSelectInterface() => this.Test<InterfaceRootType, object>(x => x.SimpleType!.Number);

    [Fact]
    public void SelectInterfaceCastConcreteType()
        => this.Test<InterfaceRootType, object>(x => ((SomeOtherSimpleType)x.SimpleType!)!.Float);

    [Fact]
    public void SelectAllFieldsInterfaceCastConcreteType()
        => this.Test<InterfaceRootType, object>(x => (SomeOtherSimpleType)x.SimpleType!);

    [Fact]
    public void SelectInterfaceAsConcreteType()
        => this.Test<InterfaceRootType, object>(x => (x.SimpleType as SomeOtherSimpleType)!.Float);

    [Fact]
    public void SelectAllFieldsInterfaceAsConcreteType()
        => this.Test<InterfaceRootType, object>(x => (x.SimpleType as SomeOtherSimpleType)!);

    [Fact]
    public void SelectAllFieldsInterfaceOnConcreteType()
        => this.Test<InterfaceRootType, object>(x =>
            x.SimpleType.On((SomeOtherSimpleType y) => y.Number.ToString()!)
                        .On((SimpleScalarType y) => y.Text));

    [Fact]
    public void SelectInterfaceArray()
        => this.Test<InterfaceRootType, object>(x => x.ArrayOfInterfaces.OfType<SomeOtherSimpleType>().Select(y => new { y.Number, y.Text, y.Float }));

    [Fact]
    public void SelectInterfaceArrayAllFieldsOnType()
        => this.Test<InterfaceRootType, object>(x => x.ArrayOfInterfaces.OfType<SomeOtherSimpleType>());

    [Fact]
    public void SelectInterfaceArrayCastConcreteType()
        => this.Test<InterfaceRootType, object>(x => x.ArrayOfInterfaces.Select(y => new { ((SomeOtherSimpleType)y).Float, y.Text }));

    [Fact]
    public void SelectInterfaceArrayAsConcreteType()
        => this.Test<InterfaceRootType, object>(x => x.ArrayOfInterfaces.Select(y => new { (y as SomeOtherSimpleType)!.Float, y.Text }));

    [Fact]
    public void SelectInterfaceArrayAsConcreteTypeWithOperation()
        => this.Test<InterfaceRootType, object>(x => x.ArrayOfInterfaces.Select(y => new { Number = (y as SomeOtherSimpleType)!.Operation.GetNumber("123"), y.Text }));

    [Fact]
    public void SelectInterfaceArrayOnConcreteType()
        => this.Test<InterfaceRootType, object>(x => x.ArrayOfInterfaces
            .Select(y => y.On((SimpleScalarType z) => z.Text)
                          .On((SomeOtherSimpleType z) => z.Float.ToString()!)));

    [Fact]
    public void SelectInterfaceArrayMultipleTypes()
        => this.Test<InterfaceRootType, object>(x => new
        {
            Types = x.ArrayOfInterfaces.OfType<SimpleScalarType>().Select(y => new { y.Number, y.Text }),
            OtherTypes = x.ArrayOfInterfaces.OfType<SomeOtherSimpleType>().Select(y => new { y.Number, y.Text, y.Float }),
        });

    [Fact]
    public void SelectInterfaceArrayMultipleTypesSelectAll()
        => this.Test<InterfaceRootType, object>(x => new
        {
            Types = x.ArrayOfInterfaces.OfType<SimpleScalarType>().Select(y => y.SelectAll()),
            OtherTypes = x.ArrayOfInterfaces.OfType<SomeOtherSimpleType>().Select(y => y.SelectAll()),
        });

    [Fact]
    public void BasicInclude()
        => this.TestInclude<NestedOperationType, object>(x => x.Operation, x => x.Include(y => y.Operation.GetNumber("123")));

    private void TestInclude<TRoot, TData>(Expression<Func<TRoot, TData>> expression, Action<GraphQLExpression<TRoot, TData>> includes)
        where TRoot : RootType<TRoot>
    {
        var request = TranslationProvider.ToRequest(expression, new(), includes);

        Snapshot.Match(request.Query);
    }

    private void Test<TRoot, TData>(Expression<Func<TRoot, TData>> expression)
        where TRoot : RootType<TRoot>
        => this.TestInclude(expression, _ => { });

    [OperationType]
    private class SimpleScalarType : ISimpleType, RootType<SimpleScalarType>
    {
        public int Number { get; set; }

        public string Text { get; set; }
    }

    [OperationType]
    private class NestedClassType : RootType<NestedClassType>
    {
        public SimpleScalarType Nested { get; set; }

        public float Float { get; set; }
    }

    [OperationType]
    private class NestedArrayType : RootType<NestedArrayType>
    {
        public IEnumerable<SimpleScalarType> Types { get; set; } = null!;

        public float Float { get; set; }
    }

    [OperationType]
    private class SimpleOperationType : RootType<SimpleOperationType>
    {
        [GraphQLOperation]
        public SimpleScalarType GetNumber() => default!;

        public float Float { get; set; }
    }

    [OperationType]
    private class OperationWithScalarParametersType : RootType<OperationWithScalarParametersType>
    {
        [GraphQLOperation]
        public SimpleScalarType GetNumber([GraphQLArgument(GQLType = "String!")] string value) => new() { Text = value };

        public float Float { get; set; }
    }

    [OperationType]
    private class OperationWithTypeParametersType : RootType<OperationWithTypeParametersType>
    {
        [GraphQLOperation]
        public SimpleScalarType GetNumber([GraphQLArgument(GQLType = "SimpleScalarType!")] SimpleScalarType input) => input;

        public float Float { get; set; }
    }

    [OperationType]
    private class OperationWithNullableTypeParametersType : RootType<OperationWithNullableTypeParametersType>
    {
        [GraphQLOperation]
        public SimpleScalarType? GetNumber([GraphQLArgument(GQLType = "SimpleScalarType")] SimpleScalarType? input) => input;

        public float Float { get; set; }
    }

    [OperationType]
    private class NestedOperationType : RootType<NestedOperationType>
    {
        public OperationWithScalarParametersType Operation => default!;

        public string Text => default!;

        public int Number { get; set; }
    }

    [OperationType]
    private class NestedOperationInOperationType : RootType<NestedOperationInOperationType>
    {
        [GraphQLOperation]
        public NestedOperationType Operation([GraphQLArgument(GQLType = "Int!")] int number) => new() { Number = number };

        public string Text => default!;
    }

    [OperationType]
    private class RenamedType : RootType<RenamedType>
    {
        [GraphQLField(Name = "notText")]
        public string Text => default!;


        [GraphQLField(Name = "object")]
        [GraphQLOperation()]
        public SimpleScalarType GetObject() => default!;
    }

    private interface ISimpleType
    {
        string Text { get; }

        int Number { get; }
    }

    private class SomeOtherSimpleType : ISimpleType
    {
        public string Text { get; }

        public int Number { get; }

        public float Float { get; }

        public OperationWithScalarParametersType Operation { get; } = default!;
    }

    [OperationType]
    private class InterfaceRootType : RootType<InterfaceRootType>
    {
        public ISimpleType SimpleType { get; set; }

        public IEnumerable<ISimpleType> ArrayOfInterfaces { get; set; } = Enumerable.Empty<ISimpleType>();
    }

    [OperationType(RootOperationType.Query)]
    public class ScarlarOnRootType : RootType<ScarlarOnRootType>
    {
        public int Number { get; set; }

        [GraphQLOperation, GraphQLField(Name = "number")]
        public int GetNumber() => this.Number;
    }
}
