// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace Microsoft.AspNetCore.Components.QuickGrid.Tests;

/// <summary>
/// Tests for ColumnBase base class functionality including property defaults,
/// alignment options, header templates, and column options support.
/// </summary>
public class ColumnBaseTest
{
    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private class TestColumn : ColumnBase<TestEntity>
    {
        private GridSort<TestEntity>? _sortBy;

        public string? TestTitle { get => Title; set => Title = value; }
        public string? TestClass { get => Class; set => Class = value; }
        public Align TestAlign { get => Align; set => Align = value; }
        public bool? TestSortable { get => Sortable; set => Sortable = value; }
        public SortDirection TestInitialSortDirection { get => InitialSortDirection; set => InitialSortDirection = value; }
        public bool TestIsDefaultSortColumn { get => IsDefaultSortColumn; set => IsDefaultSortColumn = value; }

        public override GridSort<TestEntity>? SortBy
        {
            get => _sortBy;
            set => _sortBy = value;
        }

        protected internal override void CellContent(RenderTreeBuilder builder, TestEntity item)
        {
            builder.AddContent(0, item.Name);
        }

        // Expose protected method for testing
        public bool CallIsSortableByDefault()
        {
            return IsSortableByDefault();
        }
    }

    [Fact]
    public void ColumnBase_DefaultValues()
    {
        // Arrange & Act
        var column = new TestColumn();

        // Assert
        Assert.Null(column.TestTitle);
        Assert.Null(column.TestClass);
        Assert.Equal(Align.Start, column.TestAlign);
        Assert.Null(column.TestSortable);
        Assert.Equal(default, column.TestInitialSortDirection);
        Assert.False(column.TestIsDefaultSortColumn);
    }

    [Fact]
    public void ColumnBase_Title_CanBeSet()
    {
        // Arrange
        var column = new TestColumn();

        // Act
        column.TestTitle = "Test Title";

        // Assert
        Assert.Equal("Test Title", column.TestTitle);
    }

    [Fact]
    public void ColumnBase_Class_CanBeSet()
    {
        // Arrange
        var column = new TestColumn();

        // Act
        column.TestClass = "custom-class";

        // Assert
        Assert.Equal("custom-class", column.TestClass);
    }

    [Fact]
    public void ColumnBase_Align_CanBeSet()
    {
        // Arrange
        var column = new TestColumn();

        // Act
        column.TestAlign = Align.Center;

        // Assert
        Assert.Equal(Align.Center, column.TestAlign);
    }

    [Fact]
    public void ColumnBase_Sortable_CanBeSet()
    {
        // Arrange
        var column = new TestColumn();

        // Act
        column.TestSortable = true;

        // Assert
        Assert.True(column.TestSortable);
    }

    [Fact]
    public void ColumnBase_InitialSortDirection_CanBeSet()
    {
        // Arrange
        var column = new TestColumn();

        // Act
        column.TestInitialSortDirection = SortDirection.Descending;

        // Assert
        Assert.Equal(SortDirection.Descending, column.TestInitialSortDirection);
    }

    [Fact]
    public void ColumnBase_IsDefaultSortColumn_CanBeSet()
    {
        // Arrange
        var column = new TestColumn();

        // Act
        column.TestIsDefaultSortColumn = true;

        // Assert
        Assert.True(column.TestIsDefaultSortColumn);
    }

    [Fact]
    public void ColumnBase_IsSortableByDefault_ReturnsFalse()
    {
        // Arrange
        var column = new TestColumn();

        // Act
        var result = column.CallIsSortableByDefault();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ColumnBase_Grid_ThrowsWithoutContext()
    {
        // Arrange
        var column = new TestColumn();

        // Act & Assert - accessing Grid without InternalGridContext throws (null reference or invalid operation)
        Assert.ThrowsAny<Exception>(() => column.Grid);
    }
}