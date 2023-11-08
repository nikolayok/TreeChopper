using UnityEngine;

namespace ScoredProductions.Global
{
	public class ShootScript : MonoBehaviour
	{
		public GameObject Projectile;
		public Transform ShootPoint;

		[Range(1, 99)]
		public int ProjectileSpeed;

		// Update is called once per frame
		void Update() {
			if (Input.GetMouseButtonDown(0)) {
				GameObject NewProjectile = Instantiate(Projectile, ShootPoint.position, ShootPoint.rotation);
				if (NewProjectile.GetComponent<Rigidbody>() == null) {
					NewProjectile.AddComponent<Rigidbody>();
				}
				NewProjectile.GetComponent<Rigidbody>().velocity = ShootPoint.forward * ProjectileSpeed;
				Destroy(NewProjectile, 10);
			}
		}
	}
}