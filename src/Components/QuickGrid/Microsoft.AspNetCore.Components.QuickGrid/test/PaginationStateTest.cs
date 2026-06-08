// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;
using Microsoft.AspNetCore.Components;

namespace Microsoft.AspNetCore.Components.QuickGrid.Tests;

/// <summary>
/// Tests for PaginationState functionality including page navigation,
/// TotalItemCount tracking, LastPageIndex calculation, and event handling.
/// </summary>
public class PaginationStateTest
{
    [Fact]
    public void Default_ItemsPerPage_IsTen()
    {
        // Arrange & Act
        var pagination = new PaginationState();

        // Assert
        Assert.Equal(10, pagination.ItemsPerPage);
    }

    [Fact]
    public void Default_CurrentPageIndex_IsZero()
    {
        // Arrange & Act
        var pagination = new PaginationState();

        // Assert
        Assert.Equal(0, pagination.CurrentPageIndex);
    }

    [Fact]
    public void Default_TotalItemCount_IsNull()
    {
        // Arrange & Act
        var pagination = new PaginationState();

        // Assert
        Assert.Null(pagination.TotalItemCount);
    }

    [Fact]
    public void Default_LastPageIndex_IsNull_WhenNoTotalItemCount()
    {
        // Arrange & Act
        var pagination = new PaginationState();

        // Assert
        Assert.Null(pagination.LastPageIndex);
    }

    [Fact]
    public async Task SetCurrentPageIndexAsync_UpdatesCurrentPageIndex()
    {
        // Arrange
        var pagination = new PaginationState();

        // Act
        await pagination.SetCurrentPageIndexAsync(3);

        // Assert
        Assert.Equal(3, pagination.CurrentPageIndex);
    }

    [Fact]
    public async Task SetCurrentPageIndexAsync_NegativePageIndex_Allowed()
    {
        // Arrange
        var pagination = new PaginationState();

        // Act - Should not throw
        await pagination.SetCurrentPageIndexAsync(-1);

        // Assert
        Assert.Equal(-1, pagination.CurrentPageIndex);
    }

    [Fact]
    public async Task SetCurrentPageIndexAsync_LargePageIndex_Allowed()
    {
        // Arrange
        var pagination = new PaginationState();

        // Act
        await pagination.SetCurrentPageIndexAsync(999);

        // Assert
        Assert.Equal(999, pagination.CurrentPageIndex);
    }

    [Fact]
    public async Task SetTotalItemCountAsync_UpdatesTotalItemCount()
    {
        // Arrange
        var pagination = new PaginationState();

        // Act
        await pagination.SetTotalItemCountAsync(100);

        // Assert
        Assert.Equal(100, pagination.TotalItemCount);
    }

    [Fact]
    public async Task SetTotalItemCountAsync_Zero_SetsTotalItemCount()
    {
        // Arrange
        var pagination = new PaginationState();

        // Act
        await pagination.SetTotalItemCountAsync(0);

        // Assert
        Assert.Equal(0, pagination.TotalItemCount);
    }

    [Fact]
    public async Task SetTotalItemCountAsync_RaisesTotalItemCountChangedEvent()
    {
        // Arrange
        var pagination = new PaginationState();
        var capturedCount = -1;
        pagination.TotalItemCountChanged += (_, count) => capturedCount = count ?? -1;

        // Act
        await pagination.SetTotalItemCountAsync(50);

        // Assert
        Assert.Equal(50, capturedCount);
    }

    [Fact]
    public async Task SetTotalItemCountAsync_UpdatesLastPageIndex()
    {
        // Arrange
        var pagination = new PaginationState() { ItemsPerPage = 10 };

        // Act
        await pagination.SetTotalItemCountAsync(95);

        // Assert - (95 - 1) / 10 = 9
        Assert.Equal(9, pagination.LastPageIndex);
    }

    [Fact]
    public async Task SetTotalItemCountAsync_ExactlyDivisible_ReturnsCorrectIndex()
    {
        // Arrange
        var pagination = new PaginationState() { ItemsPerPage = 10 };

        // Act
        await pagination.SetTotalItemCountAsync(100);

        // Assert - (100 - 1) / 10 = 9
        Assert.Equal(9, pagination.LastPageIndex);
    }

    [Fact]
    public async Task SetTotalItemCountAsync_SinglePage_ReturnsZero()
    {
        // Arrange
        var pagination = new PaginationState() { ItemsPerPage = 10 };

        // Act
        await pagination.SetTotalItemCountAsync(5);

        // Assert - (5 - 1) / 10 = 0
        Assert.Equal(0, pagination.LastPageIndex);
    }

    [Fact]
    public async Task SetTotalItemCountAsync_ReducesItemsBelowCurrentPage_AdjustsToLastPage()
    {
        // Arrange
        var pagination = new PaginationState() { ItemsPerPage = 10 };
        await pagination.SetCurrentPageIndexAsync(8);
        await pagination.SetTotalItemCountAsync(50); // Last page is 4, but current is 8

        // Assert - Current page should auto-adjust to last valid page (4)
        Assert.Equal(4, pagination.CurrentPageIndex);
    }

    [Fact]
    public async Task SetTotalItemCountAsync_SameValue_NoEvent()
    {
        // Arrange
        var pagination = new PaginationState();
        await pagination.SetTotalItemCountAsync(100);

        var eventCount = 0;
        pagination.TotalItemCountChanged += (_, _) => eventCount++;

        // Act - Set same value again
        await pagination.SetTotalItemCountAsync(100);

        // Assert - No event should fire
        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void GetHashCode_IncludesItemsPerPage()
    {
        // Arrange
        var pagination1 = new PaginationState { ItemsPerPage = 10 };
        var pagination2 = new PaginationState { ItemsPerPage = 20 };

        // Act
        var hash1 = pagination1.GetHashCode();
        var hash2 = pagination2.GetHashCode();

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public async Task SetCurrentPageIndexAsync_SamePage_NoChange()
    {
        // Arrange
        var pagination = new PaginationState();
        await pagination.SetCurrentPageIndexAsync(2);

        // Act - Set same page again (no exception means success as the method just updates the index)
        await pagination.SetCurrentPageIndexAsync(2);

        // Assert - Should still be 2
        Assert.Equal(2, pagination.CurrentPageIndex);
    }

    [Fact]
    public async Task SetTotalItemCountAsync_LargeTotalItemCount()
    {
        // Arrange
        var pagination = new PaginationState { ItemsPerPage = 10 };

        // Act
        await pagination.SetTotalItemCountAsync(int.MaxValue);

        // Assert
        Assert.Equal(int.MaxValue, pagination.TotalItemCount);
        Assert.Equal((int.MaxValue - 1) / 10, pagination.LastPageIndex);
    }

    [Fact]
    public async Task SetTotalItemCountAsync_OneItem_PageIndexZero()
    {
        // Arrange
        var pagination = new PaginationState { ItemsPerPage = 10 };

        // Act
        await pagination.SetTotalItemCountAsync(1);

        // Assert - (1 - 1) / 10 = 0
        Assert.Equal(0, pagination.LastPageIndex);
    }
}