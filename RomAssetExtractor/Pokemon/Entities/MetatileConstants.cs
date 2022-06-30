using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomAssetExtractor.Pokemon.Entities
{
    public enum MetatileBackground
    {
        // Source of these comments: pokefirered\src\field_camera.c
        // LAYER_TYPE_NORMAL
        // Draw garbage to the bottom background layer.
        // Draw metatile's bottom layer to the middle background layer.
        // Draw metatile's top layer to the top background layer, which covers object event sprites.
        OnlyTopCoversPlayer = 0x00,

        // LAYER_TYPE_COVERED_BY_OBJECTS
        // Draw metatile's bottom layer to the bottom background layer.
        // Draw metatile's top layer to the middle background layer.
        // Draw transparent tiles to the top background layer.
        CoveredByObjects = 0x01,

        // LAYER_TYPE_ ???
        // Draw metatile's bottom layer to the bottom background layer.
        // Draw transparent tiles to the middle background layer.
        // Draw metatile's top layer to the top background layer.
        UnknownLayerType = 0x02,

        Overlay = 0x20,
        UNKNOWN_Overlay = 0x21,

        StrongWaterCurrent = 0x02,
        Water = 0x22,

    };

    public enum MetatileEncounter
    {
        None = 0,       // 0b00
        Land = 1,       // 0b01
        Water = 2,      // 0b10
        WaterFall = 3,  // 0b11
    };
}
