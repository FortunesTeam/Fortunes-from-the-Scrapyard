using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
internal static class CloakerRagdollSetup
{
    private const string ModelPath = "Assets/FFTSAssets/Survivors/Cloaker/mdlCloaker.prefab";
    private const string WorkPath = @"C:\Users\mario\.copilot\session-state\fa19b6c1-0b62-492d-af11-fe1ad2e1a1b2\files";
    private static readonly string RequestPath = Path.Combine(WorkPath, "ragdoll-request.json");
    private static readonly string ResponsePath = Path.Combine(WorkPath, "ragdoll-response.txt");
    private static double nextCheck;

    [Serializable]
    private sealed class Request
    {
        public string action;
    }

    static CloakerRagdollSetup()
    {
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        if (EditorApplication.timeSinceStartup < nextCheck || EditorApplication.isCompiling
            || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        nextCheck = EditorApplication.timeSinceStartup + 0.5;
        if (!File.Exists(RequestPath)) return;
        RunBatch();
    }

    public static void RunBatch()
    {
        Request request = JsonUtility.FromJson<Request>(File.ReadAllText(RequestPath));
        File.Delete(RequestPath);
        try
        {
            if (request.action != "inspect")
                throw new InvalidOperationException("Unknown action: " + request.action);
            File.WriteAllText(ResponsePath, Inspect());
            Debug.Log("Cloaker ragdoll inspection completed: " + ResponsePath);
        }
        catch (Exception exception)
        {
            File.WriteAllText(ResponsePath, "FAILED\n" + exception);
            Debug.LogException(exception);
            if (Application.isBatchMode) EditorApplication.Exit(1);
        }
    }

    private static string Inspect()
    {
        var report = new StringBuilder();
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        report.AppendLine("PREFAB STAGE: " + (stage == null ? "none" : stage.assetPath + " dirty=" + stage.scene.isDirty));
        GameObject model = PrefabUtility.LoadPrefabContents(ModelPath);
        try
        {
            report.AppendLine("MODEL: " + model.name + " scale=" + model.transform.lossyScale.ToString("F4"));
            foreach (MonoBehaviour component in model.GetComponentsInChildren<MonoBehaviour>(true))
                report.AppendLine(component ? "COMPONENT: " + component.GetType().AssemblyQualifiedName
                    + " on " + component.name : "MISSING SCRIPT");
            foreach (SkinnedMeshRenderer renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                report.AppendLine("MESH: " + renderer.name + " bounds=" + renderer.bounds
                    + " root=" + renderer.rootBone?.name + " bones=" + renderer.bones.Length);
            var selected = new HashSet<string>
            {
                "Root_M", "Pelvis_M", "SpineBase_M", "Spine1_M", "Chest_M", "Neck_M", "Head_M",
                "Hip_L", "Knee_L", "Ankle_L", "Hip_R", "Knee_R", "Ankle_R",
                "Shoulder_L", "Elbow_L", "Wrist_L", "Shoulder_R", "Elbow_R", "Wrist_R"
            };
            foreach (Transform bone in model.GetComponentsInChildren<Transform>(true).Where(t => selected.Contains(t.name)))
                report.AppendLine("BONE: " + bone.name + " parent=" + bone.parent.name
                    + " position=" + model.transform.InverseTransformPoint(bone.position).ToString("F4")
                    + " scale=" + bone.lossyScale.ToString("F4"));
            report.AppendLine("EXISTING PHYSICS: bodies=" + model.GetComponentsInChildren<Rigidbody>(true).Length
                + " joints=" + model.GetComponentsInChildren<Joint>(true).Length
                + " colliders=" + model.GetComponentsInChildren<Collider>(true).Length);
            MonoBehaviour ragdoll = model.GetComponents<MonoBehaviour>().Single(c => c && c.GetType().FullName == "RoR2.RagdollController");
            report.AppendLine("RUNTIME BEGIN: " + ragdoll.GetType().GetMethod("BeginRagdoll", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(model);
        }
        return report.ToString();
    }
}
