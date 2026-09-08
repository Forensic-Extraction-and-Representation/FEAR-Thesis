using FEAR.Blazor.Shared.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FEAR.Blazor.Shared.Components
{
    public abstract class DataGridComponentBase<TItem> : ComponentBase
    {
        protected EditorDialog<TItem> editorDialog;
        protected virtual IEnumerable<TItem> Items { get; set; }
        protected string _searchString;
        protected abstract Func<TItem, bool> QuickFilters { get; }

        // events
        protected void RowClicked(DataGridRowClickEventArgs<TItem> args)
        {
        }

        protected void RowRightClicked(DataGridRowClickEventArgs<TItem> args)
        {
        }

        protected void SelectedItemsChanged(HashSet<TItem> items)
        {
        }
    }
}