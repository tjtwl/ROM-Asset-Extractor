using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomAssetExtractor.Pokemon.Entities
{
    public class AnimationCommand
    {
        public static AnimationCommand ReadCommand(PokemonRomReader reader)
        {
            var type = reader.ReadShort();

            if (type == -3)
                return AnimationLoopCommand.ReadLoopCommand(reader);
            if (type == -2)
                return AnimationJumpCommand.ReadJumpCommand(reader);
            else if (type == -1)
                return AnimationEndCommand.ReadEndCommand(reader);

            return AnimationFrameCommand.ReadFrameCommand(reader, type);
        }
    }

    public class AnimationFrameCommand : AnimationCommand
    {
        public uint ImageValue { get; private set; }
        public uint Duration { get; private set; }
        public bool FlippedHorizontally { get; private set; }
        public bool FlippedVertically { get; private set; }

        public AnimationFrameCommand(uint imageValue, uint duration, bool flippedHorizontally, bool flippedVertically)
        {
            ImageValue = imageValue;
            Duration = duration;
            FlippedHorizontally = flippedHorizontally;
            FlippedVertically = flippedVertically;
        }

        public static AnimationFrameCommand ReadFrameCommand(PokemonRomReader reader, short imageValue)
        {
            var flags = reader.ReadByte();
            reader.Skip(1);
            var duration = flags >> 2;
            var flippedVertically = (flags & 0b00000010) >> 1;
            var flippedHorizontally = (flags & 0b00000001) >> 0;

            return new AnimationFrameCommand((uint)imageValue, (uint)duration, flippedHorizontally == 1, flippedVertically == 1);
        }
    }

    public class AnimationLoopCommand : AnimationCommand
    {
        public int Count { get; private set; }

        public AnimationLoopCommand(int count)
        {
            this.Count = count;
        }

        public static AnimationLoopCommand ReadLoopCommand(PokemonRomReader reader)
        {
            var count = reader.ReadByte();
            reader.Skip(1);

            return new AnimationLoopCommand(count >> 2);
        }
    }

    public class AnimationJumpCommand : AnimationCommand
    {
        public int Target { get; private set; }

        public AnimationJumpCommand(int count)
        {
            this.Target = count;
        }

        public static AnimationJumpCommand ReadJumpCommand(PokemonRomReader reader)
        {
            var target = reader.ReadByte();
            reader.Skip(1);

            return new AnimationJumpCommand(target >> 2);
        }
    }

    public class AnimationEndCommand : AnimationCommand
    {
        public AnimationEndCommand()
        { }

        public static AnimationEndCommand ReadEndCommand(PokemonRomReader reader)
        {
            reader.Skip(2);
            return new AnimationEndCommand();
        }
    }
}
