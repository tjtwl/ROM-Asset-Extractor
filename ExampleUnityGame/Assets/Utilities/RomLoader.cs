using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ExampleUnityGame
{
    [RequireComponent(typeof(Button))]
    public class RomLoader : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnClickLoadRom);
        }

        private void OnClickLoadRom()
        {
            var dataPath = Application.dataPath;

            Task.Run(async () =>
            {
                try
                {
                    var romPath = Path.Combine(dataPath, "../../../test_roms/pokemon_firered.gba");
                    var outputDirectory = Path.Combine(dataPath, ".RomCache");

                    using var debugWriter = new DebugTextWriter();
                    await RomAssetExtractor.AssetExtractor.ExtractRom(romPath, outputDirectory,
                        saveBitmaps: false,
                        saveMapRenders: false,
                        yamlPath: Path.Combine(dataPath, "pokeroms.yml"),
                        logWriter: debugWriter);
                }
                catch(Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            });
        }
    }
}
