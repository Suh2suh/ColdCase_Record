using UnityEngine;
using UnityEngine.UI;

namespace TRS.CaptureTool
{
    // Obsolete: Finalizers/Deconstructors (~ deinit methods) are not called consistently.
    // Constructor/deconstructor subscriptions were used for subscribing objects that may not be active on scene load.
    // Instead, a higher level active parent with a version of this script will enable and disable a child object.
    [System.Obsolete("SaveScreenshotPanelScript is deprecated. Please use SaveScreenshotUIScript.")]
    public class Custom_SaveScreenshotPanel : MonoBehaviour
    {
        [Tooltip("Optional. If not set, will use the ScreenshotScript from the ScreenshotTaken action.")]
        public ScreenshotScript screenshotScript;

        [Tooltip("If set, will activate this GameObject automatically when a screenshot is taken.")]
        public bool displayAutomatically = true;

        [Tooltip("If set, will deactivate this GameObject automatically when a screenshot is saved.")]
        public bool hideOnSave = true;

        [Tooltip("If set, will unlock the cursor and set to visible to enable interactions with the panel and reset to original status when closing the panel.")]
        public bool unlockCursor = true;

        protected Texture2D screenshot;
        protected CursorLockMode originalCursorLockMode;
        protected bool originalCursorVisibility;

        public Custom_SaveScreenshotPanel()
        {
            ScreenshotScript.ScreenshotTaken += ScreenshotTaken;
        }

        ~Custom_SaveScreenshotPanel()
        {
            ScreenshotScript.ScreenshotTaken -= ScreenshotTaken;
        }

        void ScreenshotTaken(ScreenshotScript screenshotScript, Texture2D screenshotTaken)
        {
            if (this == null)
            {
                ScreenshotScript.ScreenshotTaken -= ScreenshotTaken;
                DestroyImmediate(this);
                return;
            }

            if (displayAutomatically)
            {
                if (unlockCursor && !gameObject.activeSelf)
                {
                    originalCursorLockMode = Cursor.lockState;
                    originalCursorVisibility = Cursor.visible;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                gameObject.SetActive(true);
            }

            if (this.screenshotScript == null) this.screenshotScript = screenshotScript;
            screenshot = screenshotTaken;
        }



        // 바로 저장 ㅋㅋ
        public virtual void SaveScreenshot()
        {
            screenshotScript.lastScreenshotTexture = screenshot;
#if UNITY_EDITOR
            screenshotScript.textureToEdit = screenshot;
#endif

            screenshotScript.Save(screenshot, "", false);
            screenshotScript.fileSettings.IncrementCount();
            screenshotScript.fileSettings.SaveCount();

            //if (hideOnSave) HidePanel();
        }



        public void Cancel()
        {
            //HidePanel();
        }

        void HidePanel()
        {
            if (unlockCursor)
            {
                Cursor.lockState = originalCursorLockMode;
                Cursor.visible = originalCursorVisibility;
            }
            gameObject.SetActive(false);
        }
    }
}