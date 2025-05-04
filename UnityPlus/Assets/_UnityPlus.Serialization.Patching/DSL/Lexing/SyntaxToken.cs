namespace UnityPlus.Serialization.Patching.DSL.Lexing
{
    public struct SyntaxToken
    {
        public readonly SyntaxTokenType type;
        public readonly string value;

        public SyntaxToken( SyntaxTokenType type, string value )
        {
            this.type = type;
            this.value = value;
        }

        public SyntaxToken( SyntaxTokenType type, char value )
        {
            this.type = type;
            this.value = value.ToString();
        }
    }
}