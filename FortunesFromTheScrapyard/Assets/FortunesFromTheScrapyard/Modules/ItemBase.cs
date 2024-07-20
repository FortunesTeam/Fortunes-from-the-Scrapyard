﻿using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2.ExpansionManagement;
using RoR2.EntitlementManagement;
using FortunesFromTheScrapyard.Modules;

namespace FortunesFromTheScrapyard.Items
{
    public abstract class ItemBase<T> : ItemBase where T : ItemBase<T>
    {
        public static T instance { get; private set; }

        public ItemBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBoilerplate/Item was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class ItemBase
    {
        public string itemName;
        public ItemDef itemDef;
        public abstract void Hooks();
        public virtual void Init()
        {
            Hooks();
        }
        public virtual ItemDef GetItemDef()
        {
            return FortunesContent.contentPack.itemDefs.Find(itemName);
        }
        public int GetCount(CharacterBody body)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCount(GetItemDef());
        }
        public int GetCount(Inventory inventory)
        {
            if (!inventory) { return 0; }

            return inventory.GetItemCount(GetItemDef());
        }

        public int GetCount(CharacterMaster master)
        {
            if (!master || !master.inventory) { return 0; }

            return master.inventory.GetItemCount(GetItemDef());
        }

        public int GetCountSpecific(CharacterBody body, ItemDef itemIndex)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCount(itemIndex);
        }

        public static float GetStackValue(float baseValue, float stackValue, int itemCount)
        {
            return baseValue + stackValue * (itemCount - 1);
        }
    }
}