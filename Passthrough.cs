using System;
using System.Collections.Generic;
using Il2CppBigscreen;
using MelonLoader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(BigscreenPassthrough.Passthrough), "Bigscreen Passthrough", "1.0.0", "Copilot")]
[assembly: MelonGame("Bigscreen Inc.", "Bigscreen")]

namespace BigscreenPassthrough
{
    public sealed class Passthrough : MelonMod
    {
        private readonly List<Renderer> hidden = new List<Renderer>();
        private readonly HashSet<int> hiddenIds = new HashSet<int>();
        private const string PreferenceKey = "BigscreenChromaPrototype.Enabled";
        private Camera? camera;
        private GameObject? roomRoot;
        private Toggle? menuToggle;
        private CameraClearFlags oldClearFlags;
        private Color oldBackground;
        private bool active;
        private bool enabledSetting;
        private bool teleporting;
        private float nextScan;

        public override void OnInitializeMelon()
        {
            enabledSetting = !PlayerPrefs.HasKey(PreferenceKey) || PlayerPrefs.GetInt(PreferenceKey) == 1;
            LoggerInstance.Msg("Experimental passthrough filter defaults on. Use Mod Selection or F9 to toggle it.");
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F9))
                SetEnabled(!enabledSetting);
            var seatSelection = BIG_STATIC_SINGLETONS.bigSeatSelectionUI;
            bool arcActive = seatSelection != null && seatSelection.isActiveAndEnabled && seatSelection.ArcIsActive;
            if (arcActive != teleporting)
            {
                teleporting = arcActive;
                nextScan = 0f;
                if (teleporting && active)
                    Disable();
            }
            if (Time.unscaledTime < nextScan)
                return;
            nextScan = Time.unscaledTime + 1f;
            EnsureMenu();
            if (!enabledSetting || teleporting)
                return;
            GameObject root = GameObject.Find("SceneRoot");
            GameObject cameraObject = GameObject.Find("SteamVRUser(Clone)/ViewContainer/Head/Camera_Primary");
            Camera? foundCamera = cameraObject != null ? cameraObject.GetComponent<Camera>() : null;
            if (active && (root == null || root != roomRoot || foundCamera == null || foundCamera != camera))
                Disable();
            if (!active && root != null && foundCamera != null)
                Enable(root, foundCamera);
            else if (active && root != null)
                HideEnabled(root.transform);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (active)
                Disable();
        }

        public override void OnDeinitializeMelon()
        {
            if (active)
                Disable();
        }

        private void SetEnabled(bool value)
        {
            enabledSetting = value;
            PlayerPrefs.SetInt(PreferenceKey, value ? 1 : 0);
            PlayerPrefs.Save();
            if (menuToggle != null)
                menuToggle.SetIsOnWithoutNotify(value);
            if (!value && active)
                Disable();
            nextScan = 0f;
            LoggerInstance.Msg($"Passthrough filter {(value ? "enabled" : "disabled")}.");
        }

        private void Enable(GameObject root, Camera foundCamera)
        {
            roomRoot = root;
            camera = foundCamera;
            oldClearFlags = camera.clearFlags;
            oldBackground = camera.backgroundColor;
            active = true;
            try
            {
                HideEnabled(root.transform);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0f, 153f / 255f, 51f / 255f, 1f);
                LoggerInstance.Msg($"Chroma filter applied; hid {hidden.Count} renderers.");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"Could not enable passthrough filter: {ex}");
                Disable();
            }
        }

        private void HideEnabled(Transform root)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.enabled && !IsProtected(renderer, root) && hiddenIds.Add(renderer.GetInstanceID()))
                {
                    hidden.Add(renderer);
                    renderer.enabled = false;
                }
            }
        }

        private void EnsureMenu()
        {
            if (menuToggle != null)
                return;
            GameObject canvas = GameObject.Find("ModCanvas");
            Transform? content = canvas != null ? canvas.transform.Find("DropdownPanel/Content") : null;
            if (content == null || content.childCount == 0)
                return;
            Toggle template = content.GetChild(0).GetComponent<Toggle>();
            if (template == null)
            {
                LoggerInstance.Error("Cannot add passthrough toggle: Mod Selection template has no Toggle component.");
                return;
            }
            GameObject item = UnityEngine.Object.Instantiate(template.gameObject, content);
            item.name = "Passthrough";
            item.transform.SetAsFirstSibling();
            Toggle toggle = item.GetComponent<Toggle>();
            Text label = item.transform.Find("Label").GetComponent<Text>();
            label.text = "Passthrough";
            toggle.onValueChanged.RemoveAllListeners();
            toggle.SetIsOnWithoutNotify(enabledSetting);
            toggle.onValueChanged.AddListener((UnityAction<bool>)new Action<bool>(SetEnabled));
            menuToggle = toggle;
            LoggerInstance.Msg("Added passthrough toggle to Mod Selection.");
        }

        private static bool IsProtected(Renderer renderer, Transform root)
        {
            int layer = renderer.gameObject.layer;
            if (layer == 5 || layer == 8 || layer == 10 || layer == 13 || layer == 30)
                return true;

            Transform current = renderer.transform;
            while (current != null && current != root)
            {
                string name = current.name;
                if (name.IndexOf("avatar", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("player", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("screen", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.Equals("presentation", StringComparison.OrdinalIgnoreCase) ||
                    name.IndexOf("display", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("monitor", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("video", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.Equals("UI", StringComparison.OrdinalIgnoreCase) ||
                    name.IndexOf("canvas", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                current = current.parent;
            }
            return false;
        }

        private void Disable()
        {
            foreach (Renderer renderer in hidden)
            {
                if (renderer != null)
                    renderer.enabled = true;
            }
            hidden.Clear();
            hiddenIds.Clear();
            if (camera != null)
            {
                camera.clearFlags = oldClearFlags;
                camera.backgroundColor = oldBackground;
            }
            camera = null;
            roomRoot = null;
            active = false;
            LoggerInstance.Msg("Passthrough filter disabled; scene rendering restored.");
        }
    }
}
