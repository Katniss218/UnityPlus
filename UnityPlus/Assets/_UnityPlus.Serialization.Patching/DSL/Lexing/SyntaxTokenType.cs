namespace UnityPlus.Serialization.Patching.DSL.Lexing
{
    public enum SyntaxTokenType
    {
        OPENING_PARENTHESIS,
        CLOSING_PARENTHESIS,
        OPENING_SQUARE_BRACKET,
        CLOSING_SQUARE_BRACKET,
        OPENING_CURLY_BRACKET,
        CLOSING_CURLY_BRACKET,
        OPENING_QUOTE,
        CLOSING_QUOTE,
        DOT,
        PLUS,
        MINUS,
        ASTERISK,
        SLASH,
        BACKSLASH,
        EQUALS,
        SEMICOLON,
        DOLLAR,
        HASH,
        AT,

        // symbol instead?

        FROM_CLAUSE,
        WHERE_CLAUSE,

        COMMENT,

        IDENTIFIER_SEGMENT,


        QUOTE,
        TEXT,
        DIGIT
    }
}