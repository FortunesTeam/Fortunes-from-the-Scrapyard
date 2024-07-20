using ThunderKit.Addressable.Tools;

namespace RiskOfThunder.RoR2Importer
{
    public class ConfigureAddressableGraphics : AddressableGraphicsImport
    {
        public override int Priority => base.Priority - 10;
        public override string Name => "Configure Addressable Graphics Settings";
        public override string Description => "Assigns the Risk of Rain 2 DeferredShading and DeferredReflectionCustom shaders in the Addressable Graphics settings and by proxy in the Project's Graphics Settings";
        public override string CustomDeferredReflection => "RoR2/Base/Shaders/Internal-DeferredReflections.shader";
        public override string CustomDeferredShading => "RoR2/Base/Shaders/Internal-DeferredShadingCustom.shader";
        public override string CustomDeferredScreenspaceShadows => "NGSS/NGSS_Directional.shader";
    }
}