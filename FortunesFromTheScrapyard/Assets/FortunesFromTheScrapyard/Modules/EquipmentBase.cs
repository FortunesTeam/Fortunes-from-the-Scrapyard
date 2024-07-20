﻿using BepInEx.Configuration;
using FortunesFromTheScrapyard.Modules;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FortunesFromTheScrapyard.Equipment
{
    public abstract class EquipmentBase<T> : EquipmentBase where T : EquipmentBase<T>
    {
        public static T instance { get; private set; }

        public EquipmentBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBoilerplate/Item was instantiated twice");
            instance = this as T;
        }
    }
    public abstract class EquipmentBase
    {
        public string equipName;
        public EquipmentDef equipDef;
        public abstract void Hooks();
        public virtual void Init()
        {
            Hooks();
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;
        }
        public virtual EquipmentDef GetEquipDef()
        {
            return FortunesContent.contentPack.equipmentDefs.Find(equipName);
        }
        internal bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == GetEquipDef())
            {
                return ActivateEquipment(self);
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }

        protected abstract bool ActivateEquipment(EquipmentSlot slot);


        #region Targeting
        public Ray GetAimRay(InputBankTest inputBank)
        {
            return new Ray
            {
                direction = inputBank.aimDirection,
                origin = inputBank.aimOrigin
            };
        }

        public Ray GetAimRay(CharacterBody body)
        {
            if (body.inputBank)
            {
                return new Ray(body.inputBank.aimOrigin, body.inputBank.aimDirection);
            }
            return new Ray(body.transform.position, body.transform.forward);
        }
        #endregion
    }
}