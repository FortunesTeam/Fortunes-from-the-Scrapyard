using MSU;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;

namespace TheCommission
{
    public class CommissionContent : IContentPackProvider
    {
        public string identifier => Main.GUID;

        public static ContentPack RuntimeContentPack { get; private set; } = new ContentPack();

        private static Func<IEnumerator>[] _loadDispatchers;
        private static Func<IEnumerator>[] _fieldAssignDispatchers;

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            var enumerator = CommissionAssets.Initialize(args);
            
            while (enumerator.MoveNext())
                yield return null;

            var expansionRequest = new CommissionAssets.AssetRequest<ExpansionDef>
            {
                TargetAssetNameAttribute = "CommissionExpansion",
                BundleEnum = CommissionBundle.Main
            };
            
            enumerator = expansionRequest.Load();
            while (enumerator.MoveNext()) yield return null;
            
            RuntimeContentPack.expansionDefs.AddSingle(expansionRequest.Asset);

            for (int i = 0; i < _loadDispatchers.Length; i++)
            {
                args.ReportProgress(Util.Remap(i + 1, 0f, _loadDispatchers.Length, 0f, 0.05f));
                enumerator = _loadDispatchers[i]();

                while (enumerator.MoveNext()) yield return null;
            }

            for (int i = 0; i < _fieldAssignDispatchers.Length; i++)
            {
                args.ReportProgress(Util.Remap(i + 1, 0f, _fieldAssignDispatchers.Length, 0.95f, 0.99f));
                enumerator = _fieldAssignDispatchers[i]();

                while(enumerator.MoveNext()) yield return null;
            }
        }
        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(RuntimeContentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        internal CommissionContent()
        {
            ContentManager.collectContentPackProviders += (@delegate) => @delegate(this);
        }

        static CommissionContent()
        {
            Main main = Main.Instance;
            _loadDispatchers = new Func<IEnumerator>[]
            {

            };

            _fieldAssignDispatchers = new Func<IEnumerator>[]
            {

            };
        }
    }
}