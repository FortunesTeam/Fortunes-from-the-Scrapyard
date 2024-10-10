using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.HudOverlay;
using System;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Projectile;

namespace FortunesFromTheScrapyard.Survivors.Maiden.Components
{
    public class MaidenController : MonoBehaviour
    {
        public enum DiceType
        {
            BasicDie,
            SpecialDie
        }
        public int currentResult { get; private set; }
        public int currentSpecialResult { get; private set; }
        public int empoweredCounter { get; private set; }

        private NetworkIdentity networkIdentity;
        private CharacterBody characterBody;
        private ModelSkinController skinController;
        private ChildLocator childLocator;
        private CharacterModel characterModel;
        private Animator animator;
        private SkillLocator skillLocator;
        private GameObject projectilePrefab;

        private int currentDiceIndex;
        //private Xoroshiro128Plus rng;

        public DiceDef[] passiveDice { get; private set; }
        public DiceDef[] specialDice { get; private set; }

        public event Action<MaidenController> onDisabled;
        public event Action<MaidenController> onSpecialCasted;

        public bool hasAuthority
        {
            get
            {
                if (networkIdentity == null)
                {
                    return false;
                }
                return Util.HasEffectiveAuthority(networkIdentity);
            }
        }

        private void Awake()
        {
            this.characterBody = this.GetComponent<CharacterBody>();
            ModelLocator modelLocator = this.GetComponent<ModelLocator>();
            this.childLocator = modelLocator.modelTransform.GetComponentInChildren<ChildLocator>();
            this.animator = modelLocator.modelTransform.GetComponentInChildren<Animator>();
            this.characterModel = modelLocator.modelTransform.GetComponentInChildren<CharacterModel>();
            this.skillLocator = this.GetComponent<SkillLocator>();
            this.skinController = modelLocator.modelTransform.gameObject.GetComponent<ModelSkinController>();
            this.networkIdentity = this.GetComponent<NetworkIdentity>();

            this.projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/LunarSunProjectile");

            passiveDice = new DiceDef[6];
            specialDice = new DiceDef[6];
        }

        private void Start()
        {
            //rng = new Xoroshiro128Plus(DealerSurvivor.diceRng.nextUlong);
        }
        private void OnDisable()
        {
            this.onDisabled?.Invoke(this);
            this.onDisabled = null;
        }
        public void ShuffleDiceOfType(DiceType diceType = DiceType.BasicDie, bool justRolledSpecial = false)
        {
            empoweredCounter = 0;
            if (diceType == DiceType.BasicDie)
            {
                currentResult = 0;

                if (justRolledSpecial) currentResult += currentSpecialResult;

                foreach(DiceDef dice in passiveDice)
                {
                    dice.currentRoll = dice.ShuffleDice(characterBody);

                    if (dice.currentRoll == 6)
                    { 
                        empoweredCounter++;
                    }
                    
                    currentResult += dice.currentRoll;
                }
            }
            else if(diceType == DiceType.SpecialDie)
            {
                onSpecialCasted?.Invoke(this);
                currentDiceIndex = 0;
                currentSpecialResult = 0;

                foreach (DiceDef dice in specialDice)
                {
                    dice.currentRoll = dice.ShuffleDice(characterBody);

                    currentSpecialResult += dice.currentRoll;

                    /*
                    if (!characterBody.master.IsDeployableLimited(MaidenSurvivor.MaidenDiceSlot))
                    {
                        FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                        fireProjectileInfo.projectilePrefab = projectilePrefab;
                        fireProjectileInfo.crit = characterBody.RollCrit();
                        fireProjectileInfo.damage = characterBody.damage * dice.currentRoll;
                        fireProjectileInfo.damageColorIndex = DamageColorIndex.Item;
                        fireProjectileInfo.force = 0f;
                        fireProjectileInfo.owner = base.gameObject;
                        fireProjectileInfo.position = characterBody.transform.position;
                        fireProjectileInfo.rotation = Quaternion.identity;
                        FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
                        ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
                        currentDiceIndex++;
                    }
                    */
                }

                ShuffleDiceOfType(DiceType.BasicDie, true);
            }

            if (empoweredCounter >= 3)
            {
                if (NetworkServer.active)
                {
                    //characterBody.AddTimedBuff(MaidenBuffs.diceEmpowermentBuff, 6f, 1);
                }
            }

            characterBody.RecalculateStats();
        }
        private Vector3 GetInitialDegreesFromOwnerForward(float initialDegreesFromOwnerForward, int positionIndex, Vector3 planeNormal)
        {
            if (characterBody.transform)
            {
                return Quaternion.AngleAxis(initialDegreesFromOwnerForward, planeNormal) * characterBody.transform.forward;
            }
            else return Vector3.zero;
        }
        private float GetDiceOffsetDegreesByIndex(int index)
        {
            return index switch
            {
                1 => 240f,
                2 => 120f,
                3 => 300f,
                4 => 60f,
                5 => 180f,
                _ => 0f,
            };
        }
        public void InitializeOrbiter(ProjectileOwnerOrbiter orbiter, MaidenDiceProjectileController controller)
        {
            float offset = characterBody.radius + 3f;
            float degreesPerSecond = 180f * Mathf.Pow(0.9f, offset);
            Vector3 planeNormal = Vector3.up;

            orbiter.Initialize(planeNormal, offset, degreesPerSecond, GetDiceOffsetDegreesByIndex(currentDiceIndex));

            onDisabled += DestroyOrbiter;
            onSpecialCasted += DestroyOrbiter;  
            void DestroyOrbiter(MaidenController maidenController)
            {
                if (controller)
                {
                    controller.Detonate();
                }
            }
        }

        public class DiceDef
        {
            public int currentRoll;

            public int ShuffleDice(CharacterBody characterBody)
            {
                int result = 0;
                int luck = Mathf.CeilToInt(Mathf.Abs(characterBody.master.luck));

                int currentDiceResult = UnityEngine.Random.Range(1, 6);
                int currentFirstRoll = currentDiceResult;
                for (int j = 0; j < luck; j++)
                {
                    int luckResult = UnityEngine.Random.Range(1, 6);
                    currentDiceResult = (characterBody.master.luck > 0) ? Mathf.Max(currentDiceResult, luckResult) : Mathf.Min(currentDiceResult, luckResult);
                }
                if (currentDiceResult == 6)
                {
                    if (currentFirstRoll < 6)
                    {
                        characterBody.wasLucky = true;
                    }
                    result += currentDiceResult * 3;
                }
                else
                {
                    result += currentDiceResult;
                }

                //EffectManager.SimpleEffect(, base.transform.position, Util.QuaternionSafeLookRotation(characterBody.inputBank.aimDirection), true);

                return result;
            }
        }
    }
}
