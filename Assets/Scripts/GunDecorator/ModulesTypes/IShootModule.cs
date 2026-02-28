namespace GunDecorator
{
    public interface IShootModule
    {
        public void Shoot();
    }
    
    public interface IReloadModule
    {
        public void Reload();
    }
    
    public interface INoise
    {
        public void ApplyNoise();
    }

    public interface ISecondModule
    {
        public void SetUpModule(IShootModule moduleShoot);
        public void DoAdditionnalEffect();
    }
}