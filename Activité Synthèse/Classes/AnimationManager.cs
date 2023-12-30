using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.IO;

namespace Activité_Synthèse
{
    internal class AnimationManager
    {

        public static void Start(string animtionName, int startFrame, float speed)
        {
        }
    }

    class Animation
    {
        public string name;
        public List<string> images;
        public int inBetweenFrames;

        public Animation(string name, List<string> images, int inBetweenFrames)
        {
            this.name = name;
            this.images = images;
            this.inBetweenFrames = inBetweenFrames;
        }

        public void WriteAnimation(Animation animation)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter($"Animations/{animation.name}.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, animation);
            }
        }
    }
}
