namespace Game.Systems.CollectibleSystem
{
    public interface ICollectible
    {
        CollectibleData CollectibleData { get; }

        void Collect();
    }
}
