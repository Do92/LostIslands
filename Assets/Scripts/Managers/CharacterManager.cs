using Miscellaneous;
using Game.Networking;

namespace Managers
{
    public class CharacterManager : Singleton<CharacterManager>
    {
        // Array of all available characters, can only be adjusted in the editor
        public CharacterInfo[] CharacterCollection;

        private void Awake()
        {
            for (int i = 0; i < CharacterCollection.Length; i++)
                CharacterCollection[i].Id = i;
        }

        public CharacterInfo GetCharacter(int id)
        {
            return CharacterCollection[id];
        }

        public int GetCharacterCount()
        {
            return CharacterCollection.Length;
        }
    }
}