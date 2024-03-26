using BattleGame.Items.Bombs;
using UnityEngine;

namespace BattleGame.Items {
    public interface IExplosable {
        public void Explode(Explosion explosion);
    }
}
