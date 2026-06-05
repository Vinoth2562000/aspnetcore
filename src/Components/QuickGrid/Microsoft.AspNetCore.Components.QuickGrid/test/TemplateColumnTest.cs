// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;

namespace Microsoft.AspNetCore.Components.QuickGrid.Tests;

public class TemplateColumnTest
{
    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime CreatedDate { get; set; }
    }

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
    public void TemplateColumn_SortBy_WhenNull_IsNotSortableByDefault()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();

        // Act & Assert - IsSortableByDefault returns false when SortBy is null
        var isSortableByDefaultMethod = typeof(TemplateColumn<TestEntity>).BaseType.GetMethod("IsSortableByDefault", BindingFlags.NonPublic | BindingFlags.Instance);
        var isSortableByDefault = isSortableByDefaultMethod?.Invoke(column, null);
        Assert.False((bool)isSortableByDefault!);
    }

    [Fact]
    public void TemplateColumn_SortBy_WhenSet_IsSortableByDefault()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();
        Expression<Func<TestEntity, string>> sortExpr = x => x.Name;
        var gridSort = GridSort<TestEntity>.ByAscending(sortExpr);

        // Act
        column.SortBy = gridSort;

        // Assert - IsSortableByDefault returns true when SortBy is set
        var isSortableByDefaultMethod = typeof(TemplateColumn<TestEntity>).BaseType.GetMethod("IsSortableByDefault", BindingFlags.NonPublic | BindingFlags.Instance);
        var isSortableByDefault = isSortableByDefaultMethod?.Invoke(column, null);
        Assert.True((bool)isSortableByDefault!);
    }

    [Fact]
    public void TemplateColumn_ChildContent_CanBeSet()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();

        // Act
        column.ChildContent = item => builder => builder.AddContent(0, item.Name);

        // Assert
        Assert.NotNull(column.ChildContent);
    }

    [Fact]
    public void TemplateColumn_Class_CanBeSet()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();

        // Act
        column.Class = "custom-column-class";

        // Assert
        Assert.Equal("custom-column-class", column.Class);
    }

    [Fact]
    public void TemplateColumn_Title_CanBeSet()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();

        // Act
        column.Title = "Custom Title";

        // Assert
        Assert.Equal("Custom Title", column.Title);
    }

    [Fact]
    public void TemplateColumn_Align_CanBeSet()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();

        // Act
        column.Align = Align.Center;

        // Assert
        Assert.Equal(Align.Center, column.Align);
    }

    [Fact]
    public void TemplateColumn_PlaceholderTemplate_CanBeSet()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();

        // Act
        column.PlaceholderTemplate = context => builder => { };

        // Assert
        Assert.NotNull(column.PlaceholderTemplate);
    }

    [Fact]
    public void TemplateColumn_HeaderTemplate_CanBeSet()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();

        // Act
        column.HeaderTemplate = col => builder => { };

        // Assert
        Assert.NotNull(column.HeaderTemplate);
    }

    [Fact]
    public void TemplateColumn_ColumnOptions_CanBeSet()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();

        // Act
        column.ColumnOptions = builder => { };

        // Assert
        Assert.NotNull(column.ColumnOptions);
    }

    [Fact]
    public void TemplateColumn_Sortable_CanBeSet()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();

        // Act
        column.Sortable = true;

        // Assert
        Assert.True(column.Sortable);
    }

    [Fact]
    public void TemplateColumn_InitialSortDirection_CanBeSet()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();

        // Act
        column.InitialSortDirection = SortDirection.Descending;

        // Assert
        Assert.Equal(SortDirection.Descending, column.InitialSortDirection);
    }

    [Fact]
    public void TemplateColumn_IsDefaultSortColumn_CanBeSet()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();

        // Act
        column.IsDefaultSortColumn = true;

        // Assert
        Assert.True(column.IsDefaultSortColumn);
    }

    [Fact]
    public void TemplateColumn_MultipleSortBy_Chained()
    {
        // Arrange
        var column = new TemplateColumn<TestEntity>();
        Expression<Func<TestEntity, string>> nameSort = x => x.Name;
        Expression<Func<TestEntity, int>> ageSort = x => x.Age;

        // Act - chain sort using ThenAscending/ThenDescending
        var nameGridSort = GridSort<TestEntity>.ByAscending(nameSort);
        var ageGridSort = nameGridSort.ThenAscending(ageSort);
        column.SortBy = ageGridSort;

        // Assert
        Assert.NotNull(column.SortBy);
        Assert.NotNull(ageGridSort.ToPropertyList(true)); // Verify it works
    }
}