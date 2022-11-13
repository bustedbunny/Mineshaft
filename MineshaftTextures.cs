using UnityEngine;
using Verse;

namespace Mineshaft
{
    [StaticConstructorOnStartup]
    public static class MineshaftTextures
    {
        public static Texture2D Force_Exit = ContentFinder<Texture2D>.Get("UI/MineshaftButtons/Force_Exit");
        public static Texture2D Resume = ContentFinder<Texture2D>.Get("UI/MineshaftButtons/Resume");
        public static Texture2D Stop = ContentFinder<Texture2D>.Get("UI/MineshaftButtons/Stop");
        public static Texture2D MinerIcon = ContentFinder<Texture2D>.Get("UI/MineshaftButtons/MinerIcon");
    }
}
