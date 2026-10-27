using UnityEngine;

namespace Game.Utils
{
    public enum CustomPlatform
    {
        PC,
        Mobile,
        Web,
        Undefined
    }

    public static class PlatformUtils
    {
        public static CustomPlatform GetPlatform()
        {
            RuntimePlatform currentPlatform = Application.platform;
            CustomPlatform customCustomPlatform = CustomPlatform.Undefined;
            switch (currentPlatform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.PS5:
                case RuntimePlatform.PS4:
                case RuntimePlatform.XboxOne:
                case RuntimePlatform.Switch:
                    customCustomPlatform = CustomPlatform.PC;
                    break;
                case RuntimePlatform.Android:
                case RuntimePlatform.IPhonePlayer:
                    customCustomPlatform = CustomPlatform.Mobile;
                    break;
                case RuntimePlatform.WebGLPlayer:
                    customCustomPlatform = CustomPlatform.Web;
                    break;
            }

            return customCustomPlatform;
        }
    }
}
