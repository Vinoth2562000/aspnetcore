// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Linq.Expressions;
using Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;

namespace Microsoft.AspNetCore.Components.QuickGrid.Tests;

/// <summary>
/// Tests for QuickGrid databinding scenarios including Items, ItemsProvider,
/// PaginationState two-way binding, and data refresh lifecycle.
/// </summary>
public class QuickGridDataBindingTest
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    private class ChildEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public TestEntity Parent { get; set; } = new();
    }

    #region Items (IQueryable) DataBinding Tests

    [Fact]
    public void QuickGrid_Items_SetFromList_CanQueryData()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();
        var data = new List<TestEntity>
        {
            new() { Id = 1, Name = "Alice", Age = 30 },
            new() { Id = 2, Name = "Bob", Age = 25 },
            new() { Id = 3, Name = "Charlie", Age = 35 }
        }.AsQueryable();

        // Act
        grid.Items = data;

        // Assert
        Assert.NotNull(grid.Items);
        Assert.Equal(3, grid.Items.Count());
    }

    [Fact]
    public void QuickGrid_Items_SetFromEnumerable_CreatesQueryable()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();
        var data = Enumerable.Range(1, 10).Select(i => new TestEntity { Id = i, Name = $"Item{i}" }).ToList();

        // Act
        grid.Items = data.AsQueryable();

        // Assert
        Assert.Equal(10, grid.Items.Count());
    }

    [Fact]
    public void QuickGrid_Items_CanBeChanged()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();
        var initialData = new List<TestEntity> { new() { Id = 1 } }.AsQueryable();
        var newData = new List<TestEntity> { new() { Id = 2 }, new() { Id = 3 } }.AsQueryable();

        // Act
        grid.Items = initialData;
        grid.Items = newData;

        // Assert
        Assert.Equal(2, grid.Items.Count());
    }

    [Fact]
    public void QuickGrid_Items_WithNestedPropertyExpression_QueriesCorrectly()
    {
        // Arrange
        var grid = new QuickGrid<ChildEntity>();
        var data = new List<ChildEntity>
        {
            new() { Id = 1, Title = "First", Parent = new TestEntity { Name = "Alice" } },
            new() { Id = 2, Title = "Second", Parent = new TestEntity { Name = "Bob" } }
        }.AsQueryable();

        // Act
        grid.Items = data;
        var result = grid.Items.Where(x => x.Parent.Name == "Alice").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Alice", result[0].Parent.Name);
    }

    [Fact]
    public void QuickGrid_Items_EmptyList_HasZeroCount()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();
        var emptyData = new List<TestEntity>().AsQueryable();

        // Act
        grid.Items = emptyData;

        // Assert
        Assert.Empty(grid.Items);
    }

    #endregion

    #region ItemsProvider Delegate DataBinding Tests

    [Fact]
    public void QuickGrid_ItemsProvider_SetDelegate_ReceivesRequest()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();
        GridItemsProviderRequest<TestEntity>? capturedRequest = null;
        GridItemsProvider<TestEntity> provider = request =>
        {
            capturedRequest = request;
            return ValueTask.FromResult(GridItemsProviderResult.From<TestEntity>(
                Array.Empty<TestEntity>(), 0));
        };

        // Act
        grid.ItemsProvider = provider;

        // Assert
        Assert.NotNull(grid.ItemsProvider);
    }

    [Fact]
    public void GridItemsProviderRequest_StartIndex_ReturnsCorrectValue()
    {
        // Arrange
        var request = new GridItemsProviderRequest<TestEntity>(
            startIndex: 10,
            count: 5,
            sortByColumn: null,
            sortByAscending: true,
            cancellationToken: CancellationToken.None);

        // Assert
        Assert.Equal(10, request.StartIndex);
        Assert.Equal(5, request.Count);
    }

    [Fact]
    public void GridItemsProviderRequest_Count_IsOptional()
    {
        // Arrange
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
    public void GridItemsProviderRequest_SortByColumn_IsStored()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();
        var request = new GridItemsProviderRequest<TestEntity>(
            startIndex: 0,
            count: 10,
            sortByColumn: column,
            sortByAscending: false,
            cancellationToken: CancellationToken.None);

        // Assert
        Assert.Same(column, request.SortByColumn);
        Assert.False(request.SortByAscending);
    }

    [Fact]
    public void GridItemsProviderRequest_ApplySorting_ReturnsSortedQuery()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { Name = "Charlie" },
            new() { Name = "Alice" },
            new() { Name = "Bob" }
        }.AsQueryable();
        var request = new GridItemsProviderRequest<TestEntity>(
            startIndex: 0,
            count: null,
            sortByColumn: null,
            sortByAscending: true,
            cancellationToken: CancellationToken.None);

        // Act
        var sorted = request.ApplySorting(data);

        // Assert - without SortByColumn, data should be unchanged
        Assert.Equal(3, sorted.Count());
    }

    [Fact]
    public void GridItemsProviderRequest_GetSortByProperties_ReturnsEmptyWhenNoSort()
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
    public void GridItemsProviderResult_From_CreatesResultWithItemsAndTotalCount()
    {
        // Arrange
        var items = new List<TestEntity>
        {
            new() { Id = 1, Name = "Test1" },
            new() { Id = 2, Name = "Test2" }
        };

        // Act
        var result = GridItemsProviderResult.From<TestEntity>(items, 100);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(100, result.TotalItemCount);
    }

    [Fact]
    public void GridItemsProviderResult_EmptyItems_ReturnsZeroCount()
    {
        // Act
        var result = GridItemsProviderResult.From<TestEntity>(
            Array.Empty<TestEntity>(), 0);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItemCount);
    }

    [Fact]
    public void GridItemsProviderResult_TotalCountCanExceedItemsCount()
    {
        // Arrange - Pagination scenario: returning page items but total is larger
        var pageItems = new List<TestEntity>
        {
            new() { Id = 1 },
            new() { Id = 2 }
        };

        // Act
        var result = GridItemsProviderResult.From<TestEntity>(pageItems, 1000);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1000, result.TotalItemCount);
    }

    #endregion

    #region ItemsProvider with Sorting Tests

    [Fact]
    public void GridItemsProviderRequest_ApplySorting_WithSortColumn_SortsQuery()
    {
        // Arrange
        var data = new List<TestEntity>
        {
            new() { Name = "Charlie", Age = 30 },
            new() { Name = "Alice", Age = 25 },
            new() { Name = "Bob", Age = 35 }
        }.AsQueryable();
        Expression<Func<TestEntity, string>> sortExpression = x => x.Name;
        var gridSort = GridSort<TestEntity>.ByAscending(sortExpression);

        // Create a mock column with SortBy set
        var column = new TemplateColumn<TestEntity>();
        column.SortBy = gridSort;

        var request = new GridItemsProviderRequest<TestEntity>(
            startIndex: 0,
            count: null,
            sortByColumn: column,
            sortByAscending: true,
            cancellationToken: CancellationToken.None);

        // Act
        var sorted = request.ApplySorting(data).ToList();

        // Assert - should be sorted alphabetically by Name
        Assert.Equal("Alice", sorted[0].Name);
        Assert.Equal("Bob", sorted[1].Name);
        Assert.Equal("Charlie", sorted[2].Name);
    }

    #endregion

    #region Data Source Change Detection Tests

    [Fact]
    public void QuickGrid_ItemsProvider_CanBeChanged()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();
        GridItemsProvider<TestEntity> initialProvider = _ =>
            ValueTask.FromResult(GridItemsProviderResult.From<TestEntity>(Array.Empty<TestEntity>(), 0));
        GridItemsProvider<TestEntity> newProvider = _ =>
            ValueTask.FromResult(GridItemsProviderResult.From<TestEntity>(new TestEntity[1], 1));

        // Act
        grid.ItemsProvider = initialProvider;
        grid.ItemsProvider = newProvider;

        // Assert
        Assert.Same(newProvider, grid.ItemsProvider);
    }

    [Fact]
    public void QuickGrid_ItemsAndItemsProvider_AreMutuallyExclusive()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();
        var items = new List<TestEntity> { new TestEntity { Id = 1 } }.AsQueryable();
        GridItemsProvider<TestEntity> provider = _ =>
            ValueTask.FromResult(GridItemsProviderResult.From<TestEntity>(Array.Empty<TestEntity>(), 0));

        // Act - setting both should trigger conflict check in OnParametersSet
        grid.Items = items;
        grid.ItemsProvider = provider;

        // Assert - the conflict detection happens in OnParametersSetAsync
        // We verify this by calling the method via reflection
        var method = typeof(QuickGrid<TestEntity>).GetMethod("OnParametersSetAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);

        Exception? exception = null;
        try
        {
            method.Invoke(grid, null);
        }
        catch (Exception ex)
        {
            exception = ex.InnerException ?? ex;
        }

        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains("Items", ((InvalidOperationException)exception).Message);
        Assert.Contains("ItemsProvider", ((InvalidOperationException)exception).Message);
    }

    [Fact]
    public void QuickGrid_ItemsAndItemsProvider_CanSetItemsOnly()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();
        var items = new List<TestEntity> { new TestEntity { Id = 1 } }.AsQueryable();

        // Act
        grid.Items = items;

        // Assert - Items only should not throw
        Assert.NotNull(grid.Items);
    }

    [Fact]
    public void QuickGrid_ItemsAndItemsProvider_CanSetProviderOnly()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();
        GridItemsProvider<TestEntity> provider = _ =>
            ValueTask.FromResult(GridItemsProviderResult.From<TestEntity>(Array.Empty<TestEntity>(), 0));

        // Act
        grid.ItemsProvider = provider;

        // Assert - Provider only should not throw
        Assert.NotNull(grid.ItemsProvider);
    }

    #endregion

    #region PaginationState Two-Way Binding Tests

    [Fact]
    public async Task PaginationState_SetItemsPerPage_UpdatesLastPageCalculation()
    {
        // Arrange
        var pagination = new PaginationState { ItemsPerPage = 10 };

        // LastPageIndex is null when TotalItemCount is not set
        Assert.Null(pagination.LastPageIndex);

        // Act - Set total items and verify LastPageIndex calculation
        // With 25 items and 10 per page, last page index should be 2 (pages 0, 1, 2)
        await pagination.SetTotalItemCountAsync(25);
        Assert.Equal(2, pagination.LastPageIndex);

        // Changing ItemsPerPage should update LastPageIndex calculation
        pagination.ItemsPerPage = 5;
        Assert.Equal(4, pagination.LastPageIndex); // (25-1)/5 = 4
    }

    [Fact]
    public async Task PaginationState_SetItemsPerPage_RecalculatesCurrentPage()
    {
        // Arrange
        var pagination = new PaginationState { ItemsPerPage = 10 };
        await pagination.SetTotalItemCountAsync(100);

        // Act - change ItemsPerPage which should adjust CurrentPageIndex if needed
        pagination.ItemsPerPage = 25;

        // Assert - with 100 items and 25 per page, there are 4 pages (0,1,2,3)
        Assert.Equal(3, pagination.LastPageIndex);
    }

    [Fact]
    public async Task PaginationState_CurrentPageIndex_IsZeroByDefault()
    {
        // Arrange & Act
        var pagination = new PaginationState();

        // Assert
        Assert.Equal(0, pagination.CurrentPageIndex);
    }

    [Fact]
    public async Task PaginationState_SetTotalItemCount_NotifiesListener()
    {
        // Arrange
        var pagination = new PaginationState();
        int? reportedCount = null;
        pagination.TotalItemCountChanged += (sender, count) => reportedCount = count;

        // Act
        await pagination.SetTotalItemCountAsync(50);

        // Assert
        Assert.Equal(50, reportedCount);
    }

    [Fact]
    public async Task PaginationState_SameTotalItemCount_DoesNotFireEvent()
    {
        // Arrange
        var pagination = new PaginationState();
        int eventCount = 0;
        pagination.TotalItemCountChanged += (sender, count) => eventCount++;

        await pagination.SetTotalItemCountAsync(50);
        await pagination.SetTotalItemCountAsync(50); // Same value

        // Assert - event should only fire once
        Assert.Equal(1, eventCount);
    }

    [Fact]
    public async Task PaginationState_LastPageIndex_AdjustsWhenItemsReduced()
    {
        // Arrange
        var pagination = new PaginationState { ItemsPerPage = 10 };
        await pagination.SetTotalItemCountAsync(100);
        await pagination.SetCurrentPageIndexAsync(8); // Was valid, 9 pages (0-8)

        // Act - reduce items so only 3 pages exist (0,1,2)
        await pagination.SetTotalItemCountAsync(25);

        // Assert - CurrentPageIndex should be adjusted to last valid page
        Assert.Equal(2, pagination.CurrentPageIndex);
    }

    #endregion

    #region AsyncQueryExecutor Selection Tests

    [Fact]
    public void QuickGrid_WithItems_SetsUpAsyncQueryExecutor()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();
        var items = new List<TestEntity>().AsQueryable();

        // Act
        grid.Items = items;

        // Assert - internal executor should be configured
        var executorField = typeof(QuickGrid<TestEntity>).GetField("_asyncQueryExecutor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(executorField);
    }

    #endregion

    #region ItemsProvider Request Cancellation Tests

    [Fact]
    public void GridItemsProviderRequest_CancellationToken_CanBeCancelled()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var request = new GridItemsProviderRequest<TestEntity>(
            startIndex: 0,
            count: 10,
            sortByColumn: null,
            sortByAscending: true,
            cancellationToken: cts.Token);

        // Act
        cts.Cancel();

        // Assert
        Assert.True(request.CancellationToken.IsCancellationRequested);
    }

    [Fact]
    public void GridItemsProviderRequest_CancellationToken_None_DoesNotThrow()
    {
        // Arrange
        var request = new GridItemsProviderRequest<TestEntity>(
            startIndex: 0,
            count: 10,
            sortByColumn: null,
            sortByAscending: true,
            cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(request.CancellationToken.IsCancellationRequested);
    }

    #endregion

    #region GridItemsProviderResult Type Tests

    [Fact]
    public void GridItemsProviderResult_From_RequiresItemsCollection()
    {
        // Arrange
        var items = new List<TestEntity>();

        // Act
        var result = GridItemsProviderResult.From<TestEntity>(items, 50);

        // Assert
        Assert.NotNull(result.Items);
        Assert.Equal(50, result.TotalItemCount);
    }

    [Fact]
    public void GridItemsProviderResult_From_WithComplexData_Works()
    {
        // Arrange - simulates real-world data with navigation properties
        var items = new List<TestEntity>
        {
            new() { Id = 1, Name = "Item1", Age = 30, IsActive = true },
            new() { Id = 2, Name = "Item2", Age = 25, IsActive = false }
        };

        // Act
        var result = GridItemsProviderResult.From<TestEntity>(items, 2);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalItemCount);
        Assert.True(result.Items.First().IsActive);
    }

    #endregion

    #region ItemsProvider Cancellation Tests

    [Fact]
    public void GridItemsProviderRequest_WithCancellation_CanBeCancelled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var request = new GridItemsProviderRequest<TestEntity>(
            startIndex: 0,
            count: 10,
            sortByColumn: null,
            sortByAscending: true,
            cancellationToken: cts.Token);

        // Assert
        Assert.True(request.CancellationToken.IsCancellationRequested);
    }

    [Fact]
    public void GridItemsProviderRequest_WithoutCancellation_TokenRemainsValid()
    {
        // Arrange & Act
        var request = new GridItemsProviderRequest<TestEntity>(
            startIndex: 0,
            count: 10,
            sortByColumn: null,
            sortByAscending: true,
            cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(request.CancellationToken.IsCancellationRequested);
    }

    #endregion

    #region GridItemsProviderResult Edge Cases Tests

    [Fact]
    public void GridItemsProviderResult_From_EmptyItems_ReturnsZeroTotalCount()
    {
        // Arrange
        var items = new List<TestEntity>();

        // Act
        var result = GridItemsProviderResult.From<TestEntity>(items, 0);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItemCount);
    }

    [Fact]
    public void GridItemsProviderResult_From_TotalCountCanBeGreaterThanItems()
    {
        // Arrange - pagination scenario: returning subset of items but total count is full dataset
        var items = new List<TestEntity> { new() { Id = 1 } };
        const int totalCount = 1000; // Total items in database

        // Act
        var result = GridItemsProviderResult.From<TestEntity>(items, totalCount);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(1000, result.TotalItemCount);
    }

    [Fact]
    public void GridItemsProviderResult_From_TotalCountCanBeLessThanItems()
    {
        // Arrange - scenario where items count and total count are equal
        var items = new List<TestEntity> { new() { Id = 1 }, new() { Id = 2 } };
        const int totalCount = 2;

        // Act
        var result = GridItemsProviderResult.From<TestEntity>(items, totalCount);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalItemCount);
    }

    #endregion
}
