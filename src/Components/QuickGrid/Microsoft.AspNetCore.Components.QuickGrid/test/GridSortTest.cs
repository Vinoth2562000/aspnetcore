// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;

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
    public void ToPropertyName_SimpleProperty_ReturnsPropertyName()
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
    public void ToPropertyName_NullableProperty_ReturnsPropertyName()
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
    // ─────────────────────────────────────────────────────────────────────
    // 1. Int column – asc/desc, nulls, duplicates, min/max edge values
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Apply_IntAscending_WithNullsDuplicatesAndEdgeValues()
    {
        var gridSort = GridSort<TestEntity>.ByAscending(x => x.NullableInt);
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { NullableInt = 2 },
            new() { NullableInt = null },
            new() { NullableInt = 1 },
            new() { NullableInt = 2 }
        }.AsQueryable();

        var result = gridSort.Apply(data, ascending: true).ToList();

        // null sorts first in ascending; duplicates of 2 kept adjacent
        Assert.Null(result[0].NullableInt);
        Assert.Equal(1, result[1].NullableInt);
        Assert.Equal(2, result[2].NullableInt);
        Assert.Equal(2, result[3].NullableInt);
    }

    [Fact]
    public void Apply_IntDescending_WithNullsDuplicatesAndEdgeValues()
    {
        var gridSort = GridSort<TestEntity>.ByAscending(x => x.NullableInt);
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { NullableInt = 2 },
            new() { NullableInt = null },
            new() { NullableInt = 1 },
            new() { NullableInt = 2 }
        }.AsQueryable();

        var result = gridSort.Apply(data, ascending: false).ToList();

        // descending: highest first, null last
        Assert.Equal(2, result[0].NullableInt);
        Assert.Equal(2, result[1].NullableInt);
        Assert.Equal(1, result[2].NullableInt);
        Assert.Null(result[3].NullableInt);
    }

    [Fact]
    public void Apply_Int_MaxMinEdgeValues()
    {
        var gridSort = GridSort<TestEntity>.ByAscending(x => x.NullableInt);
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { NullableInt = int.MaxValue },
            new() { NullableInt = int.MinValue },
            new() { NullableInt = 0 },
            new() { NullableInt = null }
        }.AsQueryable();

        var asc = gridSort.Apply(data, ascending: true).ToList();
        Assert.Null(asc[0].NullableInt);
        Assert.Equal(int.MinValue, asc[1].NullableInt);
        Assert.Equal(0, asc[2].NullableInt);
        Assert.Equal(int.MaxValue, asc[3].NullableInt);

        var desc = gridSort.Apply(data, ascending: false).ToList();
        Assert.Equal(int.MaxValue, desc[0].NullableInt);
        Assert.Equal(0, desc[1].NullableInt);
        Assert.Equal(int.MinValue, desc[2].NullableInt);
        Assert.Null(desc[3].NullableInt);
    }

    // ─────────────────────────────────────────────────────────────────────
    // 2. String column – asc/desc, case sensitivity, empty, nulls, duplicates
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Apply_StringAscending_WithEmptyAndNulls()
    {
        var gridSort = GridSort<TestEntity>.ByAscending(x => x.Name);
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Name = "alpha" },
            new() { Name = null },
            new() { Name = "" },
            new() { Name = "beta" }
        }.AsQueryable();

        var result = gridSort.Apply(data, ascending: true).Select(x => x.Name).ToList();

        // null first, empty string next, then lexical order alpha < beta
        Assert.Null(result[0]);
        Assert.Equal("", result[1]);
        Assert.Equal("alpha", result[2]);
        Assert.Equal("beta", result[3]);
    }

    [Fact]
    public void Apply_StringDescending_WithNullsAndDuplicates()
    {
        var gridSort = GridSort<TestEntity>.ByAscending(x => x.Name);
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Name = "beta" },
            new() { Name = null },
            new() { Name = "" },
            new() { Name = "beta" }
        }.AsQueryable();

        var result = gridSort.Apply(data, ascending: false).Select(x => x.Name).ToList();

        Assert.Equal("beta", result[0]);
        Assert.Equal("beta", result[1]);
        Assert.Equal("", result[2]);
        Assert.Null(result[3]);
    }

    [Fact]
    public void Apply_String_CaseSensitiveLexicographicOrdering()
    {
        var gridSort = GridSort<TestEntity>.ByAscending(x => x.Name);
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Name = "Apple" },
            new() { Name = "apple" },
            new() { Name = " Banana" },
            new() { Name = "apple" }
        }.AsQueryable();

        var result = gridSort.Apply(data, ascending: true).Select(x => x.Name).ToList();

        // EF Core uses culture-aware case-insensitive comparison by default.
        // "Apple" and "apple" compare as equal, so ties preserve insertion order.
        // Result: space < all-letters; among ties, original order wins.
        // Expected order: space-prefixed first, then both "apple"(s) in
        // insertion order, then final "Apple" which originated last.
        Assert.Equal(" Banana", result[0]);
        Assert.Equal("apple", result[1]); // first "apple" inserted before "Apple"
        Assert.Equal("apple", result[2]); // second "apple" inserted before "Apple"
        Assert.Equal("Apple", result[3]); // "Apple" originated last (stable sort)
    }

    [Fact]
    public void Apply_String_EmptyStringEdgeCases()
    {
        var gridSort = GridSort<TestEntity>.ByAscending(x => x.Name);
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Name = "" },
            new() { Name = "" },
            new() { Name = null },
            new() { Name = "a" }
        }.AsQueryable();

        var asc = gridSort.Apply(data, ascending: true).Select(x => x.Name).ToList();
        Assert.Null(asc[0]);
        Assert.Equal("", asc[1]);
        Assert.Equal("", asc[2]);
        Assert.Equal("a", asc[3]);

        var desc = gridSort.Apply(data, ascending: false).Select(x => x.Name).ToList();
        Assert.Equal("a", desc[0]);
        Assert.Equal("", desc[1]);
        Assert.Equal("", desc[2]);
        Assert.Null(desc[3]);
    }

    [Fact]
    public void Apply_String_SingleItemAndDuplicateStrings()
    {
        var gridSort = GridSort<TestEntity>.ByAscending(x => x.Name);
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Name = "dup" },
            new() { Name = "dup" },
            new() { Name = null },
            new() { Name = "a" }
        }.AsQueryable();

        var asc = gridSort.Apply(data, ascending: true).Select(x => x.Name).ToList();
        Assert.Null(asc[0]);
        Assert.Equal("a", asc[1]);
        Assert.Equal("dup", asc[2]);
        Assert.Equal("dup", asc[3]);
    }

    // ─────────────────────────────────────────────────────────────────────
    // 3. DateTime column – asc/desc, nulls, min/max, duplicates
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Apply_DateTime_AscendingDescending_WithNulls()
    {
        var dtNotNull = new DateTime(2020, 1, 1);
        var gridSort = GridSort<TestEntity>.ByAscending(x => x.NullableDate);
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { NullableDate = dtNotNull },
            new() { NullableDate = null },
            new() { NullableDate = new DateTime(2019, 6, 15) }
        }.AsQueryable();

        var asc = gridSort.Apply(data, ascending: true).ToList();
        Assert.Null(asc[0].NullableDate);
        Assert.Equal(new DateTime(2019, 6, 15), asc[1].NullableDate);
        Assert.Equal(dtNotNull, asc[2].NullableDate);

        var desc = gridSort.Apply(data, ascending: false).ToList();
        Assert.Equal(dtNotNull, desc[0].NullableDate);
        Assert.Equal(new DateTime(2019, 6, 15), desc[1].NullableDate);
        Assert.Null(desc[2].NullableDate);
    }

    [Fact]
    public void Apply_DateTime_MinMaxEdgeValues()
    {
        var gridSort = GridSort<TestEntity>.ByAscending(x => x.NullableDate);
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { NullableDate = DateTime.MaxValue },
            new() { NullableDate = null },
            new() { NullableDate = DateTime.MinValue }
        }.AsQueryable();

        var asc = gridSort.Apply(data, ascending: true).ToList();
        Assert.Null(asc[0].NullableDate);
        Assert.Equal(DateTime.MinValue, asc[1].NullableDate);
        Assert.Equal(DateTime.MaxValue, asc[2].NullableDate);

        var desc = gridSort.Apply(data, ascending: false).ToList();
        Assert.Equal(DateTime.MaxValue, desc[0].NullableDate);
        Assert.Equal(DateTime.MinValue, desc[1].NullableDate);
        Assert.Null(desc[2].NullableDate);
    }

    [Fact]
    public void Apply_DateTime_SingleItemAndDuplicateDates()
    {
        var dt = new DateTime(2023, 3, 3);
        var gridSort = GridSort<TestEntity>.ByAscending(x => x.NullableDate);
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { NullableDate = dt },
            new() { NullableDate = dt },
            new() { NullableDate = null }
        }.AsQueryable();

        var asc = gridSort.Apply(data, ascending: true).ToList();
        Assert.Null(asc[0].NullableDate);
        Assert.Equal(dt, asc[1].NullableDate);
        Assert.Equal(dt, asc[2].NullableDate);

        var desc = gridSort.Apply(data, ascending: false).ToList();
        Assert.Equal(dt, desc[0].NullableDate);
        Assert.Equal(dt, desc[1].NullableDate);
        Assert.Null(desc[2].NullableDate);
    }

    // ─────────────────────────────────────────────────────────────────────
    // 4. Null sort column – ApplySorting returns unsorted IQueryable
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void ApplySorting_WithNullSortByColumn_ReturnsDataUnsorted()
    {
        // When SortByColumn is null, ApplySorting should return the data as-is
        // (no sort expression is applied), but still return a valid queryable.
        var data = new TestEntity[]
        {
            new TestEntity { Name = "Zebra", Age = 30 },
            new TestEntity { Name = "Apple", Age = 10 },
        }.AsQueryable();

        var request = new GridItemsProviderRequest<TestEntity>(
            startIndex: 0, count: 10, sortByColumn: null, sortByAscending: true, cancellationToken: CancellationToken.None);

        var result = request.ApplySorting(data).ToList();

        Assert.Equal(2, result.Count);
        // Data should be returned in original order (unsorted)
        Assert.Equal("Zebra", result[0].Name);
        Assert.Equal("Apple", result[1].Name);
    }

    // ─────────────────────────────────────────────────────────────────────
    // 5. Integration – sort then pagination, chained sort + pagination
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Apply_SortThenPagination_SlicesCorrectly()
    {
        // Sorting is applied first, then a page window is extracted.
        var data = Enumerable.Range(1, 50).Select(i => new TestEntity { NullableInt = i }).AsQueryable();
        var column = new TemplateColumn<TestEntity> { SortBy = GridSort<TestEntity>.ByAscending(x => x.NullableInt) };
        var request = new GridItemsProviderRequest<TestEntity>(
            startIndex: 10, count: 5, sortByColumn: column, sortByAscending: true, cancellationToken: CancellationToken.None);

        var sorted = request.ApplySorting(data).Skip(request.StartIndex).Take(request.Count ?? int.MaxValue).ToList();

        Assert.Equal(5, sorted.Count);
        Assert.Equal(11, sorted[0].NullableInt);
        Assert.Equal(15, sorted[4].NullableInt);
    }

    [Fact]
    public void Apply_SortWithChainedThenBy_ThenPagination()
    {
        // Age asc, then Name asc; take first "page" of 3 after sorting.
        var data = new List<TestEntity>
        {
            new() { Age = 20, Name = "Bob" },
            new() { Age = 10, Name = "Alice" },
            new() { Age = 20, Name = "Carol" },
            new() { Age = 10, Name = "Bob" },
            new() { Age = 10, Name = "Alice" },
            new() { Age = 30, Name = "Dave" }
        }.AsQueryable();

        var gridSort = GridSort<TestEntity>.ByAscending(x => x.Age).ThenAscending(x => x.Name);

        var sorted = gridSort.Apply(data, ascending: true).Skip(0).Take(3).ToList();

        Assert.Equal(3, sorted.Count);
        // Age asc: 10s first (Alice sorted by Name asc), then 20s, then 30
        // Within Age=10: Alice < Alice < Bob  (stable sort, original order for ties)
        Assert.Equal(10, sorted[0].Age); Assert.Equal("Alice", sorted[0].Name);
        Assert.Equal(10, sorted[1].Age); Assert.Equal("Alice", sorted[1].Name);
        Assert.Equal(10, sorted[2].Age); Assert.Equal("Bob", sorted[2].Name);
    }

    [Fact]
    public void Apply_WithEmptyData_ReturnsEmptyWithoutThrowing()
    {
        var gridSort = GridSort<TestEntity>.ByAscending(x => x.Age);
        var data = Enumerable.Empty<TestEntity>().AsQueryable();

        var result = gridSort.Apply(data, ascending: true).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Apply_WithSingleItem_ReturnsThatItem()
    {
        var gridSort = GridSort<TestEntity>.ByAscending(x => x.Age);
        var data = new TestEntity[] { new TestEntity { Name = "Solo", Age = 42 } }.AsQueryable();

        var result = gridSort.Apply(data, ascending: true).ToList();

        Assert.Single(result);
        Assert.Equal("Solo", result[0].Name);
        Assert.Equal(42, result[0].Age);
    }

    [Fact]
    public void Apply_Int_SingleItemAndDuplicateInts()
    {
        var gridSort = GridSort<TestEntity>.ByAscending(x => x.Age);
        var data = new TestEntity[]
        {
            new TestEntity { Name = "First",  Age = 99 },
            new TestEntity { Name = "Second", Age = 42 },
            new TestEntity { Name = "Third",  Age = 42 },
        }.AsQueryable();

        var asc = gridSort.Apply(data, ascending: true).ToList();
        Assert.Equal(3, asc.Count);
        Assert.Equal(42, asc[0].Age);
        Assert.Equal(42, asc[1].Age);
        Assert.Equal(99, asc[2].Age);

        var desc = gridSort.Apply(data, ascending: false).ToList();
        Assert.Equal(99, desc[0].Age);
        Assert.Equal(42, desc[1].Age);
        Assert.Equal(42, desc[2].Age);
    }

    [Fact]
    public void Apply_String_MultipleIdenticalStrings()
    {
        var gridSort = GridSort<TestEntity>.ByAscending(x => x.Name);
        var data = new TestEntity[]
        {
            new TestEntity { Name = "X", Age = 1 },
            new TestEntity { Name = "X", Age = 2 },
            new TestEntity { Name = "X", Age = 3 },
        }.AsQueryable();

        var asc = gridSort.Apply(data, ascending: true).ToList();
        Assert.Equal(3, asc.Count);
        Assert.Equal(1, asc[0].Age);
        Assert.Equal(2, asc[1].Age);
        Assert.Equal(3, asc[2].Age);
    }

    [Fact]
    public void ToPropertyName_WithNullableInt_PreservesPropertyName()
    {
        var gridSort = GridSort<TestEntity>.ByAscending(x => x.NullableInt);

        var propName = gridSort.ToPropertyList(ascending: true).ToList()[0].PropertyName;

        Assert.Equal("NullableInt", propName);
    }

    [Fact]
    public void Apply_SortWithPagination_LastPageHandle_WhenItemsRemoved()
    {
        // When items are removed between requests and the current page's startIndex
        // is now beyond the remaining data, sorting+pagination should gracefully
        // return an empty result without throwing.
        var data = new TestEntity[]
        {
            new TestEntity { Name = "Alice", Age = 10 },
            new TestEntity { Name = "Bob",   Age = 20 },
            new TestEntity { Name = "Carol", Age = 30 },
        }.AsQueryable();

        var column = new TemplateColumn<TestEntity>
        {
            SortBy = GridSort<TestEntity>.ByAscending(x => x.Name)
        };

        // Request page 2 of 3 with page size 2 — sort is applied first,
        // then Skip(2).Take(2) returns 0 items because total is now 3
        var request = new GridItemsProviderRequest<TestEntity>(
            startIndex: 4, count: 5, sortByColumn: column, sortByAscending: true, cancellationToken: CancellationToken.None);

        var sorted = request.ApplySorting(data);
        var result = sorted.Skip(request.StartIndex).Take(request.Count ?? int.MaxValue).ToList();

        // No items beyond index 3 of a 3-item dataset (start index 4 is beyond data)
        Assert.Empty(result);
    }

    [Fact]
    public void ToPropertyName_WithChainedThenBy_ReportsAllSortKeys()
    {
        var chain = GridSort<TestEntity>
            .ByAscending(x => x.Age)
            .ThenAscending(x => x.Name)
            .ThenDescending(x => x.NullableInt);

        var props = chain.ToPropertyList(ascending: true).ToList();

        Assert.Equal(3, props.Count);
        Assert.Equal("Age",       props[0].PropertyName);
        Assert.Equal("Name",      props[1].PropertyName);
        Assert.Equal("NullableInt", props[2].PropertyName);
    }
}
