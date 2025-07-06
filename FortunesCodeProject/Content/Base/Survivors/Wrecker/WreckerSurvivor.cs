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
/*using ThreeEyedGames;
using EmotesAPI;*/
using RoR2.Skills;
using EntityStates.Wrecker.Components;
using FortunesFromTheScrapyard;
using MSU.Config;

namespace FortunesFromTheScrapyard.Survivors.Wrecker
{
    public class WreckerSurvivor : FFTSSurvivor
    {
        // lol I have no CLUE what I'm doing, If ANY of this looks right it by the grace of God or Kenko - EZ


        public const string PRIMARYTOKEN = "FFTS_WRECKER_PRIMARY_DESCRIPTION";
        public const string SECONDARYTOKEN = "FFTS_WRECKER_SECONDARY_DESCRIPTION";
        public const string UTILITYTOKEN = "FFTS_WRECKER_UTILITY_DESCRIPTION";
        public const string SPECIALTOKEN = "FFTS_WRECKER_SPECIAL_DESCRIPTION";

        [ConfigureField(FFTSConfig.ID_SURVIVORS)]
        //[FormatToken(PRIMARYTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float basePrimaryDamage = 0.6f;

        [ConfigureField(FFTSConfig.ID_SURVIVORS)]
        //[FormatToken(SECONDARYTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        internal static float baseSecondaryDamage = 1.2f;

        [ConfigureField(FFTSConfig.ID_SURVIVORS)]
        //[FormatToken(UTILITYTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float baseUtilityDamage = 4f;

        [ConfigureField(FFTSConfig.ID_SURVIVORS)]
        //[FormatToken(SPECIALTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        public static float baseSpecialDamage = 6f;

        // DamageTypes
        public static DamageAPI.ModdedDamageType WreckerExplode;


        //Projectiles
        internal static GameObject diskPrefab;
        internal static GameObject diskGhost;
        internal static GameObject diskExplosion;

        internal static GameObject soundScape;
        internal static GameObject soundWave;

        public override void Initialize()
        {
            BodyCatalog.availability.CallWhenAvailable(CreateProjectiles);

            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

            WreckerExplode = DamageAPI.ReserveDamageType();

            ModifyPrefab();

            Hooks();
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override FFTSAssetRequest<SurvivorAssetCollection> LoadAssetRequest()
        {
            return FFTSAssets.LoadAssetAsync<SurvivorAssetCollection>("acWrecker", FFTSBundle.Survivors);
        }

        public void ModifyPrefab()
        {
            var cb = characterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
        }

        private void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;

            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamageProcess;

            if (FFTSMain.emotesInstalled)
            {
                //Emotes();
            }
        }

        /*private void Emotes()
        {
            On.RoR2.SurvivorCatalog.Init += (orig) =>
            {
                orig();
                var skele = FFTSAssets.GetAssetBundle(FFTSBundle.Indev).LoadAsset<GameObject>("wrecker_emoteskeleton");
                CustomEmotesAPI.ImportArmature(this.characterPrefab, skele);
            };
            CustomEmotesAPI.animChanged += CustomEmotesAPI_animChanged;
        }
        private void CustomEmotesAPI_animChanged(string newAnimation, BoneMapper mapper)
        {
            if (newAnimation != "none")
            {
                if (mapper.transform.name == "wrecker_emoteskeleton")
                {
                    mapper.transform.parent.Find("meshGun").gameObject.SetActive(value: false);
                }
            }
            else
            {
                if (mapper.transform.name == "wrecker_emoteskeleton")
                {
                    mapper.transform.parent.Find("meshGun").gameObject.SetActive(value: true);
                }
            }
        }*/

        #region projectiles
        private void CreateProjectiles()
        {
            soundScape = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMineAltDetonated.prefab").WaitForCompletion().InstantiateClone("soundBuffZone");
            soundScape.layer = LayerIndex.noCollision.mask;
            soundScape.EnsureComponent<NetworkIdentity>();
            //if (!soundScape.GetComponent<NetworkIdentity>()) soundScape.AddComponent<NetworkIdentity>();
            BuffWard buffWard = soundScape.GetComponent<BuffWard>();
            buffWard.radius = 3f;
            buffWard.interval = 0.2f;
            buffWard.buffDef = FFTSContent.Buffs.bdWreckerSoundBuff;
            buffWard.expires = true;
            buffWard.expireDuration = 6f;
            buffWard.invertTeamFilter = false;

            soundScape.GetComponent<SphereCollider>().radius = 3f;

            //soundScape = assetCollection.FindAsset<GameObject>("WreckerSoundScapeProjectile");

            FFTSContent.scrapyardContentPack.projectilePrefabs.AddSingle(soundScape);

            soundWave = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageIceBombProjectile.prefab").WaitForCompletion().InstantiateClone("soundBuffProjectile");
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

            FFTSContent.scrapyardContentPack.projectilePrefabs.AddSingle(soundWave);

            diskPrefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2ShivProjectile.prefab").WaitForCompletion().InstantiateClone("WreckerDisk");
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

            diskGhost = assetCollection.FindAsset<GameObject>("WreckerDiskGhost");

            diskPrefab.GetComponent<ProjectileController>().ghostPrefab = diskGhost;

            diskExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LaserTurbine/LaserTurbineBomb.prefab").WaitForCompletion().InstantiateClone("WreckerDiskExplosion");

            diskPrefab.GetComponent<ProjectileImpactExplosion>().explosionEffect = diskExplosion;

            FFTSContent.scrapyardContentPack.projectilePrefabs.AddSingle(diskPrefab);
        }
        #endregion

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            if (self)
            {
                if (self.HasBuff(FFTSContent.Buffs.bdWreckerSoundBuff))
                {
                    self.armor += 20f;
                    self.attackSpeed *= 1.35f;
                }

                if (self.HasBuff(FFTSContent.Buffs.bdWreckerSlowBuff))
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
                    if (attackerBody.bodyIndex == BodyCatalog.FindBodyIndex("WreckerBody"))
                    {
                        float finalMultiplier = 1f;

                        //Get Debuff Indexes
                        BuffIndex[] debuffBuffIndices = BuffCatalog.debuffBuffIndices;
                        //Go through buffs on enemy
                        foreach (BuffIndex buff in debuffBuffIndices)
                        {
                            //Check if its a debuff
                            if (victimBody.HasBuff(buff))
                            {
                                finalMultiplier += 0.2f;
                            }
                        }

                        //Go through victim state machine and check if they are stunned, frozen, or shocked. This is hardcoded
                        if (victimMachine && (victimMachine.state is EntityStates.StunState || victimMachine.state is EntityStates.FrozenState ||
                            victimMachine.state is EntityStates.ShockState /*||
                            (FFTSMain.NeuromancerInstalled && victimMachine.state is EntityStates.Neuromancer.TimeStoppedState)*/))
                        {
                            finalMultiplier += 0.2f;
                        }

                        damageInfo.damage *= finalMultiplier;
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
            WreckerController iController = attackerBody.GetComponent<WreckerController>();
            if (NetworkServer.active)
            {
                if (iController && attackerBody.bodyIndex == BodyCatalog.FindBodyIndex("WreckerBody"))
                {
                    if (damageInfo.HasModdedDamageType(WreckerExplode))
                    {
                        victimBody.AddTimedBuff(FFTSContent.Buffs.bdWreckerSlowBuff, 3f);
                    }
                }
            }
        }
    }
}