using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Core.Config;
using ThunderKit.Core.Data;
using ThunderKit.Markdown;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RiskOfThunder.RoR2Importer
{
    using static ThunderKit.Common.Constants;
    public class AssemblyPublicizerConfiguration : OptionalExecutor
    {
        public override string Name => "Assembly Publicizer";
        public override string Description => "Listed assemblies will be publicized using NStrip." +
            "\nPublicized assemblies retain their inspector look and functionality, this does not strip assemblies.";
        public override int Priority => Constants.Priority.AssemblyPublicizerConfiguration;

        public List<string> assemblyNames = new List<string> { "RoR2.dll", "KinematicCharacterController.dll" };

        public UnityEngine.Object NStripExecutable;

        private SerializedObject serializedObject;
        private VisualElement rootVisualElement;
        private MarkdownElement MessageElement
        {
            get
            {
                if (_messageElement == null)
                {
                    _messageElement = new MarkdownElement();
                    _messageElement.MarkdownDataType = MarkdownDataType.Text;
                }
                return _messageElement;
            }
        }
        private MarkdownElement _messageElement;

        public override bool Execute() => true;

        protected override VisualElement CreateProperties()
        {
            serializedObject = new SerializedObject(this);
            var executableProperty = serializedObject.FindProperty(nameof(NStripExecutable));
            var assemblyList = serializedObject.FindProperty(nameof(assemblyNames));
            rootVisualElement = new VisualElement();

            //Nstrip should ideally be located automatically, This method should find it if nstrip is in Packages, which it should be.
            if (executableProperty.objectReferenceValue == null)
            {
                //If NStrip couldnt be located, display warning
                if (TryToFindNStripExecutable(out var executable))
                {
                    executableProperty.objectReferenceValue = executable;
                    serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    MessageElement.Data = $"***__WARNING__***: Could not find NStrip Executable! Hover over the \"N Strip Executable\" field for instructions.";
                    rootVisualElement.Add(MessageElement);
                }
            }

            PropertyField nstripField = new PropertyField(executableProperty);
            nstripField.tooltip = $"The NStrip executable, this is used for the publicizing system" +
                $"\nIf this field appears to be empty, then the RoR2Importer has failed to find the executable automatically." +
                $"\nPlease select the NStrip executable in your project, if no Executable exists, download NStrip version 1.4 or newer";
            nstripField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(OnNStripSet);
            rootVisualElement.Add(nstripField);

            PropertyField listField = new PropertyField(assemblyList);
            listField.tooltip = $"A list of assembly names to publicize, Case Sensitive.";
            rootVisualElement.Add(listField);
            return rootVisualElement;
        }

        private void OnNStripSet(ChangeEvent<UnityEngine.Object> evt)
        {
            var nstrip = evt.newValue;
            if (nstrip == null)
            {
                MessageElement.Data = $"***__WARNING__***: Could not find NStrip Executable! Hover over the \"N Strip Executable\" field for instructions.";
                if (!rootVisualElement.Contains(MessageElement))
                {
                    rootVisualElement.Add(MessageElement);
                }
                return;
            }

            var relativePath = AssetDatabase.GetAssetPath(nstrip);
            var fullPath = Path.GetFullPath(relativePath);
            var fileName = Path.GetFileName(fullPath);

            if (fileName != "NStrip.exe")
            {
                MessageElement.Data = $"Object in \"N Strip Executable\" is not NStrip!";
                if (!rootVisualElement.Contains(MessageElement))
                {
                    rootVisualElement.Add(MessageElement);
                }
                return;
            }

            MessageElement.RemoveFromHierarchy();
        }

        private bool TryToFindNStripExecutable(out UnityEngine.Object nstripExecutable)
        {
            nstripExecutable = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(Constants.Paths.NStripExePath);
            if (!nstripExecutable)
            {
                var nstripPath = AssetDatabase.FindAssets("", FindAllFolders)
                             .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                             .FirstOrDefault(path => path.EndsWith("NStrip.exe"));

                nstripExecutable = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(nstripPath);
            }

            return nstripExecutable;
        }


        internal static AssemblyPublicizerConfiguration GetDataStorer()
        {
            var settings = ThunderKitSetting.GetOrCreateSettings<ImportConfiguration>();
            var publicizerDataStorer = settings.ConfigurationExecutors.OfType<AssemblyPublicizerConfiguration>().FirstOrDefault();
            return publicizerDataStorer;
        }

        public override void Cleanup()
        {
            if (!Directory.Exists(Constants.Paths.PublicizedAssembliesFolder))
            {
                return;
            }

            var publicizedAssemblies = Directory.EnumerateFiles(Constants.Paths.PublicizedAssembliesFolder, "*.dll", SearchOption.AllDirectories);
            foreach (string assemblyPath in publicizedAssemblies)
            {
                File.Delete(assemblyPath);
            }
        }
    }
}