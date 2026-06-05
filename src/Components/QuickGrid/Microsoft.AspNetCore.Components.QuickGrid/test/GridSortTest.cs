// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Linq.Expressions;

namespace Microsoft.AspNetCore.Components.QuickGrid.Tests;

public class GridSortTest
{
    // Test model classes
    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime? NullableDate { get; set; }
        public int? NullableInt { get; set; }
        public TestChild Child { get; set; } = new();
    }

    private class TestChild
    {
        public string ChildName { get; set; } = string.Empty;
        public DateTime? ChildNullableDate { get; set; }
    }

    [Fact]
    public void ToPropertyList_SimpleProperty_ReturnsCorrectPropertyName()
    {
        // Arrange
        Expression<Func<TestEntity, string>> expression = x => x.Name;

        // Act
        var gridSort = GridSort<TestEntity>.ByAscending(expression);
        var propertyList = gridSort.ToPropertyList(ascending: true);

        // Assert
        Assert.Single(propertyList);
        Assert.Equal("Name", propertyList.First().PropertyName);
        Assert.Equal(SortDirection.Ascending, propertyList.First().Direction);
    }

    [Fact]
    public void ToPropertyList_NullableProperty_ReturnsCorrectPropertyName()
    {
        // Arrange
        Expression<Func<TestEntity, DateTime?>> expression = x => x.NullableDate;

        // Act
        var gridSort = GridSort<TestEntity>.ByAscending(expression);
        var propertyList = gridSort.ToPropertyList(ascending: true);

        // Assert
        Assert.Single(propertyList);
        Assert.Equal("NullableDate", propertyList.First().PropertyName);
        Assert.Equal(SortDirection.Ascending, propertyList.First().Direction);
    }

    [Fact]
    public void ToPropertyName_NullableInt_ReturnsPropertyName()
    {
        // Arrange
        Expression<Func<TestEntity, int?>> expression = x => x.NullableInt;

        // Act
        var gridSort = GridSort<TestEntity>.ByAscending(expression);
        var propertyList = gridSort.ToPropertyList(ascending: true);

        // Assert
        Assert.Single(propertyList);
        Assert.Equal("NullableInt", propertyList.First().PropertyName);
        Assert.Equal(SortDirection.Ascending, propertyList.First().Direction);
    }

    [Fact]
    public void ToPropertyName_NestedProperty_ReturnsNestedPropertyName()
    {
        // Arrange
        Expression<Func<TestEntity, string>> expression = x => x.Child.ChildName;

        // Act
        var gridSort = GridSort<TestEntity>.ByAscending(expression);
        var propertyList = gridSort.ToPropertyList(ascending: true);

        // Assert
        Assert.Single(propertyList);
        Assert.Equal("Child.ChildName", propertyList.First().PropertyName);
        Assert.Equal(SortDirection.Ascending, propertyList.First().Direction);
    }

    [Fact]
    public void ToPropertyName_NestedNullableProperty_ReturnsNestedPropertyName()
    {
        // Arrange
        Expression<Func<TestEntity, DateTime?>> expression = x => x.Child.ChildNullableDate;

        // Act
        var gridSort = GridSort<TestEntity>.ByAscending(expression);
        var propertyList = gridSort.ToPropertyList(ascending: true);

        // Assert
        Assert.Single(propertyList);
        Assert.Equal("Child.ChildNullableDate", propertyList.First().PropertyName);
        Assert.Equal(SortDirection.Ascending, propertyList.First().Direction);
    }

    [Fact]
    public void ToPropertyName_DescendingSort_ReturnsCorrectDirection()
    {
        // Arrange
        Expression<Func<TestEntity, DateTime?>> expression = x => x.NullableDate;

        // Act
        var gridSort = GridSort<TestEntity>.ByDescending(expression);
        var propertyList = gridSort.ToPropertyList(ascending: true);

        // Assert
        Assert.Single(propertyList);
        Assert.Equal("NullableDate", propertyList.First().PropertyName);
        Assert.Equal(SortDirection.Descending, propertyList.First().Direction);
    }

    [Fact]
    public void ToPropertyName_MultipleSort_ReturnsAllProperties()
    {
        // Arrange
        Expression<Func<TestEntity, string>> firstExpression = x => x.Name;
        Expression<Func<TestEntity, DateTime?>> secondExpression = x => x.NullableDate;

        // Act
        var gridSort = GridSort<TestEntity>.ByAscending(firstExpression)
            .ThenDescending(secondExpression);
        var propertyList = gridSort.ToPropertyList(ascending: true);

        // Assert
        Assert.Equal(2, propertyList.Count);

        var firstProperty = propertyList.First();
        Assert.Equal("Name", firstProperty.PropertyName);
        Assert.Equal(SortDirection.Ascending, firstProperty.Direction);

        var secondProperty = propertyList.Last();
        Assert.Equal("NullableDate", secondProperty.PropertyName);
        Assert.Equal(SortDirection.Descending, secondProperty.Direction);
    }

    [Fact]
    public void ToPropertyName_InvalidExpression_ThrowsArgumentException()
    {
        // Arrange
        Expression<Func<TestEntity, string>> invalidExpression = x => x.Name.ToUpper(CultureInfo.InvariantCulture);

        // Act & Assert
        var gridSort = GridSort<TestEntity>.ByAscending(invalidExpression);
        var exception = Assert.Throws<ArgumentException>(() => gridSort.ToPropertyList(ascending: true));
        Assert.Contains("The supplied expression can't be represented as a property name for sorting", exception.Message);
    }

    [Fact]
    public void ToPropertyName_MethodCallExpression_ThrowsArgumentException()
    {
        // Arrange
        Expression<Func<TestEntity, string>> invalidExpression = x => x.Name.Substring(0, 1);

        // Act & Assert
        var gridSort = GridSort<TestEntity>.ByAscending(invalidExpression);
        var exception = Assert.Throws<ArgumentException>(() => gridSort.ToPropertyList(ascending: true));
        Assert.Contains("The supplied expression can't be represented as a property name for sorting", exception.Message);
    }

    [Fact]
    public void ToPropertyName_ConstantExpression_ThrowsArgumentException()
    {
        // Arrange
        Expression<Func<TestEntity, string>> invalidExpression = x => "constant";

        // Act & Assert
        var gridSort = GridSort<TestEntity>.ByAscending(invalidExpression);
        var exception = Assert.Throws<ArgumentException>(() => gridSort.ToPropertyList(ascending: true));
        Assert.Contains("The supplied expression can't be represented as a property name for sorting", exception.Message);
    }

    [Fact]
    public void ToPropertyList_CachesAscendingAndDescendingSeparately()
    {
        Expression<Func<TestEntity, string>> first = x => x.Name;
        Expression<Func<TestEntity, int?>> second = x => x.NullableInt;

        var gridSort = GridSort<TestEntity>.ByAscending(first).ThenDescending(second);

        var asc1 = gridSort.ToPropertyList(ascending: true);
        var asc2 = gridSort.ToPropertyList(ascending: true);
        var desc1 = gridSort.ToPropertyList(ascending: false);
        var desc2 = gridSort.ToPropertyList(ascending: false);

        Assert.Same(asc1, asc2);
        Assert.Same(desc1, desc2);
        Assert.NotSame(asc1, desc1);
    }

    [Fact]
    public void ByAscending_Apply_SortsAscendingWhenRequested()
    {
        // Arrange
        Expression<Func<TestEntity, string>> expression = x => x.Name;
        var gridSort = GridSort<TestEntity>.ByAscending(expression);
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Name = "Charlie" },
            new() { Name = "Alice" },
            new() { Name = "Bob" }
        }.AsQueryable();

        // Act
        var result = gridSort.Apply(data, ascending: true);
        var orderedList = result.ToList();

        // Assert
        Assert.Equal("Alice", orderedList[0].Name);
        Assert.Equal("Bob", orderedList[1].Name);
        Assert.Equal("Charlie", orderedList[2].Name);
    }

    [Fact]
    public void ByDescending_Apply_SortsDescendingWhenRequested()
    {
        // Arrange
        Expression<Func<TestEntity, string>> expression = x => x.Name;
        var gridSort = GridSort<TestEntity>.ByDescending(expression);
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Name = "Charlie" },
            new() { Name = "Alice" },
            new() { Name = "Bob" }
        }.AsQueryable();

        // Act
        var result = gridSort.Apply(data, ascending: true);
        var orderedList = result.ToList();

        // Assert
        Assert.Equal("Charlie", orderedList[0].Name);
        Assert.Equal("Bob", orderedList[1].Name);
        Assert.Equal("Alice", orderedList[2].Name);
    }

    [Fact]
    public void ByAscending_ThenAscending_ChainsSortClauses()
    {
        // Arrange
        Expression<Func<TestEntity, int>> firstExpression = x => x.Age;
        Expression<Func<TestEntity, string>> secondExpression = x => x.Name;
        var gridSort = GridSort<TestEntity>.ByAscending(firstExpression).ThenAscending(secondExpression);
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Age = 30, Name = "Bob" },
            new() { Age = 25, Name = "Alice" },
            new() { Age = 30, Name = "Alice" },
            new() { Age = 25, Name = "Bob" }
        }.AsQueryable();

        // Act
        var result = gridSort.Apply(data, ascending: true);
        var orderedList = result.ToList();

        // Assert - Primary: Age ascending, Secondary: Name ascending
        Assert.Equal(25, orderedList[0].Age);
        Assert.Equal("Alice", orderedList[0].Name);
        Assert.Equal(25, orderedList[1].Age);
        Assert.Equal("Bob", orderedList[1].Name);
        Assert.Equal(30, orderedList[2].Age);
        Assert.Equal("Alice", orderedList[2].Name);
        Assert.Equal(30, orderedList[3].Age);
        Assert.Equal("Bob", orderedList[3].Name);
    }

    [Fact]
    public void ByAscending_ThenDescending_InvertsSecondaryDirection()
    {
        // Arrange
        Expression<Func<TestEntity, int>> firstExpression = x => x.Age;
        Expression<Func<TestEntity, string>> secondExpression = x => x.Name;
        var gridSort = GridSort<TestEntity>.ByAscending(firstExpression).ThenDescending(secondExpression);
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Age = 30, Name = "Bob" },
            new() { Age = 25, Name = "Alice" },
            new() { Age = 30, Name = "Alice" },
            new() { Age = 25, Name = "Bob" }
        }.AsQueryable();

        // Act
        var result = gridSort.Apply(data, ascending: true);
        var orderedList = result.ToList();

        // Assert - Primary: Age ascending, Secondary: Name DESCENDING (inverted)
        Assert.Equal(25, orderedList[0].Age);
        Assert.Equal("Bob", orderedList[0].Name); // Alice first if asc, but we said desc
        Assert.Equal(25, orderedList[1].Age);
        Assert.Equal("Alice", orderedList[1].Name);
        Assert.Equal(30, orderedList[2].Age);
        Assert.Equal("Bob", orderedList[2].Name);
        Assert.Equal(30, orderedList[3].Age);
        Assert.Equal("Alice", orderedList[3].Name);
    }

    [Fact]
    public void ToPropertyList_CachesAscendingResults()
    {
        // Arrange
        Expression<Func<TestEntity, string>> expression = x => x.Name;
        var gridSort = GridSort<TestEntity>.ByAscending(expression);

        // Act - call twice
        var firstCall = gridSort.ToPropertyList(ascending: true);
        var secondCall = gridSort.ToPropertyList(ascending: true);

        // Assert - should be same reference (cached)
        Assert.Same(firstCall, secondCall);
    }

    [Fact]
    public void ToPropertyList_CachesDescendingResultsSeparately()
    {
        // Arrange
        Expression<Func<TestEntity, string>> expression = x => x.Name;
        var gridSort = GridSort<TestEntity>.ByAscending(expression);

        // Act
        var ascendingResult = gridSort.ToPropertyList(ascending: true);
        var descendingResult = gridSort.ToPropertyList(ascending: false);

        // Assert - different references, different content
        Assert.NotSame(ascendingResult, descendingResult);
        Assert.Equal(SortDirection.Ascending, ascendingResult.First().Direction);
        Assert.Equal(SortDirection.Descending, descendingResult.First().Direction);
    }

    [Fact]
    public void Apply_WithDescendingInitialSort_FlipsSortDirection()
    {
        // Arrange
        Expression<Func<TestEntity, string>> expression = x => x.Name;
        var gridSort = GridSort<TestEntity>.ByDescending(expression);
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Name = "Charlie" },
            new() { Name = "Alice" },
            new() { Name = "Bob" }
        }.AsQueryable();

        // Act - apply with ascending=true (should flip to descending because initial is desc)
        var result = gridSort.Apply(data, ascending: true);
        var orderedList = result.ToList();

        // Assert - flipped: desc first (initial was desc, so asc=true flips it)
        Assert.Equal("Charlie", orderedList[0].Name);
    }

    [Fact]
    public void Apply_WithChainedThenBy_CreatesCorrectlyOrderedQuery()
    {
        // Arrange
        Expression<Func<TestEntity, int>> ageExpr = x => x.Age;
        Expression<Func<TestEntity, string>> nameExpr = x => x.Name;
        var gridSort = GridSort<TestEntity>.ByAscending(ageExpr).ThenDescending(nameExpr);
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Age = 30, Name = "Bob" },
            new() { Age = 25, Name = "Bob" },
            new() { Age = 30, Name = "Alice" },
            new() { Age = 25, Name = "Alice" }
        }.AsQueryable();

        // Act
        var result = gridSort.Apply(data, ascending: true);
        var orderedList = result.ToList();

        // Assert
        // Age ascending first (25, 25, 30, 30), then Name descending within each age (Bob before Alice)
        Assert.Equal(25, orderedList[0].Age); Assert.Equal("Bob", orderedList[0].Name);
        Assert.Equal(25, orderedList[1].Age); Assert.Equal("Alice", orderedList[1].Name);
        Assert.Equal(30, orderedList[2].Age); Assert.Equal("Bob", orderedList[2].Name);
        Assert.Equal(30, orderedList[3].Age); Assert.Equal("Alice", orderedList[3].Name);
    }
}