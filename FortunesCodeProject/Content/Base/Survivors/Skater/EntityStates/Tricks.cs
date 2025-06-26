using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2;

namespace EntityStates.Skater
{
    public class TrickUp : TrickBaseState
    {
        protected override string trickName { get; set; } = "Up";
    }
    public class TrickLeft : TrickBaseState
    {
        protected override string trickName { get; set; } = "Left";
    }
    public class TrickDown : TrickBaseState
    {
        protected override string trickName { get; set; } = "Down";
    }
    public class TrickRight : TrickBaseState
    {
        protected override string trickName { get; set; } = "Right";
    }
}
