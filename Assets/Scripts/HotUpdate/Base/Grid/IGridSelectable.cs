namespace HotUpdate.Base.Grid
{
    public interface IGridSelectable<out T> : IGridInteractive<T>
    {
        bool Selected { get; set; }
    }
}
