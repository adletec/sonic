using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Adletec.Sonic.Parsing.Tokenizing;

namespace Adletec.Sonic.Parsing
{
    /// <summary>
    /// A token reader that converts the input string in a list of tokens.
    /// </summary>
    public class TokenReader
    {
        private readonly CultureInfo cultureInfo;
        private readonly char decimalSeparator;
        private readonly char argumentSeparator;

        /// <summary>
        /// Default constructor. Uses the InvariantCulture and ',' as argument separator.
        /// </summary>
        public TokenReader() 
            : this(CultureInfo.InvariantCulture, ',')
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cultureInfo">The culture info to use for parsing the floating point numbers.</param>
        /// <param name="argumentSeparator">The argument separator to use for functions.</param>
        /// <exception cref="ArgumentException">Thrown when the argument separator is the same as the decimal separator.</exception>
        public TokenReader(CultureInfo cultureInfo, char argumentSeparator)
        {
            this.cultureInfo = cultureInfo;
            this.decimalSeparator = cultureInfo.NumberFormat.NumberDecimalSeparator[0];
            this.argumentSeparator = argumentSeparator;
            
            if (cultureInfo.NumberFormat.NumberDecimalSeparator.ToCharArray(0, 1)[0] == argumentSeparator)
            {
                throw new ArgumentException(nameof(argumentSeparator) + " cannot be the same as " +
                                            nameof(cultureInfo.NumberFormat.NumberDecimalSeparator), nameof(argumentSeparator));
            }
        }

        /// <summary>
        /// Read in the provided expression and convert it into a list of tokens that can be processed by the
        /// Abstract Syntax Tree Builder.
        /// </summary>
        /// <param name="expression">The expression to be converted into a list of tokens.</param>
        /// <returns>The list of tokens for the provided expression.</returns>
        public List<Token> Read(string expression)
        {
            if (string.IsNullOrEmpty(expression))
                throw new ArgumentNullException(nameof(expression));

            var tokens = new List<Token>();

            var characters = expression.ToCharArray();

            var isFormulaSubPart = true;
            var isScientific = false;

            for(var i = 0; i < characters.Length; i++)
            {
                if (IsPartOfNumeric(characters[i], true, false, isFormulaSubPart))
                {
                    if (characters[i] == '-')
                    {
                        tokens.Add(new Token { TokenType = TokenType.Operation, Value = '_', StartPosition = i, Length = 1 });
                        continue;
                    }
                    
                    var buffer = new StringBuilder();
                    buffer.Append(characters[i]);
                    var startPosition = i;
                                       

                    while (++i < characters.Length && IsPartOfNumeric(characters[i], false, characters[i-1] == '-', isFormulaSubPart))
                    {
                        if (isScientific && IsScientificNotation(characters[i]))
                            throw new InvalidTokenParseException($"Invalid token \"{characters[i]}\" detected at position {i}.", expression, i, characters[i].ToString());

                        if (IsScientificNotation(characters[i]))
                        {
                            isScientific = IsScientificNotation(characters[i]);

                            if (characters.Length > i + 1 && characters[i + 1] == '-')
                            {
                                buffer.Append(characters[i++]);
                            }
                        }

                        buffer.Append(characters[i]);
                    }

                    // Verify if we do not have an int
                    if (int.TryParse(buffer.ToString(), out var intValue))
                    {
                        tokens.Add(new Token { TokenType = TokenType.Integer, Value = intValue, StartPosition = startPosition, Length = i - startPosition });
                        isFormulaSubPart = false;
                    }
                    else
                    {
                        if (double.TryParse(buffer.ToString(), NumberStyles.Float | NumberStyles.AllowThousands,
                            cultureInfo, out var doubleValue))
                        {
                            tokens.Add(new Token { TokenType = TokenType.FloatingPoint, Value = doubleValue, StartPosition = startPosition, Length = i - startPosition });
                            isScientific = false;
                            isFormulaSubPart = false;
                        }
                        else
                        {
                            throw new InvalidFloatingPointNumberParseException($"Invalid floating point number: {buffer}",
                                expression, startPosition, buffer.ToString());
                        }
                    }

                    if (i == characters.Length)
                    {
                        // Last character read
                        continue;
                    }
                }

                if (IsPartOfTextToken(characters[i], true))
                {
                    var buffer = "" + characters[i];
                    var startPosition = i;

                    var nextCharIndex = i + 1;
                    while (nextCharIndex < characters.Length && IsPartOfTextToken(characters[nextCharIndex], false))
                    {
                        buffer += characters[++i];
                        nextCharIndex = i + 1;
                    }

                    // exclusive end (the first char after the token)
                    var textTokenEnd = nextCharIndex;

                    // Find next non-whitespace character index, so we can check if it is an opening parenthesis
                    // which would make our text token to a function.
                    while (characters.Length > nextCharIndex && char.IsWhiteSpace(characters[nextCharIndex]))
                    {
                        nextCharIndex++;
                    }
                    
                    if (characters.Length > nextCharIndex && characters[nextCharIndex] == '(')
                    {
                        // We know the next token already, so we can process it and set the index accordingly
                        i = nextCharIndex;
                        tokens.Add(new Token { TokenType = TokenType.Function, Value = buffer, StartPosition = startPosition, Length = textTokenEnd - startPosition });
                        tokens.Add(new Token { TokenType = TokenType.LeftParenthesis, Value = '(', StartPosition = i, Length = 1 });
                        isFormulaSubPart = true;
                        continue;
                    }

                    tokens.Add(new Token { TokenType = TokenType.Symbol, Value = buffer, StartPosition = startPosition, Length = textTokenEnd - startPosition });
                    isFormulaSubPart = false;

                    continue;
                }
                if (characters[i] == this.argumentSeparator)
                {
                    tokens.Add(new Token { TokenType = TokenType.ArgumentSeparator, Value = characters[i], StartPosition = i, Length = 1 });
                    isFormulaSubPart = false;
                }
                else
                {
                    switch (characters[i])
                    { 
                        case ' ':
                            continue;
                        case '+':
                        case '*':
                        case '/':
                        case '^':
                        case '%':
                        case '≤':
                        case '≥':
                        case '≠':
                            tokens.Add(new Token { TokenType = TokenType.Operation, Value = characters[i], StartPosition = i, Length = 1 });                            
                            isFormulaSubPart = true;
                            break;
                        case '-':
                            if (IsUnaryMinus(characters[i], tokens))
                            {
                                // We use the token '_' for a unary minus in the AST builder
                                tokens.Add(new Token { TokenType = TokenType.Operation, Value = '_', StartPosition = i, Length = 1 });
                            }
                            else
                            {
                                tokens.Add(new Token { TokenType = TokenType.Operation, Value = characters[i], StartPosition = i, Length = 1 });                            
                            }
                            isFormulaSubPart = true;
                            break;
                        case '(':
                            tokens.Add(new Token { TokenType = TokenType.LeftParenthesis, Value = characters[i], StartPosition = i, Length = 1 });
                            isFormulaSubPart = true;
                            break;
                        case ')':
                            tokens.Add(new Token { TokenType = TokenType.RightParenthesis, Value = characters[i], StartPosition = i, Length = 1 });
                            isFormulaSubPart = false;
                            break;
                        case '<':
                            if (i + 1 < characters.Length && characters[i + 1] == '=')
                                tokens.Add(new Token { TokenType = TokenType.Operation, Value = '≤', StartPosition = i++, Length = 2 });
                            else
                                tokens.Add(new Token { TokenType = TokenType.Operation, Value = '<', StartPosition = i, Length = 1 });
                            isFormulaSubPart = false;
                            break;
                        case '>':
                            if (i + 1 < characters.Length && characters[i + 1] == '=')
                                tokens.Add(new Token { TokenType = TokenType.Operation, Value = '≥', StartPosition = i++, Length = 2 });
                            else
                                tokens.Add(new Token { TokenType = TokenType.Operation, Value = '>', StartPosition = i, Length = 1 });
                            isFormulaSubPart = false;
                            break;
                        case '!':
                            if (i + 1 < characters.Length && characters[i + 1] == '=')
                            {
                                tokens.Add(new Token { TokenType = TokenType.Operation, Value = '≠', StartPosition = i++, Length = 2 });
                                isFormulaSubPart = false;
                            }
                            else
                                throw new InvalidTokenParseException($"Invalid token \"{characters[i]}\" detected at position {i}.", expression, i, characters[i].ToString());
                            break;
                        case '&':
                            if (i + 1 < characters.Length && characters[i + 1] == '&')
                            {
                                tokens.Add(new Token { TokenType = TokenType.Operation, Value = '&', StartPosition = i++, Length = 2 });
                                isFormulaSubPart = false;
                            }
                            else
                                throw new InvalidTokenParseException($"Invalid token \"{characters[i]}\" detected at position {i}.", expression, i, characters[i].ToString());
                            break;
                        case '|':
                            if (i + 1 < characters.Length && characters[i + 1] == '|')
                            {
                                tokens.Add(new Token { TokenType = TokenType.Operation, Value = '|', StartPosition = i++, Length = 2 });
                                isFormulaSubPart = false;
                            }
                            else
                                throw new InvalidTokenParseException($"Invalid token \"{characters[i]}\" detected at position {i}.", expression, i, characters[i].ToString());
                            break;
                        case '=':
                            if (i + 1 < characters.Length && characters[i + 1] == '=')
                            {
                                tokens.Add(new Token { TokenType = TokenType.Operation, Value = '=', StartPosition = i++, Length = 2 });
                                isFormulaSubPart = false;
                            }
                            else
                                throw new InvalidTokenParseException($"Invalid token \"{characters[i]}\" detected at position {i}.", expression, i, characters[i].ToString());
                            break;
                        default:
                            throw new InvalidTokenParseException($"Invalid token \"{characters[i]}\" detected at position {i}.", expression, i, characters[i].ToString());
                    }
                }
            }

            return tokens;
        }

        private bool IsPartOfNumeric(char character, bool isFirstCharacter, bool afterMinus, bool isFormulaSubPart)
        {
            return character == decimalSeparator || (character >= '0' && character <= '9') || (isFormulaSubPart && isFirstCharacter && character == '-') || (!isFirstCharacter && !afterMinus && character == 'e') || (!isFirstCharacter && character == 'E');
        }

        private bool IsPartOfTextToken(char character, bool isFirstCharacter)
        {
            return (character >= 'a' && character <= 'z') || (character >= 'A' && character <= 'Z') || (!isFirstCharacter && character >= '0' && character <= '9') || (!isFirstCharacter && character == '_');
        }

        private bool IsUnaryMinus(char currentToken, IList<Token> tokens)
        {
            if (currentToken != '-') return false;
            var previousToken = tokens[tokens.Count - 1];

            // can't be function since function is _always_ directly followed by a left parenthesis
            return !(previousToken.TokenType == TokenType.FloatingPoint ||
                     previousToken.TokenType == TokenType.Integer ||
                     previousToken.TokenType == TokenType.Symbol ||
                     previousToken.TokenType == TokenType.RightParenthesis);

        }

        private bool IsScientificNotation(char currentToken)
        {
            return currentToken == 'e' || currentToken == 'E';
        }
    }
}
