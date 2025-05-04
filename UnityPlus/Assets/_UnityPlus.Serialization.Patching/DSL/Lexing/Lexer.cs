using System.Collections.Generic;

namespace UnityPlus.Serialization.Patching.DSL.Lexing
{

    public class Lexer
    {
        // Multiple-pass lexer

        public Lexer() { }

        public IEnumerable<SyntaxToken> Lex( string str )
        {
            List<SyntaxToken> tokens = FirstPass( str );

            tokens = SecondPass( tokens );



            return tokens;
        }

        private List<SyntaxToken> FirstPass( string str )
        {
            List<SyntaxToken> tokens = new();

            for( int i = 0; i < str.Length; i++ )
            {
                SyntaxToken currentToken;

                char c = str[i];

                if( char.IsWhiteSpace( c ) )
                {
                    // currentToken = new SyntaxToken( SyntaxTokenType.WHITESPACE, c );
                    // skip whitespace.
                    continue;
                }

                switch( c )
                {
                    case '{':
                        currentToken = new SyntaxToken( SyntaxTokenType.OPENING_CURLY_BRACKET, c );
                        break;
                    case '}':
                        currentToken = new SyntaxToken( SyntaxTokenType.CLOSING_CURLY_BRACKET, c );
                        break;
                    case '[':
                        currentToken = new SyntaxToken( SyntaxTokenType.OPENING_SQUARE_BRACKET, c );
                        break;
                    case ']':
                        currentToken = new SyntaxToken( SyntaxTokenType.CLOSING_SQUARE_BRACKET, c );
                        break;
                    case '(':
                        currentToken = new SyntaxToken( SyntaxTokenType.OPENING_PARENTHESIS, c );
                        break;
                    case ')':
                        currentToken = new SyntaxToken( SyntaxTokenType.CLOSING_PARENTHESIS, c );
                        break;
                    case ';':
                        currentToken = new SyntaxToken( SyntaxTokenType.SEMICOLON, c );
                        break;
                    case '+':
                        currentToken = new SyntaxToken( SyntaxTokenType.PLUS, c );
                        break;
                    case '-':
                        currentToken = new SyntaxToken( SyntaxTokenType.MINUS, c );
                        break;
                    case '*':
                        currentToken = new SyntaxToken( SyntaxTokenType.ASTERISK, c );
                        break;
                    case '/':
                        currentToken = new SyntaxToken( SyntaxTokenType.SLASH, c );
                        break;
                    case '\\':
                        currentToken = new SyntaxToken( SyntaxTokenType.BACKSLASH, c );
                        break;
                    case '=':
                        currentToken = new SyntaxToken( SyntaxTokenType.EQUALS, c );
                        break;
                    case '.':
                        currentToken = new SyntaxToken( SyntaxTokenType.DOT, c );
                        break;
                    case '$':
                        currentToken = new SyntaxToken( SyntaxTokenType.DOLLAR, c );
                        break;
                    case '#':
                        currentToken = new SyntaxToken( SyntaxTokenType.HASH, c );
                        break;
                    case '@':
                        currentToken = new SyntaxToken( SyntaxTokenType.AT, c );
                        break;

                    case '"':
                        currentToken = new SyntaxToken( SyntaxTokenType.QUOTE, c );
                        break;

                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        currentToken = new SyntaxToken( SyntaxTokenType.DIGIT, c );
                        break;

                    default:
                        currentToken = new SyntaxToken( SyntaxTokenType.TEXT, c );
                        break;
                }

                tokens.Add( currentToken );
            }

            return tokens;
        }
    
        private List<SyntaxToken> SecondPass( List<SyntaxToken> tokens )
        {


            return tokens;
        }
    }
}