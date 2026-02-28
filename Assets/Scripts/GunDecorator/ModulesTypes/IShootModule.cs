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
        public void SetUpModule(IShootModule moduleShoot);
        public void DoAdditionnalEffect();
    }
}