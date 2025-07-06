using EntityStates;
using RoR2.Audio;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using FortunesFromTheScrapyard.Content.Base.Survivors.Skater.Components;
using FortunesFromTheScrapyard;

namespace EntityStates.Skater
{
    public class SkaterSkatingMain : GenericCharacterMain
    {
        private EntityStateMachine weaponStateMachine;
        private bool letGo;

        float minDuration = 0.5f;
        float maxSpeedGiven = 300f;
        float speedGiven = 0f;
        OverlapAttack attack;
        float stopwatch = 0f;
        bool attacking;
        bool isCollision;
        bool failsafe;
        bool isTeleport;
        bool sprintLifted;
        bool sprintDown;

        (bool upHeld, bool leftHeld, bool downHeld, bool rightHeld) trickInputs;
        bool anyDirectionHeld => (trickInputs.upHeld || trickInputs.leftHeld || trickInputs.downHeld || trickInputs.rightHeld);
        enum TrickDirection
        {
            None, Up, Left, Down, Right
        }
        TrickDirection currentTrickDirection = TrickDirection.None;
        int storedLayerIndex;
        Vector3 flatMoveVector;
        Vector3 previousPositionXZ;

        SkaterController skaterController;

        public override void OnEnter()
        {
            base.OnEnter();
            skaterController = GetComponent<SkaterController>();
            skaterController.isSkating = true;
            weaponStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Weapon");

            sprintDown = false;
        }
        public override void FixedUpdate()
        {
            BaseBaseFixedUpdate();
            BaseFixedUpdate();
            GatherInputs();
            /*moveVector = aimDirection;
            flatMoveVector = new Vector3(moveVector.x, 0, moveVector.z).normalized;*/
            HandleMovements();
            PerformInputs();
            if (speedGiven < maxSpeedGiven && characterMotor.isGrounded && (Vector3.Angle(flatMoveVector, characterMotor.moveDirection) < 15f))
            {
                characterBody.AddBuff(FFTSContent.Buffs.bdSkaterSpeedBuff);
                speedGiven++;
            }
            if (speedGiven >= 100/*maxSpeedGiven / 5*/)
            {
                if (!attacking)
                {
                    attacking = true;
                    storedLayerIndex = gameObject.layer;
                    gameObject.layer = LayerIndex.fakeActor.intVal;
                    characterMotor.Motor.RebuildCollidableLayers();
                }
                //blech do this another way
                /*if (previousPosition != null)
                {
                float distanceFromPreviousPosition = Vector3.Distance(previousPositionXZ, new Vector3(transform.position.x, 0, transform.position.z));
                float expectedDistanceFromPreviousPosition = Vector3.Distance(previousPositionXZ, previousPositionXZ + new Vector3(characterMotor.lastVelocity.x, 0, characterMotor.lastVelocity.z) * Time.fixedDeltaTime);
                    if (distanceFromPreviousPosition <= expectedDistanceFromPreviousPosition * 0.90 || distanceFromPreviousPosition >= expectedDistanceFromPreviousPosition * 3.0)
                    {
                        if (failsafe)
                        isCollision = true;
                        else
                        {
                            failsafe = true;
                        }
                    }
                    else if (failsafe)
                        failsafe = false;
                }*/
            }

            characterBody.isSprinting = true;

            PlayerCharacterMasterController playerMaster;
            if (/*inputBank.sprint.down*/playerMaster = characterBody.master.playerCharacterMasterController)
            {
                if (PlayerCharacterMasterController.CanSendBodyInput(playerMaster.networkUser, out _, out var player, out _, out _))
                {
                    sprintDown = player.GetButton(18);
                    if (!sprintLifted && !sprintDown)
                    {
                        sprintLifted = true;
                    }

                    trickInputs.upHeld = player.GetAxis(1) >= 0.5f;
                    trickInputs.leftHeld = player.GetAxis(0) <= -0.5f;
                    trickInputs.downHeld = player.GetAxis(1) <= -0.5f;
                    trickInputs.rightHeld = player.GetAxis(0) >= 0.5f;
                    TrickDirection temp = currentTrickDirection;
                    if (currentTrickDirection == TrickDirection.None)
                    {
                        if (trickInputs.upHeld)
                        {
                            currentTrickDirection = TrickDirection.Up;
                        }
                        else if (trickInputs.leftHeld)
                        {
                            currentTrickDirection = TrickDirection.Left;
                        }
                        else if (trickInputs.downHeld)
                        {
                            currentTrickDirection = TrickDirection.Down;
                        }
                        else if (trickInputs.rightHeld)
                        {
                            currentTrickDirection = TrickDirection.Right;
                        }
                    }
                    else if (!anyDirectionHeld)
                    {
                        currentTrickDirection = TrickDirection.None;
                    }

                    if (!characterMotor.isGrounded && temp == TrickDirection.None && temp != currentTrickDirection)
                    {
                        TrickBaseState trickState = null;
                        switch (trickInputs)
                        {
                            case var _ when trickInputs.upHeld:
                                trickState = new TrickUp();
                                break;
                            case var _ when trickInputs.leftHeld:
                                trickState = new TrickLeft();
                                break;
                            case var _ when trickInputs.downHeld:
                                trickState = new TrickDown();
                                break;
                            case var _ when trickInputs.rightHeld:
                                trickState = new TrickRight();
                                break;
                            default:
                                break;
                        }
                        if (trickState != null)
                        {
                            weaponStateMachine.SetInterruptState(trickState, InterruptPriority.Skill);
                        }
                    }
                }
            }
            if (sprintLifted && sprintDown && fixedAge >= 0.5f)
            {
                outer.SetNextStateToMain();
            }

        }

        public override void OnExit()
        {
            characterBody.SetBuffCount(FFTSContent.Buffs.bdSkaterSpeedBuff.buffIndex, 0);
            base.OnExit();
        }

        public void BaseBaseFixedUpdate()
        {
            fixedAge += GetDeltaTime();
            stopwatch += GetDeltaTime();
        }
        public void BaseFixedUpdate()
        {

            if (hasCharacterMotor)
            {
                float num = estimatedVelocity.y - lastYSpeed;
                if (isGrounded && !wasGrounded && hasModelAnimator)
                {
                    int layerIndex = modelAnimator.GetLayerIndex("Impact");
                    if (layerIndex >= 0)
                    {
                        modelAnimator.SetLayerWeight(layerIndex, Mathf.Clamp01(Mathf.Max(0.3f, num / 5f, modelAnimator.GetLayerWeight(layerIndex))));
                        modelAnimator.PlayInFixedTime("LightImpact", layerIndex, 0f);
                    }
                }
                wasGrounded = isGrounded;
                lastYSpeed = estimatedVelocity.y;
            }
            if (!hasRootMotionAccumulator)
            {
                return;
            }
            Vector3 vector = rootMotionAccumulator.ExtractRootMotion();
            if (useRootMotion && vector != Vector3.zero && isAuthority)
            {
                if ((bool)characterMotor)
                {
                    characterMotor.rootMotion += vector;
                }
                if ((bool)railMotor)
                {
                    railMotor.rootMotion += vector;
                }
            }
        }

        public override void HandleMovements()
        {
            if (useRootMotion)
            {
                if (hasCharacterMotor)
                {
                    characterMotor.moveDirection = Vector3.zero;
                }
                if (hasRailMotor)
                {
                    railMotor.inputMoveVector = moveVector;
                }
            }
            else
            {
                if (hasCharacterMotor)
                {
                    float extraMoveSpeed = Mathf.Max(characterBody.moveSpeed - (characterBody.baseMoveSpeed + (characterBody.baseMoveSpeed * 0.02f * speedGiven) + characterBody.levelMoveSpeed * (characterBody.level - 1f) + (characterBody.levelMoveSpeed * (characterBody.level - 1f) * 0.02f * speedGiven)), 1f);
                    characterMotor.moveDirection = Vector3.RotateTowards(characterMotor.moveDirection, ((moveVector == Vector3.zero || !characterMotor.isGrounded) ? characterDirection.forward : moveVector), Mathf.Lerp(Mathf.PI, /*Mathf.Min(*/Mathf.PI / (maxSpeedGiven / 40f)/* * extraMoveSpeed, Mathf.PI)*/, speedGiven / maxSpeedGiven) * GetDeltaTime() * extraMoveSpeed, 1f);
                }
                if (hasRailMotor)
                {
                    railMotor.inputMoveVector = moveVector;
                }
            }
            _ = isGrounded;
            if (!hasRailMotor && hasCharacterDirection && hasCharacterBody)
            {
                if (hasAimAnimator && aimAnimator.aimType == AimAnimator.AimType.Smart)
                {
                    Vector3 vector = (moveVector == Vector3.zero || !characterMotor.isGrounded) ? characterDirection.forward : moveVector;
                    float num = Vector3.Angle(aimDirection, vector);
                    float num2 = Mathf.Max(aimAnimator.pitchRangeMax + aimAnimator.pitchGiveupRange, aimAnimator.yawRangeMax + aimAnimator.yawGiveupRange);
                    characterDirection.moveVector = (bool)characterBody && characterBody.shouldAim && num > num2 ? aimDirection : vector;
                }
                else
                {
                    characterDirection.moveVector = (bool)characterBody && characterBody.shouldAim ? aimDirection : moveVector;
                }
            }
            if (!isAuthority)
            {
                return;
            }
            ProcessJump();
            if (hasCharacterBody)
            {
                characterBody.isSprinting = true;
            }
        }
    }
}
