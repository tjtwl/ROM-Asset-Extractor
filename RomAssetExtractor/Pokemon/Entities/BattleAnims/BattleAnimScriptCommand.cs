using RomAssetExtractor.GbaSystem;
using System.Collections.Generic;

namespace RomAssetExtractor.Pokemon.Entities
{
    public class BattleAnimScriptCommand
    {
        private const string SCRIPT_END = "end";
        private const string SCRIPT_RETURN = "return";
        
        public CommandByte Byte { get; private set; }
        internal readonly BattleAnimScriptArgument[] Arguments;
        internal readonly string Name;
        private readonly bool endsScript;

        public BattleAnimScriptCommand(string commandName, params BattleAnimScriptArgument[] arguments)
        {
            this.Name = commandName;
            this.Arguments = arguments;

            if (this.Name == SCRIPT_END)
                this.endsScript = true;

            if (this.Name == SCRIPT_RETURN)
                this.endsScript = true;
        }

        public static BattleAnimScriptCommand Get(CommandByte command)
        {
            Commands[command].Byte = command;
            return Commands[command];
        }

        public static BattleAnimScriptCommand Get(byte command)
            => Get((CommandByte)command);

        internal bool TryRead(PokemonRomReader reader, out BattleAnimScriptCall scriptCall)
        {
            scriptCall = new BattleAnimScriptCall(this);

            if (endsScript)
                return false;

            var arguments = new List<object>();
            for (int i = 0; i < Arguments.Length; i++)
            {
                var numArguments = Arguments[i].IsVarArg ? reader.ReadByte() : 1;

                for (int a = 0; a < numArguments; a++)
                {
                     arguments.Add(reader.ReadBytesAsInt(Arguments[i].TypeSize));
                }
            }

            scriptCall.Arguments = arguments.ToArray();

            return true;
        }

        /*
         * Source: battle_anim_script.inc from pokefirered (https://github.com/pret/pokefirered)
         */
        public enum CommandByte
        {
            LoadSpriteGfx = 0x0,
            UnloadSpriteGfx = 0x1,
            CreateSprite = 0x02,
            CreateVisualTask = 0x03,
            Delay = 0x4,
            WaitForVisualFinish = 0x5,
            Hang1 = 0x6,
            Hang2 = 0x7,
            End = 0x8,
            PlaySe = 0x9,
            MonBg = 0xa,
            ClearMonBg = 0xb,
            SetAlpha = 0x0C,
            BlendOff = 0xd,
            Call = 0xe,
            Return = 0xf,
            SetArg = 0x10,
            ChooseTwoTurnAnim = 0x11,
            JumpIfMoveTurn = 0x12,
            GoTo = 0x13,
            FadeToBg = 0x14,
            RestoreBg = 0x15,
            WaitBgFadeOut = 0x16,
            WaitBgFadeIn = 0x17,
            ChangeBg = 0x18,
            PlaySeWithPan = 0x19,
            SetPan = 0x1a,
            PanSe1B = 0x1b,
            LoopSeWithPan = 0x1c,
            WaitPlaySeWithPan = 0x1d,
            SetBldcnt = 0x1e,
            CreateSoundTask = 0x1F,
            WaitSound = 0x20,
            JumpArgEq = 0x21,
            MonBg22 = 0x22,
            ClearMonBg23 = 0x23,
            JumpIfContest = 0x24,
            FadeToBgFromSet = 0x25,
            PanSe26 = 0x26,
            PanSe27 = 0x27,
            MonBgPrio28 = 0x28,
            MonBgPrio29 = 0x29,
            MonBgPrio2A = 0x2a,
            Invisible = 0x2b,
            Visible = 0x2c,
            DoubleBattle2D = 0x2d,
            DoubleBattle2E = 0x2e,
            StopSound = 0x2f,
        }
        private static readonly Dictionary<CommandByte, BattleAnimScriptCommand> Commands = new Dictionary<CommandByte, BattleAnimScriptCommand>
        {
            { CommandByte.LoadSpriteGfx, new BattleAnimScriptCommand("loadspritegfx",
                new BattleAnimScriptArgument("tag", sizeof(short))
                )
            },
            { CommandByte.UnloadSpriteGfx, new BattleAnimScriptCommand("unloadspritegfx",
                new BattleAnimScriptArgument("tag", sizeof(short))
                )
            },
            { CommandByte.CreateSprite, new BattleAnimScriptCommand("createsprite",
                new BattleAnimScriptArgument("template", Pointer.GetSize()),
                new BattleAnimScriptArgument("anim_battler_OR_subpriority_offset", sizeof(byte)),
                new BattleAnimScriptArgument("Lsprites", sizeof(short), isVarArg: true)
                )
            },
            { CommandByte.CreateVisualTask, new BattleAnimScriptCommand("createvisualtask",
                new BattleAnimScriptArgument("addr", Pointer.GetSize()),
                new BattleAnimScriptArgument("priority", sizeof(byte)),
                new BattleAnimScriptArgument("Lcreatetasks", sizeof(short), isVarArg: true)
                )
            },
            { CommandByte.Delay, new BattleAnimScriptCommand("delay",
                new BattleAnimScriptArgument("param0", sizeof(byte))
                )
            },
            { CommandByte.WaitForVisualFinish, new BattleAnimScriptCommand("waitforvisualfinish") },
            { CommandByte.Hang1, new BattleAnimScriptCommand("hang1") },
            { CommandByte.Hang2, new BattleAnimScriptCommand("hang2") },
            { CommandByte.End, new BattleAnimScriptCommand(SCRIPT_END) },
            { CommandByte.PlaySe, new BattleAnimScriptCommand("playse",
                new BattleAnimScriptArgument("se", sizeof(short))
                )
            },
            { CommandByte.MonBg, new BattleAnimScriptCommand("monbg",
                new BattleAnimScriptArgument("battler", sizeof(byte))
                )
            },
            { CommandByte.ClearMonBg, new BattleAnimScriptCommand("clearmonbg",
                new BattleAnimScriptArgument("battler", sizeof(byte))
                )
            },
            { CommandByte.SetAlpha, new BattleAnimScriptCommand("setalpha",
                new BattleAnimScriptArgument("eva_OR_evb", sizeof(short))
                )
            },
            { CommandByte.BlendOff, new BattleAnimScriptCommand("blendoff") },
            { CommandByte.Call, new BattleAnimScriptCommand("call",
                new BattleAnimScriptArgument("param0", Pointer.GetSize())
                )
            },
            { CommandByte.Return, new BattleAnimScriptCommand(SCRIPT_RETURN) },
            { CommandByte.SetArg, new BattleAnimScriptCommand("setarg",
                new BattleAnimScriptArgument("param0", sizeof(byte)),
                new BattleAnimScriptArgument("param1", sizeof(short))
                )
            },
            { CommandByte.ChooseTwoTurnAnim, new BattleAnimScriptCommand("choosetwoturnanim",
                new BattleAnimScriptArgument("param0", Pointer.GetSize()),
                new BattleAnimScriptArgument("param1", Pointer.GetSize())
                )
            },
            { CommandByte.JumpIfMoveTurn, new BattleAnimScriptCommand("jumpifmoveturn",
                new BattleAnimScriptArgument("param0", sizeof(byte)),
                new BattleAnimScriptArgument("ptr", Pointer.GetSize())
                )
            },
            { CommandByte.GoTo, new BattleAnimScriptCommand("goto",
                new BattleAnimScriptArgument("ptr", Pointer.GetSize())
                )
            },
            { CommandByte.FadeToBg, new BattleAnimScriptCommand("fadetobg",
                new BattleAnimScriptArgument("bg", sizeof(byte))
                )
            },
            { CommandByte.RestoreBg, new BattleAnimScriptCommand("restorebg") },
            { CommandByte.WaitBgFadeOut, new BattleAnimScriptCommand("waitbgfadeout") },
            { CommandByte.WaitBgFadeIn, new BattleAnimScriptCommand("waitbgfadein") },
            { CommandByte.ChangeBg, new BattleAnimScriptCommand("changebg",
                new BattleAnimScriptArgument("bg", sizeof(byte))
                )
            },
            { CommandByte.PlaySeWithPan, new BattleAnimScriptCommand("playsewithpan",
                new BattleAnimScriptArgument("se", sizeof(short)),
                new BattleAnimScriptArgument("pan", sizeof(byte))
                )
            },
            { CommandByte.SetPan, new BattleAnimScriptCommand("setpan",
                new BattleAnimScriptArgument("pan", sizeof(byte))
                )
            },
            { CommandByte.PanSe1B, new BattleAnimScriptCommand("panse_1B",
                new BattleAnimScriptArgument("se", sizeof(short)),
                new BattleAnimScriptArgument("param1", sizeof(byte)),
                new BattleAnimScriptArgument("param2", sizeof(byte)),
                new BattleAnimScriptArgument("param3", sizeof(byte)),
                new BattleAnimScriptArgument("param4", sizeof(byte))
                )
            },
            { CommandByte.LoopSeWithPan, new BattleAnimScriptCommand("loopsewithpan",
                new BattleAnimScriptArgument("se", sizeof(short)),
                new BattleAnimScriptArgument("pan", sizeof(byte)),
                new BattleAnimScriptArgument("wait", sizeof(byte)),
                new BattleAnimScriptArgument("times", sizeof(byte))
                )
            },
            { CommandByte.WaitPlaySeWithPan, new BattleAnimScriptCommand("waitplaysewithpan",
                new BattleAnimScriptArgument("se", sizeof(short)),
                new BattleAnimScriptArgument("pan", sizeof(byte)),
                new BattleAnimScriptArgument("wait", sizeof(byte))
                )
            },
            { CommandByte.SetBldcnt, new BattleAnimScriptCommand("setbldcnt",
                new BattleAnimScriptArgument("param0", sizeof(short))
                )
            },
            { CommandByte.CreateSoundTask, new BattleAnimScriptCommand("createsoundtask",
                new BattleAnimScriptArgument("addr", Pointer.GetSize()),
                new BattleAnimScriptArgument("Lcreatetasks_1F", sizeof(short), isVarArg: true)
                )
            },
            { CommandByte.WaitSound, new BattleAnimScriptCommand("waitsound") },
            { CommandByte.JumpArgEq, new BattleAnimScriptCommand("jumpargeq",
                new BattleAnimScriptArgument("param0", sizeof(byte)),
                new BattleAnimScriptArgument("param1", sizeof(short)),
                new BattleAnimScriptArgument("ptr", Pointer.GetSize())
                )
            },
            { CommandByte.MonBg22, new BattleAnimScriptCommand("monbg_22",
                new BattleAnimScriptArgument("battler", sizeof(byte))
                )
            },
            { CommandByte.ClearMonBg23, new BattleAnimScriptCommand("clearmonbg_23",
                new BattleAnimScriptArgument("battler", sizeof(byte))
                )
            },
            { CommandByte.JumpIfContest, new BattleAnimScriptCommand("jumpifcontest",
                new BattleAnimScriptArgument("ptr", Pointer.GetSize())
                )
            },
            { CommandByte.FadeToBgFromSet, new BattleAnimScriptCommand("fadetobgfromset",
                new BattleAnimScriptArgument("param0", sizeof(byte)),
                new BattleAnimScriptArgument("param1", sizeof(byte)),
                new BattleAnimScriptArgument("param2", sizeof(byte))
                )
            },
            { CommandByte.PanSe26, new BattleAnimScriptCommand("panse_26",
                new BattleAnimScriptArgument("se", sizeof(short)),
                new BattleAnimScriptArgument("param1", sizeof(byte)),
                new BattleAnimScriptArgument("param2", sizeof(byte)),
                new BattleAnimScriptArgument("param3", sizeof(byte)),
                new BattleAnimScriptArgument("param4", sizeof(byte))
                )
            },
            { CommandByte.PanSe27, new BattleAnimScriptCommand("panse_27",
                new BattleAnimScriptArgument("se", sizeof(short)),
                new BattleAnimScriptArgument("param1", sizeof(byte)),
                new BattleAnimScriptArgument("param2", sizeof(byte)),
                new BattleAnimScriptArgument("param3", sizeof(byte)),
                new BattleAnimScriptArgument("param4", sizeof(byte))
                )
            },
            { CommandByte.MonBgPrio28, new BattleAnimScriptCommand("monbgprio_28",
                new BattleAnimScriptArgument("battler", sizeof(byte))
                )
            },
            { CommandByte.MonBgPrio29, new BattleAnimScriptCommand("monbgprio_29") },
            { CommandByte.MonBgPrio2A, new BattleAnimScriptCommand("monbgprio_2A",
                new BattleAnimScriptArgument("battler", sizeof(byte))
                )
            },
            { CommandByte.Invisible, new BattleAnimScriptCommand("invisible",
                new BattleAnimScriptArgument("battler", sizeof(byte))
                )
            },
            { CommandByte.Visible, new BattleAnimScriptCommand("visible",
                new BattleAnimScriptArgument("battler", sizeof(byte))
                )
            },
            { CommandByte.DoubleBattle2D, new BattleAnimScriptCommand("doublebattle_2D",
                new BattleAnimScriptArgument("battler", sizeof(byte))
                )
            },
            { CommandByte.DoubleBattle2E, new BattleAnimScriptCommand("doublebattle_2E",
                new BattleAnimScriptArgument("battler", sizeof(byte))
                )
            },
            { CommandByte.StopSound, new BattleAnimScriptCommand("stopsound") },
        };
    }
}
