namespace GunDecorator
{
    public interface IShootModule
    {
        public void TryShoot();
        public void Shooting();
    }
    
    public interface IReloadModule
    {
        public void Reload();
    }
    
    public interface INoiseModule
    {
        public void ApplyNoise();
    }

    public interface ISecondModule
    {
        public void SetUpModule(IShootModule shootModule);
        public void SetNext(ISecondModule next);
        public void DoAdditionnalEffect();
        public void Shooting();
    }
}