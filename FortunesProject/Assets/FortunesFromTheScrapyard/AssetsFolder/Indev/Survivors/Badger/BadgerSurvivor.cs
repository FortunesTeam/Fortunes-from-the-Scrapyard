using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using RoR2.UI;
using EntityStates;
using RoR2.Projectile;
using RoR2.EntityLogic;
using System.Runtime.CompilerServices;
using ThreeEyedGames;
using EmotesAPI;
using RoR2.Skills;
using EntityStates.Badger.Components;
using FortunesFromTheScrapyard;
<<<<<<< Updated upstream
=======
using MSU.Config;
using EntityStates.AffixVoid;
>>>>>>> Stashed changes

namespace FortunesFromTheScrapyard.Survivors.Badger
{
    public class BadgerSurvivor : ScrapyardSurvivor
    {
        // lol I have no CLUE what I'm doing, If ANY of this looks right it by the grace of God or Kenko - EZ


        // DamageTypes
        public static DamageAPI.ModdedDamageType BadgerExplode;


        //Projectiles
        internal static GameObject diskPrefab;
        internal static GameObject soundScape;
        internal static GameObject soundWave;

        public override void Initialize()
        {
            Hooks();

            BodyCatalog.availability.CallWhenAvailable(CreateProjectiles);

            

            BadgerExplode = DamageAPI.ReserveDamageType();

            ModifyPrefab();


        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest<SurvivorAssetCollection> LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<SurvivorAssetCollection>("acBadger", ScrapyardBundle.Indev);
        }

        public void ModifyPrefab()
        {
            var cb = characterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
        }

        private static void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;

            On.RoR2.HealthComponent.TakeDamage += new On.RoR2.HealthComponent.hook_TakeDamage(HealthComponent_TakeDamageProcess);

<<<<<<< Updated upstream
=======
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

            Emotes();

            /* if (ScrapyardMain.emotesInstalled)
            {
                Emotes();
            } */
>>>>>>> Stashed changes
        }
        #region projectiles
        private static void CreateProjectiles()
        {
<<<<<<< Updated upstream
=======
            soundScape = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMineAltDetonated.prefab").WaitForCompletion().InstantiateClone("soundBuffZone");
            if (!soundScape.GetComponent<NetworkIdentity>()) soundScape.AddComponent<NetworkIdentity>();
            BuffWard buffWard = soundScape.GetComponent<BuffWard>();
            buffWard.radius = 2.5f;
            buffWard.interval = 0.01f;
            buffWard.buffDef = ScrapyardContent.Buffs.bdBadgerSoundBuff;
            buffWard.expires = true;
            buffWard.expireDuration = 5f;
            buffWard.invertTeamFilter = false;

            soundScape.GetComponent<SphereCollider>().radius = 2.5f;

            ScrapyardContent.scrapyardContentPack.projectilePrefabs.AddSingle(soundScape);

            soundWave = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageIceBombProjectile.prefab").WaitForCompletion().InstantiateClone("soundBuffProjectile");
            if (!soundWave.GetComponent<NetworkIdentity>()) soundWave.AddComponent<NetworkIdentity>();

            ProjectileOverlapAttack overlapAttack = soundWave.GetComponent<ProjectileOverlapAttack>();
            overlapAttack.damageCoefficient = 1f;
            overlapAttack.impactEffect = null;

            ProjectileSingleTargetImpact singleTargetImpact = soundWave.GetComponent<ProjectileSingleTargetImpact>();
            singleTargetImpact.impactEffect = null;
            singleTargetImpact.destroyOnWorld = false;

            ProjectileDamage damage = soundWave.GetComponent<ProjectileDamage>();
            damage.damageType = DamageType.Generic;

            ProjectileSimple simple = soundWave.GetComponent<ProjectileSimple>();
            simple.lifetime = 5f;

            ProjectileController controller = soundWave.GetComponent<ProjectileController>();
            controller.ghostPrefab = null;

            soundWave.AddComponent<SoundWaveController>();

            ScrapyardContent.scrapyardContentPack.projectilePrefabs.AddSingle(soundWave);

>>>>>>> Stashed changes
            diskPrefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2ShivProjectile.prefab").WaitForCompletion().InstantiateClone("BadgerDisk");
            if (!diskPrefab.GetComponent<NetworkIdentity>()) diskPrefab.AddComponent<NetworkIdentity>();
            diskPrefab.GetComponent<ProjectileDamage>().damageType = DamageType.CrippleOnHit;
            Component.Destroy(diskPrefab.GetComponent<ProjectileSingleTargetImpact>());

            // ProjectileSimple diskSimple = diskPrefab.GetComponent<ProjectileSimple>();

            ProjectileImpactExplosion diskEX = diskPrefab.AddComponent<ProjectileImpactExplosion>();

            diskEX.blastRadius = 12;
            diskEX.canRejectForce = false;
            diskEX.blastDamageCoefficient = 1;
            diskEX.blastProcCoefficient = 1;
            // diskEX.impactEffect = null; // used interrorgators impack vfx so it's probs gonna need something unique
            diskEX.destroyOnEnemy = true;
            diskEX.destroyOnWorld = true;
            diskEX.impactOnWorld = false;
            diskEX.lifetime = 5f;

<<<<<<< Updated upstream
=======
            diskGhost = assetCollection.FindAsset<GameObject>("BadgerDiskGhost");

            diskPrefab.GetComponent<ProjectileController>().ghostPrefab = diskGhost;

            diskExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/OmniImpactVFXLightningMage.prefab").WaitForCompletion().InstantiateClone("BadgerDiskExplosion");

            diskEX.explosionEffect = diskExplosion;
            diskEX.impactEffect = diskExplosion;

>>>>>>> Stashed changes
            ScrapyardContent.scrapyardContentPack.projectilePrefabs.AddSingle(diskPrefab);

            soundScape = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMineAltDetonated.prefab").WaitForCompletion().InstantiateClone("soundBuffZone", false);

            BuffWard buffWard = soundScape.GetComponent<BuffWard>();
            buffWard.radius = 2.5f;
            buffWard.interval = 0.01f;
            buffWard.buffDef = ScrapyardContent.Buffs.bdBadgerSoundBuff;
            buffWard.expires = true;
            buffWard.expireDuration = 5f;
            buffWard.invertTeamFilter = false;

            soundScape.GetComponent<SphereCollider>().radius = 2.5f;

            ScrapyardContent.scrapyardContentPack.projectilePrefabs.AddSingle(soundScape);

            soundWave = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageIceBombProjectile.prefab").WaitForCompletion().InstantiateClone("soundBuffProjectile", false);

            ProjectileOverlapAttack overlapAttack = soundWave.GetComponent<ProjectileOverlapAttack>();
            overlapAttack.damageCoefficient = 0f;
            overlapAttack.impactEffect = null;

            ProjectileSingleTargetImpact singleTargetImpact = soundWave.GetComponent<ProjectileSingleTargetImpact>();
            singleTargetImpact.impactEffect = null;

            ProjectileDamage damage = soundWave.GetComponent<ProjectileDamage>();
            damage.damageType = DamageType.Generic;

            ProjectileSimple simple = soundWave.GetComponent<ProjectileSimple>();
            simple.lifetime = 5f;

            ProjectileController controller = soundWave.GetComponent<ProjectileController>();
            controller.ghostPrefab = null;

            soundWave.AddComponent<SoundWaveController>();

            ScrapyardContent.scrapyardContentPack.projectilePrefabs.AddSingle(soundWave);
        }
        #endregion

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            if (self)
            {
                if (self.HasBuff(ScrapyardContent.Buffs.bdBadgerSoundBuff))
                {
                    self.moveSpeed *= 1.25f;
                    self.attackSpeed *= 1.35f;
                }

                if (self.HasBuff(ScrapyardContent.Buffs.bdBadgerSlowBuff))
                {
                    self.moveSpeed *= 0.5f;
                }
            }
        }

<<<<<<< Updated upstream
        private static void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)

=======
        private void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
>>>>>>> Stashed changes
        {
            if (NetworkServer.active && self.alive || !self.godMode || self.ospTimer <= 0f)
            {
                CharacterBody victimBody = self.body;
                CharacterMotor victimMotor = null;
                CharacterBody attackerBody = null;

                if (self.body.characterMotor)
                {
                    victimMotor = self.body.characterMotor;
                }

                if (damageInfo.attacker)
                {
                    attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                }

                if (damageInfo.damage > 0 && !damageInfo.rejected && victimBody && attackerBody)
                {
                    if (victimBody.baseMoveSpeed != 0 && victimMotor && attackerBody.bodyIndex == BodyCatalog.FindBodyIndex("BadgerBody"))

                    {
                        if (!victimBody.isBoss && !victimBody.isChampion && victimBody.baseNameToken != "GOLEM_BODY_NAME" && victimMotor.velocity.magnitude <= 0)
                        {
                            damageInfo.damage *= 2f;
                        }
                        else if (victimBody.moveSpeed <= victimBody.baseMoveSpeed)
                        {
                            float failSafeMoveSpeed = victimBody.moveSpeed;

                            if (failSafeMoveSpeed < 0) failSafeMoveSpeed = 0;

                            float calc = 1 - (failSafeMoveSpeed / (victimBody.baseMoveSpeed + victimBody.levelMoveSpeed * victimBody.level));

                            damageInfo.damage *= Util.Remap(calc, 0, 1, 1, 2);
                        }
                    }
                }
            }

            orig.Invoke(self, damageInfo);
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
        {
            DamageInfo damageInfo = damageReport.damageInfo;
            if (!damageReport.attackerBody || !damageReport.victimBody)
            {
                return;
            }
            HealthComponent victim = damageReport.victim;
            GameObject inflictorObject = damageInfo.inflictor;
            CharacterBody victimBody = damageReport.victimBody;
            EntityStateMachine victimMachine = victimBody.GetComponent<EntityStateMachine>();
            CharacterBody attackerBody = damageReport.attackerBody;
            GameObject attackerObject = damageReport.attacker.gameObject;
            BadgerController iController = attackerBody.GetComponent<BadgerController>();
            if (NetworkServer.active)
            {
                if (iController && attackerBody.bodyIndex == BodyCatalog.FindBodyIndex("BadgerBody"))

                {
                    if (damageInfo.HasModdedDamageType(BadgerExplode))
                    {
                        // attackerBody.AddTimedBuff(bdBadgerArmorbuff) thinkin of giving him temporary armor for hitting it idk

                        attackerBody.healthComponent.Heal(attackerBody.maxHealth * 0.02f, new ProcChainMask());

                        victimBody.AddTimedBuff(ScrapyardContent.Buffs.bdBadgerSlowBuff, 3f);
                    }
                }
            }
        }
    }
}