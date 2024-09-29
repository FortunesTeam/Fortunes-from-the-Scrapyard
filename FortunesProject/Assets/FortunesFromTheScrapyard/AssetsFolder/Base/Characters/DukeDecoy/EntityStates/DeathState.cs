using FortunesFromTheScrapyard;
using MSU;
using MSU.Config;
using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using FortunesFromTheScrapyard.Characters.DukeDecoy.Components;
using FortunesFromTheScrapyard.Characters.DukeDecoy;

namespace EntityStates.DukeDecoy
{
    public class DeathState : GenericCharacterDeath
    {
        [SerializeField]
        public static GameObject deathEffectPrefab = FortunesFromTheScrapyard.Characters.DukeDecoy.DukeDecoy.dukeDecoyDeathExplosion;

        public float smallHopVelocity = 0.35f;

        private bool blewUp;

        private DukeDecoyExplosion decoyExplosion;

        public float duration = 0.2f;

        public static string deathSoundString;

        public override void OnEnter()
        {
            decoyExplosion = base.gameObject.GetComponent<DukeDecoyExplosion>();
            base.OnEnter();
            SmallHop(base.characterMotor, smallHopVelocity);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (NetworkServer.active && !blewUp && decoyExplosion && base.fixedAge > duration)
            {
                BlastAttack blastAttack = new BlastAttack();

                blastAttack.procCoefficient = 1f;
                blastAttack.attacker = decoyExplosion.ownerBody.gameObject;
                blastAttack.inflictor = null;
                blastAttack.teamIndex = decoyExplosion.ownerBody.teamComponent.teamIndex;
                blastAttack.baseDamage = decoyExplosion.ownerBody.damage * decoyExplosion.damageCoefficient;
                blastAttack.baseForce = 500f;
                blastAttack.crit = decoyExplosion.isCrit;
                blastAttack.position = decoyExplosion.decoyBody.corePosition;
                blastAttack.radius = 20;
                blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                // blastAttack.bonusForce = Vector3.zero;
                blastAttack.damageType = DamageType.Stun1s;
                blastAttack.damageColorIndex = DamageColorIndex.Default;

                blastAttack.Fire();

                EffectManager.SpawnEffect(deathEffectPrefab, new EffectData
                {
                    origin = base.characterBody.corePosition,
                    scale = 3f
                }, transmit: true);
                Util.PlaySound(deathSoundString, base.gameObject);
                blewUp = true;

                DestroyBodyAsapServer();
            }
        }

        public override void OnExit()
        {
            DestroyModel();
            base.OnExit();
        }
    }
}