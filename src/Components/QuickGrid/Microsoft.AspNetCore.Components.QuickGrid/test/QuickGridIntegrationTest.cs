// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Reflection;
using Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;

namespace Microsoft.AspNetCore.Components.QuickGrid.Tests;

/// <summary>
/// Integration tests for QuickGrid component covering public API contracts,
/// Items/ItemsProvider conflict detection, and property defaults.
/// </summary>
public class QuickGridIntegrationTest
{
    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    [Fact]
    public void QuickGrid_ItemsAndItemsProvider_BothSet_ThrowsInvalidOperation()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();
        var items = new List<TestEntity> { new TestEntity { Name = "Test", Age = 25 } }.AsQueryable();
        GridItemsProvider<TestEntity> itemsProvider = _ => ValueTask.FromResult(GridItemsProviderResult.From<TestEntity>(Array.Empty<TestEntity>(), 0));

        // Act & Assert - the conflict is detected when OnParametersSetAsync is called
        // We use a separate try-catch because reflection wraps exceptions
        grid.Items = items;
        grid.ItemsProvider = itemsProvider;

        var method = typeof(QuickGrid<TestEntity>).GetMethod("OnParametersSetAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);

        Exception? actualException = null;
        try
        {
            method.Invoke(grid, null);
        }
        catch (Exception ex)
        {
            actualException = ex.InnerException ?? ex;
        }

        Assert.NotNull(actualException);
        var ioe = Assert.IsType<InvalidOperationException>(actualException);
        Assert.Contains("Items", ioe.Message);
        Assert.Contains("ItemsProvider", ioe.Message);
    }

    [Fact]
    public void QuickGrid_ItemsAndItemsProvider_BothSet_ConflictMessage()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();
        var items = new List<TestEntity> { new TestEntity { Name = "Test", Age = 25 } }.AsQueryable();
        GridItemsProvider<TestEntity> itemsProvider = _ => ValueTask.FromResult(GridItemsProviderResult.From<TestEntity>(Array.Empty<TestEntity>(), 0));

        // Act & Assert - exception message should indicate mutual exclusivity
        grid.Items = items;
        grid.ItemsProvider = itemsProvider;

        var method = typeof(QuickGrid<TestEntity>).GetMethod("OnParametersSetAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);

        Exception? actualException = null;
        try
        {
            method.Invoke(grid, null);
        }
        catch (Exception ex)
        {
            actualException = ex.InnerException ?? ex;
        }

        Assert.NotNull(actualException);
        var ioe = Assert.IsType<InvalidOperationException>(actualException);
        Assert.Contains("both were specified", ioe.Message, StringComparison.OrdinalIgnoreCase);
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

    [Fact]
    public void QuickGrid_Virtualization_CanBeSet()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();

        // Act
        grid.Virtualize = true;
        grid.ItemSize = 75;
        grid.OverscanCount = 5;

        // Assert
        Assert.True(grid.Virtualize);
        Assert.Equal(75, grid.ItemSize);
        Assert.Equal(5, grid.OverscanCount);
    }

    [Fact]
    public void QuickGrid_Class_CanBeSet()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();

        // Act
        grid.Class = "custom-grid-class";

        // Assert
        Assert.Equal("custom-grid-class", grid.Class);
    }

    [Fact]
    public void QuickGrid_Theme_DefaultValue_IsDefault()
    {
        // Arrange
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
    public void QuickGrid_QueryParameterNamePrefix_DefaultValue()
    {
        // Arrange
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
    public void QuickGrid_ItemKey_CanBeSet()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();

        // Act
        grid.ItemKey = item => item.Name;

        // Assert
        Assert.NotNull(grid.ItemKey);
    }

    [Fact]
    public void QuickGrid_DefaultItemKey_ReturnsItem()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();
        var entity = new TestEntity { Name = "Test", Age = 25 };

        // Act
        var key = grid.ItemKey(entity);

        // Assert - default returns the item itself
        Assert.Same(entity, key);
    }

    [Fact]
    public void QuickGrid_RowClass_CanBeSet()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();

        // Act
        grid.RowClass = item => item.Age > 18 ? "adult" : "minor";

        // Assert
        Assert.NotNull(grid.RowClass);
    }

    [Fact]
    public void QuickGrid_SortByColumnAsync_MethodExists()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();

        // Assert - we can at least verify the grid has this method
        Assert.True(grid.GetType().GetMethod("SortByColumnAsync") != null);
    }

    [Fact]
    public void QuickGrid_RefreshDataAsync_MethodExists()
    {
        // Arrange
        var grid = new QuickGrid<TestEntity>();

        // Assert - verify the method exists
        Assert.True(grid.GetType().GetMethod("RefreshDataAsync") != null);
    }
}