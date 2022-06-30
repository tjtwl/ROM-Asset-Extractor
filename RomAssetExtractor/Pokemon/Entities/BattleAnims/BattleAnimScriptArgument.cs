namespace RomAssetExtractor.Pokemon.Entities
{
    public class BattleAnimScriptArgument
    {
        internal readonly int TypeSize;
        internal readonly bool IsVarArg;

        private readonly string argumentName;
        private readonly bool isRequired;

        public BattleAnimScriptArgument(string argumentName, int typeSize, bool isRequired = true, bool isVarArg = false)
        {
            this.IsVarArg = isVarArg;
            this.TypeSize = typeSize;

            this.argumentName = argumentName;
            this.isRequired = isRequired;
        }
    }
}
