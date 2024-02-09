namespace Adletec.Sonic.Parsing.Tokenizing
{
    /// <summary>
    /// Represents an input token
    /// </summary>
    public struct Token
    {
        /// <summary>
        /// The start position of the token in the input function text. For quoted tokens (e.g.'foo bar' in "a + 'foo bar'"), this is the position of the opening quote.
        /// </summary>
        public int StartPosition;

        /// <summary>
        /// The length of token in the input function text. For quoted tokens (e.g.'foo bar' in "a + 'foo bar'"), this is the length of the token _including_ the opening and closing quotes.
        /// </summary>
        public int Length;

        /// <summary>
        /// The type of the token.
        /// </summary>
        public TokenType TokenType;

        /// <summary>
        /// The value of the token. For quoted tokens (e.g.'foo bar' in "a + 'foo bar'"), this is the token _without_ the opening and closing quotes. This is so quoted and unquoted tokens evaluate to the same value (variable, constant, function).
        /// </summary>
        public object Value;
    }
}