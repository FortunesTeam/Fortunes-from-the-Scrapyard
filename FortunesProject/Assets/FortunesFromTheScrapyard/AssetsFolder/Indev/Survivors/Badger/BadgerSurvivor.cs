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
using MSU.Config;
using EntityStates;

namespace FortunesFromTheScrapyard.Survivors.Badger
{
    public class BadgerSurvivor : ScrapyardSurvivor
    {
        // lol I have no CLUE what I'm doing, If ANY of this looks right it by the grace of God or Kenko - EZ


        public const string PRIMARYTOKEN = "SCRAPYARD_BADGER_PRIMARY_DESC";
        public const string SECONDARYTOKEN = "SCRAPYARD_BADGER_SECONDARY_DESC";
        public const string UTILITYTOKEN = "SCRAPYARD_BADGER_UTILITY_DESC";
        public const string SPECIALTOKEN = "SCRAPYARD_BADGER_SPECIAL_DESC";

        [ConfigureField(ScrapyardConfig.ID_SURVIVORS)]
        [FormatToken(PRIMARYTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float basePrimaryDamage = 0.6f;

        [ConfigureField(ScrapyardConfig.ID_SURVIVORS)]
        [FormatToken(SECONDARYTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        internal static float baseSecondaryDamage = 0.2f;

        [ConfigureField(ScrapyardConfig.ID_SURVIVORS)]
        [FormatToken(UTILITYTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float baseUtilityDamage = 4f;

        [ConfigureField(ScrapyardConfig.ID_SURVIVORS)]
        [FormatToken(SPECIALTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        public static float baseSpecialDamage = 6f;

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

            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

            BadgerExplode = DamageAPI.ReserveDamageType();

            ModifyPrefab();

            if(ScrapyardMain.emotesInstalled)
            {
                Emotes();
            }
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

        }

        private void Emotes()
        {
            On.RoR2.SurvivorCatalog.Init += (orig) =>
            {
                orig();
                var skele = ScrapyardAssets.GetAssetBundle(ScrapyardBundle.Indev).LoadAsset<GameObject>("badger_emoteskeleton");
                CustomEmotesAPI.ImportArmature(this.characterPrefab, skele);
            };
            CustomEmotesAPI.animChanged += CustomEmotesAPI_animChanged;
        }
        private void CustomEmotesAPI_animChanged(string newAnimation, BoneMapper mapper)
        {
            if (newAnimation != "none")
            {
                if (mapper.transform.name == "badger_emoteskeleton")
                {
                    mapper.transform.parent.Find("meshGun").gameObject.SetActive(value: false);
                }
            }
            else
            {
                if (mapper.transform.name == "badger_emoteskeleton")
                {
                    mapper.transform.parent.Find("meshGun").gameObject.SetActive(value: true);
                }
            }
        }

        #region projectiles
        private static void CreateProjectiles()
        {
            diskPrefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2ShivProjectile.prefab").WaitForCompletion().InstantiateClone("BadgerDisk");
            if (!diskPrefab.GetComponent<NetworkIdentity>()) diskPrefab.AddComponent<NetworkIdentity>();
            diskPrefab.GetComponent<ProjectileDamage>().damageType = DamageType.Stun1s;
            Component.Destroy(diskPrefab.GetComponent<ProjectileSingleTargetImpact>());

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

            ScrapyardContent.scrapyardContentPack.projectilePrefabs.AddSingle(diskPrefab);

            soundScape = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMineAltDetonated.prefab").WaitForCompletion().InstantiateClone("soundBuffZone", false);
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

            soundWave = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageIceBombProjectile.prefab").WaitForCompletion().InstantiateClone("soundBuffProjectile", false);
            if (!soundWave.GetComponent<NetworkIdentity>()) soundWave.AddComponent<NetworkIdentity>();

            ProjectileOverlapAttack overlapAttack = soundWave.GetComponent<ProjectileOverlapAttack>();
            overlapAttack.damageCoefficient = 1f;
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

        private static void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (NetworkServer.active && self.alive || !self.godMode || self.ospTimer <= 0f)
            {
                CharacterBody victimBody = self.body;
                CharacterBody attackerBody = null;
                EntityStateMachine victimMachine = null;
                if (victimBody)
                {
                    victimMachine = victimBody.GetComponent<EntityStateMachine>();
                }

                if (damageInfo.attacker)
                {
                    attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                }

                if (damageInfo.damage > 0 && !damageInfo.rejected && victimBody && attackerBody)
                {
                    if (attackerBody.bodyIndex == BodyCatalog.FindBodyIndex("BadgerBody"))
                    {
                        foreach(BuffIndex buff in victimBody.activeBuffsList)
                        {
                            if (BuffCatalog.GetBuffDef(buff).isDebuff)
                            {
                                damageInfo.damage *= 1.2f;
                            }
                        }

                        if(victimMachine && (victimMachine.state is EntityStates.StunState || victimMachine.state is EntityStates.FrozenState ||
                            victimMachine.state is EntityStates.ShockState || victimMachine.state is EntityStates.SleepState || 
                            victimMachine.state is EntityStates.Neuromancer.TimeStoppedState))
                        {
                            damageInfo.damage *= 0.2f;
                        }
                    }
                }
            }

            orig.Invoke(self, damageInfo);
        }

        private static void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
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
                        victimBody.AddTimedBuff(ScrapyardContent.Buffs.bdBadgerSlowBuff, 3f);
                    }
                }
            }
        }  
    }
}