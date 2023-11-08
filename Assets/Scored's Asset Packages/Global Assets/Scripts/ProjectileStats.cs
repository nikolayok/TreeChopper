using UnityEngine;

namespace ScoredProductions.Global
{
	public class ProjectileStats : MonoBehaviour
	{
		[Range(0, 1000)]
		public float Damage;

		void OnCollisionEnter(Collision other) {
			DamageInterface x = other.gameObject.GetComponent<DamageInterface>();
			if (x != null) {
				x.ReceiveDamage(Damage);
			}
			Destroy(gameObject);
		}
	}
}