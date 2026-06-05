// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;
using Microsoft.AspNetCore.Components.Rendering;

namespace Microsoft.AspNetCore.Components.QuickGrid.Tests;

/// <summary>
/// Comprehensive test suite covering all QuickGrid features including sorting, pagination,
/// column formatting, virtualization, ItemsProvider, and accessibility features.
/// </summary>
public class QuickGridComprehensiveTest
{
    // Test entity for various feature tests
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public decimal Salary { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
    }

    private class NestedEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public ChildEntity Child { get; set; } = new();
    }

    private class ChildEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    #region Sorting Tests

    [Fact]
    public void GridSort_ByAscending_CreatesAscendingSort()
    {
        // Arrange
        Expression<Func<TestEntity, string>> expression = x => x.Name;

        // Act
        var gridSort = GridSort<TestEntity>.ByAscending(expression);
        var propertyList = gridSort.ToPropertyList(ascending: true);

        // Assert
        Assert.NotNull(gridSort);
        Assert.Single(propertyList);
        Assert.Equal("Name", propertyList.First().PropertyName);
        Assert.Equal(SortDirection.Ascending, propertyList.First().Direction);
    }

    [Fact]
    public void GridSort_ByDescending_CreatesDescendingSort()
    {
        // Arrange
        Expression<Func<TestEntity, int>> expression = x => x.Age;

        // Act
        var gridSort = GridSort<TestEntity>.ByDescending(expression);
        var propertyList = gridSort.ToPropertyList(ascending: true);

        // Assert
        Assert.NotNull(gridSort);
        Assert.Single(propertyList);
        Assert.Equal("Age", propertyList.First().PropertyName);
        Assert.Equal(SortDirection.Descending, propertyList.First().Direction);
    }

    [Fact]
    public void GridSort_ChainedThenAscending_CreatesMultipleSort()
    {
        // Arrange
        Expression<Func<TestEntity, string>> firstName = x => x.Name;
        Expression<Func<TestEntity, int>> secondAge = x => x.Age;

        // Act
        var gridSort = GridSort<TestEntity>.ByAscending(firstName)
            .ThenAscending(secondAge);
        var propertyList = gridSort.ToPropertyList(ascending: true);

        // Assert
        Assert.Equal(2, propertyList.Count);
        Assert.Equal("Name", propertyList.ElementAt(0).PropertyName);
        Assert.Equal(SortDirection.Ascending, propertyList.ElementAt(0).Direction);
        Assert.Equal("Age", propertyList.ElementAt(1).PropertyName);
        Assert.Equal(SortDirection.Ascending, propertyList.ElementAt(1).Direction);
    }

    [Fact]
    public void GridSort_ChainedThenDescending_InvertsSecondaryDirection()
    {
        // Arrange
        Expression<Func<TestEntity, string>> firstName = x => x.Name;
        Expression<Func<TestEntity, int>> secondAge = x => x.Age;

        // Act
        var gridSort = GridSort<TestEntity>.ByAscending(firstName)
            .ThenDescending(secondAge);
        var propertyList = gridSort.ToPropertyList(ascending: true);

        // Assert
        Assert.Equal("Name", propertyList.ElementAt(0).PropertyName);
        Assert.Equal(SortDirection.Ascending, propertyList.ElementAt(0).Direction);
        Assert.Equal("Age", propertyList.ElementAt(1).PropertyName);
        Assert.Equal(SortDirection.Descending, propertyList.ElementAt(1).Direction);
    }

    [Fact]
    public void GridSort_ApplyAscending_SortsDataAscending()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { Id = 3, Name = "Charlie" },
            new() { Id = 1, Name = "Alice" },
            new() { Id = 2, Name = "Bob" }
        }.AsQueryable();

        Expression<Func<TestEntity, string>> expression = x => x.Name;
        var gridSort = GridSort<TestEntity>.ByAscending(expression);

        // Act
        var result = gridSort.Apply(data, ascending: true).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal("Bob", result[1].Name);
        Assert.Equal("Charlie", result[2].Name);
    }

    [Fact]
    public void GridSort_ApplyDescending_SortsDataDescending()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { Id = 3, Name = "Charlie" },
            new() { Id = 1, Name = "Alice" },
            new() { Id = 2, Name = "Bob" }
        }.AsQueryable();

        Expression<Func<TestEntity, string>> expression = x => x.Name;
        var gridSort = GridSort<TestEntity>.ByAscending(expression);

        // Act
        var result = gridSort.Apply(data, ascending: false).ToList();

        // Assert
        Assert.Equal("Charlie", result[0].Name);
        Assert.Equal("Bob", result[1].Name);
        Assert.Equal("Alice", result[2].Name);
    }

    [Fact]
    public void GridSort_WithNullableProperty_HandlesNullCorrectly()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { Id = 1, Name = "Alice", ModifiedDate = new DateTime(2024, 1, 1) },
            new() { Id = 2, Name = "Bob", ModifiedDate = null },
            new() { Id = 3, Name = "Charlie", ModifiedDate = new DateTime(2024, 1, 3) }
        }.AsQueryable();

        Expression<Func<TestEntity, DateTime?>> expression = x => x.ModifiedDate;
        var gridSort = GridSort<TestEntity>.ByAscending(expression);

        // Act
        var result = gridSort.Apply(data, ascending: true).ToList();

        // Assert - nulls typically sort first in ascending order
        Assert.Equal(3, result.Count);
        Assert.Null(result[0].ModifiedDate);
    }

    [Fact]
    public void GridSort_WithNestedProperty_IncludesDotNotation()
    {
        // Arrange
        Expression<Func<NestedEntity, string>> expression = x => x.Child.Name;

        // Act
        var gridSort = GridSort<NestedEntity>.ByAscending(expression);
        var propertyList = gridSort.ToPropertyList(ascending: true);

        // Assert
        Assert.Single(propertyList);
        Assert.Equal("Child.Name", propertyList.First().PropertyName);
    }

    [Fact]
    public void GridSort_MultipleChainedSorts_CachesResultsAscendingAndDescendingSeparately()
    {
        // Arrange
        Expression<Func<TestEntity, string>> firstName = x => x.Name;
        Expression<Func<TestEntity, int>> secondAge = x => x.Age;
        var gridSort = GridSort<TestEntity>.ByAscending(firstName).ThenDescending(secondAge);

        // Act
        var resultAsc1 = gridSort.ToPropertyList(ascending: true);
        var resultAsc2 = gridSort.ToPropertyList(ascending: true);
        var resultDesc1 = gridSort.ToPropertyList(ascending: false);
        var resultDesc2 = gridSort.ToPropertyList(ascending: false);

        // Assert - same instances should be returned from cache
        Assert.Same(resultAsc1, resultAsc2);
        Assert.Same(resultDesc1, resultDesc2);
        Assert.NotSame(resultAsc1, resultDesc1);
    }

    [Fact]
    public void GridSort_InvalidExpression_ThrowsArgumentException()
    {
        // Arrange
        Expression<Func<TestEntity, string>> invalidExpression = x => x.Name.ToUpper(CultureInfo.InvariantCulture);

        // Act & Assert
        var gridSort = GridSort<TestEntity>.ByAscending(invalidExpression);
        var exception = Assert.Throws<ArgumentException>(() => gridSort.ToPropertyList(ascending: true));
        Assert.Contains("can't be represented as a property name", exception.Message);
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task PaginationState_MultiplePageNavigations_WorksCorrectly()
    {
        // Arrange
        var pagination = new PaginationState { ItemsPerPage = 10 };
        await pagination.SetTotalItemCountAsync(100);

        // Act & Assert - navigate through multiple pages
        await pagination.SetCurrentPageIndexAsync(0);
        Assert.Equal(0, pagination.CurrentPageIndex);

        await pagination.SetCurrentPageIndexAsync(5);
        Assert.Equal(5, pagination.CurrentPageIndex);

        await pagination.SetCurrentPageIndexAsync(9);
        Assert.Equal(9, pagination.CurrentPageIndex);
    }

    [Fact]
    public async Task PaginationState_ItemsPerPageChange_UpdatesLastPageIndex()
    {
        // Arrange
        var pagination = new PaginationState { ItemsPerPage = 10 };
        await pagination.SetTotalItemCountAsync(100);
        Assert.Equal(9, pagination.LastPageIndex);

        // Act
        pagination.ItemsPerPage = 20;

        // Assert - LastPageIndex should now be 4 (100 / 20 - 1)
        Assert.Equal(4, pagination.LastPageIndex);
    }

    [Fact]
    public async Task PaginationState_ReduceTotalItemCount_AdjustsCurrentPageIfNeeded()
    {
        // Arrange
        var pagination = new PaginationState { ItemsPerPage = 10 };
        await pagination.SetTotalItemCountAsync(100);
        await pagination.SetCurrentPageIndexAsync(8);

        // Act - reduce total items to 50, last page should be 4
        await pagination.SetTotalItemCountAsync(50);

        // Assert - current page should auto-adjust to 4 (the last valid page)
        Assert.Equal(4, pagination.CurrentPageIndex);
        Assert.Equal(4, pagination.LastPageIndex);
    }

    [Fact]
    public async Task PaginationState_IncreaseTotalItemCount_DoesNotChangeCurrentPage()
    {
        // Arrange
        var pagination = new PaginationState { ItemsPerPage = 10 };
        await pagination.SetTotalItemCountAsync(50);
        await pagination.SetCurrentPageIndexAsync(3);

        // Act
        await pagination.SetTotalItemCountAsync(200);

        // Assert - current page should remain unchanged
        Assert.Equal(3, pagination.CurrentPageIndex);
        Assert.Equal(19, pagination.LastPageIndex);
    }

    [Fact]
    public async Task PaginationState_SetSameTotalItemCount_NoEventRaised()
    {
        // Arrange
        var pagination = new PaginationState();
        await pagination.SetTotalItemCountAsync(100);

        var eventRaised = false;
        pagination.TotalItemCountChanged += (_, _) => eventRaised = true;

        // Act
        await pagination.SetTotalItemCountAsync(100);

        // Assert - no event should be raised
        Assert.False(eventRaised);
    }

    [Fact]
    public async Task PaginationState_TotalItemCountChangedEvent_RaisedOnChange()
    {
        // Arrange
        var pagination = new PaginationState();
        int? capturedCount = null;
        pagination.TotalItemCountChanged += (_, count) => capturedCount = count;

        // Act
        await pagination.SetTotalItemCountAsync(75);

        // Assert
        Assert.Equal(75, capturedCount);
    }

    [Fact]
    public async Task PaginationState_FirstPageCalculation_IsAlwaysZero()
    {
        // Arrange
        var pagination = new PaginationState { ItemsPerPage = 5 };

        // Act
        await pagination.SetCurrentPageIndexAsync(0);
        await pagination.SetTotalItemCountAsync(1);

        // Assert
        Assert.Equal(0, pagination.CurrentPageIndex);
        Assert.Equal(0, pagination.LastPageIndex);
    }

    [Fact]
    public async Task PaginationState_WithSingleItem_LastPageIndexIsZero()
    {
        // Arrange
        var pagination = new PaginationState { ItemsPerPage = 10 };

        // Act
        await pagination.SetTotalItemCountAsync(1);

        // Assert
        Assert.Equal(0, pagination.LastPageIndex);
    }

    [Fact]
    public async Task PaginationState_LargeItemCount_CalculatesCorrectly()
    {
        // Arrange
        var pagination = new PaginationState { ItemsPerPage = 25 };

        // Act
        await pagination.SetTotalItemCountAsync(1000);

        // Assert - (1000 - 1) / 25 = 39
        Assert.Equal(39, pagination.LastPageIndex);
    }

    [Fact]
    public void PaginationState_GetHashCode_IncludesAllRelevantFields()
    {
        // Arrange
        var pag1 = new PaginationState { ItemsPerPage = 10 };
        var pag2 = new PaginationState { ItemsPerPage = 10 };
        var pag3 = new PaginationState { ItemsPerPage = 20 };

        // Act & Assert
        Assert.Equal(pag1.GetHashCode(), pag2.GetHashCode());
        Assert.NotEqual(pag1.GetHashCode(), pag3.GetHashCode());
    }

    #endregion

    #region PropertyColumn Tests

    [Fact]
    public void PropertyColumn_WithIntFormat_AppliesFormatting()
    {
        // Arrange
        var column = new PropertyColumn<TestEntity, int>();
        Expression<Func<TestEntity, int>> propertyExpr = x => x.Id;

        // Act
        var propertyInfo = typeof(PropertyColumn<TestEntity, int>).GetProperty("Property");
        propertyInfo?.SetValue(column, propertyExpr);
        column.Format = "D5";

        var onParamsMethod = typeof(PropertyColumn<TestEntity, int>).GetMethod("OnParametersSet",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        onParamsMethod?.Invoke(column, null);

        // Assert
        Assert.Equal("D5", column.Format);
    }

    [Fact]
    public void PropertyColumn_WithDateTimeFormat_AppliesFormatting()
    {
        // Arrange
        var column = new PropertyColumn<TestEntity, DateTime>();
        Expression<Func<TestEntity, DateTime>> propertyExpr = x => x.CreatedDate;

        // Act
        var propertyInfo = typeof(PropertyColumn<TestEntity, DateTime>).GetProperty("Property");
        propertyInfo?.SetValue(column, propertyExpr);
        column.Format = "yyyy-MM-dd HH:mm:ss";

        var onParamsMethod = typeof(PropertyColumn<TestEntity, DateTime>).GetMethod("OnParametersSet",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        onParamsMethod?.Invoke(column, null);

        // Assert
        Assert.Equal("yyyy-MM-dd HH:mm:ss", column.Format);
    }

    [Fact]
    public void PropertyColumn_WithDecimalFormat_AppliesFormatting()
    {
        // Arrange
        var column = new PropertyColumn<TestEntity, decimal>();
        Expression<Func<TestEntity, decimal>> propertyExpr = x => x.Salary;

        // Act
        var propertyInfo = typeof(PropertyColumn<TestEntity, decimal>).GetProperty("Property");
        propertyInfo?.SetValue(column, propertyExpr);
        column.Format = "C2";

        var onParamsMethod = typeof(PropertyColumn<TestEntity, decimal>).GetMethod("OnParametersSet",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        onParamsMethod?.Invoke(column, null);

        // Assert
        Assert.Equal("C2", column.Format);
    }

    [Fact]
    public void PropertyColumn_WithNullableProperty_AutoGeneratesTitle()
    {
        // Arrange
        var column = new PropertyColumn<TestEntity, DateTime?>();
        Expression<Func<TestEntity, DateTime?>> propertyExpr = x => x.ModifiedDate;

        // Act
        var propertyInfo = typeof(PropertyColumn<TestEntity, DateTime?>).GetProperty("Property");
        propertyInfo?.SetValue(column, propertyExpr);

        var onParamsMethod = typeof(PropertyColumn<TestEntity, DateTime?>).GetMethod("OnParametersSet",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        onParamsMethod?.Invoke(column, null);

        // Assert
        Assert.Equal("ModifiedDate", column.Title);
    }

    [Fact]
    public void PropertyColumn_SetCustomTitle_PreventsTitleOverwrite()
    {
        // Arrange
        var column = new PropertyColumn<TestEntity, string>();
        Expression<Func<TestEntity, string>> propertyExpr = x => x.Name;

        // Act
        column.Title = "Employee Name";
        var propertyInfo = typeof(PropertyColumn<TestEntity, string>).GetProperty("Property");
        propertyInfo?.SetValue(column, propertyExpr);

        var onParamsMethod = typeof(PropertyColumn<TestEntity, string>).GetMethod("OnParametersSet",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        onParamsMethod?.Invoke(column, null);

        // Assert
        Assert.Equal("Employee Name", column.Title);
    }

    [Fact]
    public void PropertyColumn_SortBy_IsAutomaticallyGeneratedFromProperty()
    {
        // Arrange
        var column = new PropertyColumn<TestEntity, string>();
        Expression<Func<TestEntity, string>> propertyExpr = x => x.Name;

        // Act
        var propertyInfo = typeof(PropertyColumn<TestEntity, string>).GetProperty("Property");
        propertyInfo?.SetValue(column, propertyExpr);

        var onParamsMethod = typeof(PropertyColumn<TestEntity, string>).GetMethod("OnParametersSet",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        onParamsMethod?.Invoke(column, null);

        // Assert
        Assert.NotNull(column.SortBy);
    }

    [Fact]
    public void PropertyColumn_SetSortBy_ThrowsNotSupportedException()
    {
        // Arrange
        var column = new PropertyColumn<TestEntity, string>();

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => column.SortBy = null);
    }

    [Fact]
    public void PropertyColumn_WithFormatOnNonIFormattableType_ThrowsInvalidOperation()
    {
        // Arrange
        var column = new PropertyColumn<TestEntity, object>();
        Expression<Func<TestEntity, object>> propertyExpr = x => new object();

        // Act
        var propertyInfo = typeof(PropertyColumn<TestEntity, object>).GetProperty("Property");
        propertyInfo?.SetValue(column, propertyExpr);
        column.Format = "some-format";

        var onParamsMethod = typeof(PropertyColumn<TestEntity, object>).GetMethod("OnParametersSet",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Assert
        var exception = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
            onParamsMethod?.Invoke(column, null));
        Assert.IsType<InvalidOperationException>(exception.InnerException);
    }

    #endregion

    #region TemplateColumn Tests

    [Fact]
    public void TemplateColumn_SortBy_CanBeSetExplicitly()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();
        Expression<Func<TestEntity, string>> sortExpr = x => x.Name;
        var gridSort = GridSort<TestEntity>.ByAscending(sortExpr);

        // Act
        column.SortBy = gridSort;

        // Assert
        Assert.Same(gridSort, column.SortBy);
    }

    [Fact]
    public void TemplateColumn_WithoutSortBy_IsNotSortable()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();

        // Act
        var isSortableByDefault = typeof(TemplateColumn<TestEntity>).BaseType
            ?.GetMethod("IsSortableByDefault", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(column, null);

        // Assert
        Assert.False((bool)isSortableByDefault!);
    }

    [Fact]
    public void TemplateColumn_WithSortBy_IsSortable()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();
        Expression<Func<TestEntity, string>> sortExpr = x => x.Name;
        var gridSort = GridSort<TestEntity>.ByAscending(sortExpr);

        // Act
        column.SortBy = gridSort;
        var isSortableByDefault = typeof(TemplateColumn<TestEntity>).BaseType
            ?.GetMethod("IsSortableByDefault", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(column, null);

        // Assert
        Assert.True((bool)isSortableByDefault!);
    }

    [Fact]
    public void TemplateColumn_ChildContent_CanBeAssigned()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();

        // Act
        column.ChildContent = item => builder => builder.AddContent(0, item.Name);

        // Assert
        Assert.NotNull(column.ChildContent);
    }

    [Fact]
    public void TemplateColumn_MultipleChainedSorts_Works()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();
        Expression<Func<TestEntity, string>> nameSort = x => x.Name;
        Expression<Func<TestEntity, int>> ageSort = x => x.Age;

        // Act
        var nameGridSort = GridSort<TestEntity>.ByAscending(nameSort);
        var ageGridSort = nameGridSort.ThenAscending(ageSort);
        column.SortBy = ageGridSort;

        // Assert
        Assert.NotNull(column.SortBy);
        var propertyList = ageGridSort.ToPropertyList(true);
        Assert.Equal(2, propertyList.Count);
    }

    #endregion

    #region Column Base Tests

    [Fact]
    public void ColumnBase_Align_DefaultIsStart()
    {
        // Arrange & Act
        var grid = new QuickGrid<TestEntity>();
        var column = CreateTestColumn();

        // Assert
        Assert.Equal(Align.Start, column.Align);
    }

    [Fact]
    public void ColumnBase_Align_CanBeSetToCenter()
    {
        // Arrange
        var column = CreateTestColumn();

        // Act
        column.Align = Align.Center;

        // Assert
        Assert.Equal(Align.Center, column.Align);
    }

    [Fact]
    public void ColumnBase_Align_CanBeSetToEnd()
    {
        // Arrange
        var column = CreateTestColumn();

        // Act
        column.Align = Align.End;

        // Assert
        Assert.Equal(Align.End, column.Align);
    }

    [Fact]
    public void ColumnBase_Class_CanBeSet()
    {
        // Arrange
        var column = CreateTestColumn();

        // Act
        column.Class = "highlight bold";

        // Assert
        Assert.Equal("highlight bold", column.Class);
    }

    [Fact]
    public void ColumnBase_Sortable_CanBeSetToTrue()
    {
        // Arrange
        var column = CreateTestColumn();

        // Act
        column.Sortable = true;

        // Assert
        Assert.True(column.Sortable);
    }

    [Fact]
    public void ColumnBase_Sortable_CanBeSetToFalse()
    {
        // Arrange
        var column = CreateTestColumn();

        // Act
        column.Sortable = false;

        // Assert
        Assert.False(column.Sortable);
    }

    [Fact]
    public void ColumnBase_InitialSortDirection_CanBeSetToDescending()
    {
        // Arrange
        var column = CreateTestColumn();

        // Act
        column.InitialSortDirection = SortDirection.Descending;

        // Assert
        Assert.Equal(SortDirection.Descending, column.InitialSortDirection);
    }

    [Fact]
    public void ColumnBase_IsDefaultSortColumn_CanBeSetToTrue()
    {
        // Arrange
        var column = CreateTestColumn();

        // Act
        column.IsDefaultSortColumn = true;

        // Assert
        Assert.True(column.IsDefaultSortColumn);
    }

    #endregion

    #region Grid Features Tests

    [Fact]
    public void QuickGrid_ItemKey_DefaultReturnsItem()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();
        var entity = new TestEntity { Id = 1, Name = "Test" };

        // Act
        var key = grid.ItemKey(entity);

        // Assert
        Assert.Same(entity, key);
    }

    [Fact]
    public void QuickGrid_ItemKey_CustomKeyFunction()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();

        // Act
        grid.ItemKey = item => item.Id;

        // Assert
        var entity = new TestEntity { Id = 42, Name = "Test" };
        Assert.Equal(42, grid.ItemKey(entity));
    }

    [Fact]
    public void QuickGrid_Theme_DefaultValue()
    {
        // Arrange & Act
        var grid = new QuickGrid<TestEntity>();

        // Assert
        Assert.Equal("default", grid.Theme);
    }

    [Fact]
    public void QuickGrid_Theme_CanBeSet()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();

        // Act
        grid.Theme = "dark";

        // Assert
        Assert.Equal("dark", grid.Theme);
    }

    [Fact]
    public void QuickGrid_Theme_CanBeCustom()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();

        // Act
        grid.Theme = "custom-theme";

        // Assert
        Assert.Equal("custom-theme", grid.Theme);
    }

    [Fact]
    public void QuickGrid_Class_CanBeSet()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();

        // Act
        grid.Class = "table-striped table-hover";

        // Assert
        Assert.Equal("table-striped table-hover", grid.Class);
    }

    [Fact]
    public void QuickGrid_QueryParameterNamePrefix_DefaultIsEmpty()
    {
        // Arrange & Act
        var grid = new QuickGrid<TestEntity>();

        // Assert
        Assert.Equal("", grid.QueryParameterNamePrefix);
    }

    [Fact]
    public void QuickGrid_QueryParameterNamePrefix_CanBeSet()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();

        // Act
        grid.QueryParameterNamePrefix = "products";

        // Assert
        Assert.Equal("products", grid.QueryParameterNamePrefix);
    }

    [Fact]
    public void QuickGrid_Virtualize_DefaultIsFalse()
    {
        // Arrange & Act
        var grid = new QuickGrid<TestEntity>();

        // Assert
        Assert.False(grid.Virtualize);
    }

    [Fact]
    public void QuickGrid_Virtualize_CanBeEnabled()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();

        // Act
        grid.Virtualize = true;

        // Assert
        Assert.True(grid.Virtualize);
    }

    [Fact]
    public void QuickGrid_ItemSize_DefaultValue()
    {
        // Arrange & Act
        var grid = new QuickGrid<TestEntity>();

        // Assert
        Assert.Equal(50, grid.ItemSize);
    }

    [Fact]
    public void QuickGrid_ItemSize_CanBeSet()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();

        // Act
        grid.ItemSize = 75;

        // Assert
        Assert.Equal(75, grid.ItemSize);
    }

    [Fact]
    public void QuickGrid_OverscanCount_DefaultValue()
    {
        // Arrange & Act
        var grid = new QuickGrid<TestEntity>();

        // Assert
        Assert.Equal(3, grid.OverscanCount);
    }

    [Fact]
    public void QuickGrid_OverscanCount_CanBeSet()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();

        // Act
        grid.OverscanCount = 10;

        // Assert
        Assert.Equal(10, grid.OverscanCount);
    }

    [Fact]
    public void QuickGrid_OverscanCount_CanBeZero()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();

        // Act
        grid.OverscanCount = 0;

        // Assert
        Assert.Equal(0, grid.OverscanCount);
    }

    [Fact]
    public void QuickGrid_RowClass_CanBeSet()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();

        // Act
        grid.RowClass = item => item.Age > 30 ? "senior" : "junior";

        // Assert
        var entity = new TestEntity { Age = 35 };
        Assert.Equal("senior", grid.RowClass!(entity));
    }

    [Fact]
    public void QuickGrid_RowClass_ReturnsNullForNoClass()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();

        // Act
        grid.RowClass = item => null;

        // Assert
        var entity = new TestEntity { Age = 35 };
        Assert.Null(grid.RowClass!(entity));
    }

    [Fact]
    public void QuickGrid_Pagination_CanBeSet()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();
        var pagination = new PaginationState { ItemsPerPage = 20 };

        // Act
        grid.Pagination = pagination;

        // Assert
        Assert.Same(pagination, grid.Pagination);
        Assert.Equal(20, pagination.ItemsPerPage);
    }

    #endregion

    #region GridItemsProviderRequest Tests

    [Fact]
    public void GridItemsProviderRequest_StartIndex_IsCorrect()
    {
        // Arrange & Act
        var request = new GridItemsProviderRequest<TestEntity>(
            startIndex: 10,
            count: 20,
            sortByColumn: null,
            sortByAscending: true,
            cancellationToken: CancellationToken.None);

        // Assert
        Assert.Equal(10, request.StartIndex);
    }

    [Fact]
    public void GridItemsProviderRequest_Count_IsCorrect()
    {
        // Arrange & Act
        var request = new GridItemsProviderRequest<TestEntity>(
            startIndex: 0,
            count: 25,
            sortByColumn: null,
            sortByAscending: true,
            cancellationToken: CancellationToken.None);

        // Assert
        Assert.Equal(25, request.Count);
    }

    [Fact]
    public void GridItemsProviderRequest_CountNull_IsUnlimited()
    {
        // Arrange & Act
        var request = new GridItemsProviderRequest<TestEntity>(
            startIndex: 0,
            count: null,
            sortByColumn: null,
            sortByAscending: true,
            cancellationToken: CancellationToken.None);

        // Assert
        Assert.Null(request.Count);
    }

    [Fact]
    public void GridItemsProviderRequest_GetSortByProperties_ReturnsEmptyWhenNoSortColumn()
    {
        // Arrange
        var request = new GridItemsProviderRequest<TestEntity>(
            startIndex: 0,
            count: null,
            sortByColumn: null,
            sortByAscending: true,
            cancellationToken: CancellationToken.None);

        // Act
        var properties = request.GetSortByProperties();

        // Assert
        Assert.Empty(properties);
    }

    [Fact]
    public void GridItemsProviderRequest_ApplySorting_ReturnsSourceWhenNoSortColumn()
    {
        // Arrange
        var data = new List<TestEntity> { new() { Name = "Test" } }.AsQueryable();
        var request = new GridItemsProviderRequest<TestEntity>(
            startIndex: 0,
            count: null,
            sortByColumn: null,
            sortByAscending: true,
            cancellationToken: CancellationToken.None);

        // Act
        var result = request.ApplySorting(data);

        // Assert
        Assert.Same(data, result);
    }

    [Fact]
    public void GridItemsProviderRequest_CancellationToken_IsAccessible()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var request = new GridItemsProviderRequest<TestEntity>(
            startIndex: 0,
            count: null,
            sortByColumn: null,
            sortByAscending: true,
            cancellationToken: cts.Token);

        // Act & Assert
        Assert.Equal(cts.Token, request.CancellationToken);
        Assert.False(request.CancellationToken.IsCancellationRequested);
    }

    #endregion

    #region Integrated Feature Tests

    [Fact]
    public void QuickGrid_WithPaginationAndSorting_BothWorkTogether()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();
        var pagination = new PaginationState { ItemsPerPage = 10 };

        // Act
        grid.Pagination = pagination;
        grid.ItemKey = item => item.Id;

        // Assert
        Assert.Same(pagination, grid.Pagination);
        Assert.NotNull(grid.ItemKey);
    }

    [Fact]
    public void QuickGrid_WithVirtualizationAndItemSize_ConfiguresVirtualization()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();

        // Act
        grid.Virtualize = true;
        grid.ItemSize = 100;
        grid.OverscanCount = 5;

        // Assert
        Assert.True(grid.Virtualize);
        Assert.Equal(100, grid.ItemSize);
        Assert.Equal(5, grid.OverscanCount);
    }

    [Fact]
    public void QuickGrid_WithMultipleFeatures_AllPropertiesWork()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();
        var pagination = new PaginationState { ItemsPerPage = 15 };

        // Act
        grid.Class = "table-sm";
        grid.Theme = "bootstrap5";
        grid.QueryParameterNamePrefix = "emp";
        grid.Pagination = pagination;
        grid.Virtualize = true;
        grid.ItemSize = 60;
        grid.ItemKey = item => item.Id;
        grid.RowClass = item => item.IsActive ? "active" : null;

        // Assert
        Assert.Equal("table-sm", grid.Class);
        Assert.Equal("bootstrap5", grid.Theme);
        Assert.Equal("emp", grid.QueryParameterNamePrefix);
        Assert.Same(pagination, grid.Pagination);
        Assert.True(grid.Virtualize);
        Assert.Equal(60, grid.ItemSize);
        Assert.Equal(1, grid.ItemKey(new TestEntity { Id = 1 }));
        Assert.Equal("active", grid.RowClass!(new TestEntity { IsActive = true }));
    }

    #endregion

    #region Helper Methods

    private TemplateColumn<TestEntity> CreateTestColumn()
    {
        return new TemplateColumn<TestEntity>
        {
            ChildContent = item => builder => builder.AddContent(0, item.Name)
        };
    }

    #endregion
}
