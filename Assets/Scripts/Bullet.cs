using UnityEngine;
using com.jimmychoi.shootingGame.Weapon;

public class Bullet : WeaponBase
{
	// 弾の移動スピード
	public int speed = 10;

	// ゲームオブジェクト生成から削除するまでの時間
	public float lifeTime = 1;

	// 攻撃力
	public int power = 1;
}