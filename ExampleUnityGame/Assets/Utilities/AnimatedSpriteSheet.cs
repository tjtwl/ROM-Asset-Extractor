using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ExampleUnityGame
{
    public class AnimatedSpriteSheet : MonoBehaviour
    {
        public string spriteSheetPath;

        [Tooltip("Order of these sprites must match subsprites in the texture. So if idle_north is 0, the first subsprite in the texture should be idle_north.")]
        public Sprite[] placeholderSprites;

        public int FrameCount => placeholderSprites.Length;

        protected Dictionary<string, Sprite> frameSprites;

        void Start()
        {
            var pixels = File.ReadAllBytes(Path.Combine(Application.dataPath, spriteSheetPath));
            var texture = new Texture2D(0, 0); // size will get calculated from pixels
            texture.filterMode = FilterMode.Point;
            texture.LoadImage(pixels);

            var widthPerFrame = texture.width / FrameCount;
            frameSprites = new Dictionary<string, Sprite>(FrameCount);

            for (int i = 0; i < FrameCount; i++)
            {
                var sprite = Sprite.Create(texture,
                    new Rect(i * widthPerFrame, 0, widthPerFrame, texture.height),
                    new Vector2(.5f, .5f),
                    8,
                    0,
                    SpriteMeshType.FullRect,
                    Vector4.zero
                );
                sprite.name = placeholderSprites[i].name;

                frameSprites.Add(sprite.name, sprite);
            }
        }

        public Sprite GetSprite(string name)
        {
            if(frameSprites.ContainsKey(name))
                return frameSprites[name];

            return null;
        }
    }
}
