using com.jimmychoi.shootingGame.ui.utility;

namespace com.jimmychoi.shootingGame.Weapon
{
    public class CommonBullet : WeaponBase
    {
        public int speed = 10;
        public float lifeTime = 1;
        public int power = 1;

        public CommonBullet(PrefabManasablePool pool)
        {
            base.bulletPool = pool;
        }
    }
}