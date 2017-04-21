using UnityEngine;
using UnityEditor;

namespace Editor
{
    public class GameViewTools : ScriptableObject
    {
        public static string fileName = "Screenshots/Game View Screenshot ";
        public static int startNumber = 1;

        // This does need unity to be focused on the game window!
        [MenuItem("Custom tools/Game View: Take Screenshot %g")]
        static void TakeScreenshot()
        {
            int number = startNumber;
            string name = "" + number;

            while (System.IO.File.Exists(fileName + name + ".png"))
            {
                number++;
                name = "" + number;
            }

            startNumber = number + 1;

            Application.CaptureScreenshot(fileName + name + ".png");
        }
    }
}