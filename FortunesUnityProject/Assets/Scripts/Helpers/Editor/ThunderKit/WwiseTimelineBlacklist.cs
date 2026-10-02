using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Core.Config;

namespace Fortunes.Editor
{
    public sealed class WwiseTimelineBlacklist : BlacklistProcessor
    {
        public override string Name => "Wwise Timeline Assembly Blacklist";
        public override int Priority => 9_001;

        public override IEnumerable<string> Process(IEnumerable<string> blacklist)
        {
            if (!File.Exists("Assets/Wwise/Timeline/Runtime/AK.Wwise.Unity.Timeline.asmdef"))
                return blacklist;

            // MSU's Wwise blacklist omits the Timeline DLL extension.
            return blacklist.Concat(new[] { "AK.Wwise.Unity.Timeline.dll", "Unity.Timeline.dll" });
        }
    }
}
