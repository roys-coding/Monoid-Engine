using Hexa.NET.ImGui;
using Microsoft.Xna.Framework.Input;
using MyMonoGameApp.DearImGui;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml.Schema;

namespace MyMonoGameApp;

public static class GameConsole
{
    #region STRUCTS
    /// <summary>
    /// A console command.
    /// </summary>
    public readonly struct Command
    {
        /// <summary>
        /// Max characters allowed in a command's keyword.
        /// </summary>
        public const int MAX_KEYWORD_LENGTH = 24;

        /// <summary>
        /// Function called when a command is executed.
        /// </summary>
        /// <param name="parameters">Parameters passed to the command.</param>
        /// <returns></returns>
        public delegate bool ExecutionCallback(Dictionary<string, ParsedParameter> parameters);

        /// <summary>
        /// Case-insensitive keyword used to execute this command through the console. Can only contain letters, keywords and underscores, and must not be longer than MAX_KEYWORD_LENGTH characters.
        /// </summary>
        public string Keyword { get; }
        /// <summary>
        /// A brief description of what this command does.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// A list of parameters that may be passed to this command when executed.
        /// </summary>
        public CommandParameter[] Parameters { get; }
        /// <summary>
        /// Function called when a command is executed.
        /// </summary>
        public ExecutionCallback Callback { get; }

        /// <summary>
        /// Instantiates a console command.
        /// </summary>
        /// <param name="keyword">Case-insensitive keyword used to execute this command through the console. Can only contain letters, keywords and underscores, and must not be longer than <see cref="MAX_KEYWORD_LENGTH"/> characters.</param>
        /// <param name="description">A brief description of what this command does.</param>
        /// <param name="callback">Function called when a command is executed.</param>
        /// <param name="parameters">A list of parameters that may be passed to this command when executed.</param>
        /// <exception cref="ArgumentNullException">Thrown if any argument is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="keyword"/> contains invalid characters, or is longer than <see cref="MAX_KEYWORD_LENGTH"/> characters.</exception>
        public Command(string keyword, string description, ExecutionCallback callback, params CommandParameter[] parameters)
        {
            Keyword = keyword ?? throw new ArgumentNullException(nameof(keyword));

            // Ensure that keyword only contains letters, digits and underscores.
            bool keywordContainsInvalidCharacters = !keyword.All(c=>char.IsLetterOrDigit(c) || c == '_');

            if (keywordContainsInvalidCharacters)
            {
                throw new ArgumentException($"Invalid command keyword '{keyword}'. A command keyword can only contain letters, keywords and underscores.");
            }

            if (keyword.Length > MAX_KEYWORD_LENGTH)
            {
                throw new ArgumentException($"Invalid command keyword '{keyword}'. A command keyword cannot be longer than {MAX_KEYWORD_LENGTH} characters.");
            }

            Description = description ?? throw new ArgumentNullException(nameof(description));
            Callback = callback ?? throw new ArgumentNullException(nameof(callback));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        /// <summary>
        /// Executes this command.
        /// </summary>
        /// <param name="parameters">List of input parameters as strings.</param>
        /// <returns>
        /// <c>true</c> if the execution was successful;
        /// <c>false</c> otherwise.
        /// </returns>
        public bool Execute(string[] parameters)
        {
            if (Callback == null)
            {
                Error("Execution callback function not found");
                return false;
            }

            Dictionary<string, ParsedParameter> parsedParameters = new();

            for (int i = 0; i < Parameters.Length; i++)
            {
                CommandParameter parameter = Parameters[i];
                ParsedParameter parsedParameter;

                // Parameter was not specified in the command execution.
                if (i >= parameters.Length)
                {
                    // If parameter is obligatory, exit.
                    if (parameter.IsObligatory)
                    {
                        Error($"Missing parameter: {parameter}");
                        return false;
                    }
                    // Pass the default value otherwise.
                    else
                    {
                        parsedParameter = new(parameter.Name, true, parameter.DefaultValue);
                    }
                }
                // Parse parameter from input string.
                else
                {
                    object boxedParameter;
                    string parameterString = parameters[i];
                    bool wasParseSuccessful = parameter.TryParseBoxed(parameterString, out boxedParameter);

                    parsedParameter = new(parameter.Name, false, boxedParameter);

                    // Parse was unsuccessful, exit.
                    if (!wasParseSuccessful)
                    {
                        Error($"Could not parse parameter: {parameter}.");
                        return false;
                    }
                }

                parsedParameters.Add(parameter.Name, parsedParameter);
            }

            return Callback(parsedParameters);
        }
    }

    /// <summary>
    /// A command's parameter.
    /// </summary>
    public readonly struct CommandParameter
    {
        private static readonly Dictionary<ParameterType, Type> _parameterTypeMap = new()
        {
            {ParameterType.Boolean, typeof(bool)},
            {ParameterType.String, typeof(string)},
            {ParameterType.Int, typeof(int)},
            {ParameterType.Float, typeof(float)},
            {ParameterType.Double, typeof(double)},
            {ParameterType.Decimal, typeof(decimal)},
            {ParameterType.Byte, typeof(byte)},
            {ParameterType.Enum, typeof(Enum)},
        };

        /// <summary>
        /// The type of this parameter.
        /// </summary>
        public ParameterType Type { get; }
        /// <summary>
        /// Custom type provided to this parameter.
        /// </summary>
        /// <remarks>Used to parse the parameter from a string to an object of this type.<br/>
        /// It's usage depending on <see cref="ParameterType"/>:
        /// <list type="bullet">
        /// <item><see cref="ParameterType.Enum"/>: Must equal to the type of the enum.</item>
        /// <item><see cref="ParameterType.CustomType"/>: Must equal to the custom type.</item>
        /// <item>Any other: ignored.</item>
        /// </list>
        /// </remarks>
        public Type CustomType { get; }
        /// <summary>
        /// Name of this parameter.
        /// </summary>
        /// <remarks>Used for descriptive purposes only.</remarks>
        public string Name { get; }
        /// <summary>
        /// A brief description of what this parameter is used for.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// Whether this parameter must be passed for the command to execute.
        /// </summary>
        public bool IsObligatory { get; }
        /// <summary>
        /// Value passed when the user does not provide a value.
        /// </summary>
        public object DefaultValue { get; }

        /// <summary>
        /// Instantiates a command parameter.
        /// </summary>
        /// <param name="type">The type of this parameter.</param>
        /// <param name="name">Name of this parameter.</param>
        /// <param name="description">A brief description of what this parameter is used for.</param>
        /// <param name="isObligatory">Whether this parameter must be passed for the command to execute.</param>
        /// <param name="defaultValue">Value passed when the user does not provide a value.</param>
        /// <param name="customType">Custom type provided to this parameter.<br/><br/>
        /// Must be an enum type for parameters of type <see cref="ParameterType.Enum"/>,<br/>
        /// or the desired parameter type for parameters of type <see cref="ParameterType.CustomType"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="description"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <see cref="ParameterType.CustomType"/> or <see cref="ParameterType.Enum"/>, and <paramref name="customType"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="type"/> is <see cref="ParameterType.Enum"/>, and <paramref name="customType"/> is not a enum type.</exception>
        public CommandParameter(ParameterType type, string name, string description, bool isObligatory = true, object defaultValue = null, Type customType = null)
        {
            if (type == ParameterType.Enum)
            {
                if (customType == null)
                {
                    throw new ArgumentNullException($"{nameof(customType)} must not be null for parameters of type '{type}'.");
                }
                else if (!customType.IsEnum)
                {
                    throw new ArgumentException($"{nameof(customType)} must be an enum type for parameters of type '{ParameterType.Enum}'.");
                }
            }

            if (type == ParameterType.CustomType && customType == null)
            {
                throw new ArgumentNullException($"{nameof(customType)} must not be null for parameters of type '{type}'.");
            }

            Type = type;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsObligatory = isObligatory;
            DefaultValue = defaultValue;
            CustomType = customType;
        }

        public static CommandParameter CreateBoolean(string name, string description, bool isObligatory = false, bool defaultValue = false)
        {
            return new(ParameterType.Boolean, name, description, isObligatory, defaultValue);
        }
        public static CommandParameter CreateString(string name, string description, bool isObligatory = false, string defaultValue = "")
        {
            return new(ParameterType.String, name, description, isObligatory, defaultValue);
        }
        public static CommandParameter CreateInt(string name, string description, bool isObligatory = false, int defaultValue = 0)
        {
            return new(ParameterType.String, name, description, isObligatory, defaultValue);
        }
        public static CommandParameter CreateFloat(string name, string description, bool isObligatory = false, float defaultValue = 0f)
        {
            return new(ParameterType.String, name, description, isObligatory, defaultValue);
        }
        public static CommandParameter CreateDouble(string name, string description, bool isObligatory = false, double defaultValue = 0.0)
        {
            return new(ParameterType.String, name, description, isObligatory, defaultValue);
        }
        public static CommandParameter CreateDecimal(string name, string description, bool isObligatory = false, decimal defaultValue = 0m)
        {
            return new(ParameterType.String, name, description, isObligatory, defaultValue);
        }
        public static CommandParameter CreateByte(string name, string description, bool isObligatory = false, byte defaultValue = 0)
        {
            return new(ParameterType.String, name, description, isObligatory, defaultValue);
        }
        public static CommandParameter CreateEnum(string name, string description, int defaultValue, Type enumType, bool isObligatory = false)
        {
            return new(ParameterType.String, name, description, isObligatory, defaultValue, enumType);
        }
        public static CommandParameter CreateCustomType(string name, string description, int defaultValue, Type customType, bool isObligatory = false)
        {
            return new(ParameterType.CustomType, name, description, isObligatory, defaultValue, customType);
        }
        public static CommandParameter CreateCustomType<T>(string name, string description, int defaultValue, bool isObligatory = false)
        {
            return new(ParameterType.CustomType, name, description, isObligatory, defaultValue, typeof(T));
        }

        /// <summary>
        /// Converts the string representation of the parameter to the corresponding type, depending on the parameter's <see cref="ParameterType"/>.
        /// </summary>
        /// <param name="input">Input string, which will be parsed.</param>
        /// <param name="result">When this method returns, this will equal to the parsed object.</param>
        /// <returns>
        /// <c>true</c> if the parse was successful;<br/>
        /// <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown if the parameter's type is <see cref="ParameterType.Enum"/>, and <see cref="CustomType"/> is not an enum type.</exception>
        /// <exception cref="NotImplementedException">Thrown if the parameter's type is not supported.</exception>
        public bool TryParseBoxed(string input, out object result)
        {
            // Custom parsing for enums.
            if (Type == ParameterType.Enum)
            {
                if (!CustomType.IsEnum)
                {
                    throw new InvalidOperationException($"Custom type must be an enum type for parameters of type '{Type}'.");
                }

                return Enum.TryParse(CustomType, input, out result);
            }

            // Use type parsers.
            Type parseType;

            if (Type == ParameterType.CustomType)
            {
                parseType = CustomType;
            }
            else
            {
                // Parameter type not supported.
                if (!_parameterTypeMap.ContainsKey(Type))
                {
                    throw new NotImplementedException($"Parameter type '{Type}' not supported.");
                }

                parseType = _parameterTypeMap[Type];
            }

            // Casting to type not supported.
            if (!_parameterParsers.ContainsKey(parseType))
            {
                throw new NotImplementedException($"Parameters of type '{parseType.Name}' not supported.");
            }

            return _parameterParsers[parseType](input, out result);
        }

        public override string ToString()
        {
            string prefix;
            string suffix;

            if (IsObligatory)
            {
                prefix = "[";
                suffix = "]";
            }
            else
            {
                prefix = "(";
                suffix = ")";
            }

            string type = Type switch
            {
                ParameterType.CustomType => CustomType.Name,
                ParameterType.Enum => $"(Enum) {CustomType.Name}",
                _ => Type.ToString(),
            };

            return $"{prefix}{Name}: {type}{suffix}";
        }
    }

    /// <summary>
    /// A command parameter that was parsed from a string.
    /// </summary>
    public readonly struct ParsedParameter
    {
        /// <summary>
        /// Name of the original parameter.
        /// </summary>
        public string ParameterName { get; }
        /// <summary>
        /// Whether a user-provided value was passed to the command execution, or the parameter's default value was passed instead.
        /// </summary>
        public bool IsDefault { get; }
        /// <summary>
        /// The parsed value of the parameter, boxed.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// A command parameter that was parsed from a string.
        /// </summary>
        /// <param name="parameterName">Name of the original parameter.</param>
        /// <param name="isDefault">Whether a user-provided value was passed to the command execution, or the parameter's default value was passed instead.</param>
        /// <param name="value">The parsed value of the parameter, boxed.</param>
        public ParsedParameter(string parameterName, bool isDefault, object value)
        {
            ParameterName = parameterName;
            IsDefault = isDefault;
            Value = value;
        }
    }

    /// <summary>
    /// Represents an entry in the console log.
    /// </summary>
    private readonly struct LogEntry
    {
        /// <summary>
        /// Message displayed.
        /// </summary>
        public string Message { get; }
        /// <summary>
        /// Color of the message displayed.
        /// </summary>
        /// <remarks>Set to null to use default text color.</remarks>
        public System.Numerics.Vector4? Color { get; }

        /// <summary>
        /// Instantiates a log entry.
        /// </summary>
        /// <param name="message">Message displayed.</param>
        /// <param name="color">Color of the message displayed. Set to null to use default text color.</param>
        public LogEntry(string message, System.Numerics.Vector4? color)
        {
            Message = message;
            Color = color;
        }
    }
    #endregion

    #region ENUMS
    /// <summary>
    /// Severity of a console log entry.
    /// </summary>
    public enum Severity
    {
        /// <summary>
        /// Information in regards the program's execution.
        /// </summary>
        Information,
        /// <summary>
        /// Information that is only useful to the user.
        /// </summary>
        Debug,
        /// <summary>
        /// Information in regards potential errors that might cause malfunctions or crashes.
        /// </summary>
        Warning,
        /// <summary>
        /// Information in regards errors that might cause malfunctions or crashes.
        /// </summary>
        Error,
        /// <summary>
        /// Information in regards errors that will cause malfunctions or crashes.
        /// </summary>
        Fatal
    }
    
    /// <summary>
    /// Valid parameter types.
    /// </summary>
    public enum ParameterType
    {
        Boolean,
        String,
        Int,
        Float,
        Double,
        Decimal,
        Byte,
        Enum,
        CustomType
    }

    private enum HintingState
    {
        NotHinting,
        AutoCompleteCommand,
        HintParameter
    }
    #endregion

    const int MAX_ENTRIES = 1000;
    const int MAX_INPUT_LENGTH = 256;

    public delegate bool ParameterParser(string input, out object result);

    // Console State.
    private static bool _isOpen = false;
    private static bool _isInitialized = false;

    // Options.
    private static bool _textWrapping = true;
    private static bool _autoScroll = true;
    private static bool _commandHistory = true;
    private static bool _hinting = true;

    // Commands.
    private static readonly Dictionary<Type, ParameterParser> _parameterParsers = new();
    private static readonly Dictionary<string, Command> _commands = new();

    // Logs and input.
    private static readonly List<LogEntry> _logHistory = new();
    private static string _inputCommand = "";

    // Command history.
    private static readonly List<ColoredText> _commandsHistory = new();
    private static int _commandHistoryPosition = -1;
    private static string _lastCommand = "";

    // Hinting.
    private static HintingState _hintingState = HintingState.NotHinting;
    private static List<string> _hints = new();
    private static Command? _hintCommand;
    private static int _hintSelection = 0;
    private static bool _updateHints = false;

    // ImGui
    private static bool _forceScrollToBottom = false;
    private static bool _forceInputTextFocus = false;
    private static bool _reloadInputText = false;
    private static bool _isInputTextFocused = false;

    static GameConsole()
    {
        // Create default parsers.
        _parameterParsers.Add(typeof(bool), (string input, out object result) =>
        {
            bool wasParseSuccessful = bool.TryParse(input, out bool boolResult);
            result = boolResult;
            return wasParseSuccessful;
        });
        _parameterParsers.Add(typeof(string), (string input, out object result) =>
        {
            result = input;
            return true;
        });
        _parameterParsers.Add(typeof(int), (string input, out object result) =>
        {
            bool wasParseSuccessful = int.TryParse(input, out int intResult);
            result = intResult;
            return wasParseSuccessful;
        });
        _parameterParsers.Add(typeof(float), (string input, out object result) =>
        {
            bool wasParseSuccessful = float.TryParse(input, out float floatResult);
            result = floatResult;
            return wasParseSuccessful;
        });
        _parameterParsers.Add(typeof(double), (string input, out object result) =>
        {
            bool wasParseSuccessful = double.TryParse(input, out double doubleResult);
            result = doubleResult;
            return wasParseSuccessful;
        });
        _parameterParsers.Add(typeof(decimal), (string input, out object result) =>
        {
            bool wasParseSuccessful = decimal.TryParse(input, out decimal decimalResult);
            result = decimalResult;
            return wasParseSuccessful;
        });
        _parameterParsers.Add(typeof(byte), (string input, out object result) =>
        {
            bool wasParseSuccessful = byte.TryParse(input, out byte byteResult);
            result = byteResult;
            return wasParseSuccessful;
        });

        CreateCommand("help", "Displays the list of commands.", (parameters) =>
        {
            ParsedParameter commandParameter = parameters["Command"];
            StringBuilder sb = new();

            if (commandParameter.IsDefault)
            {
                sb.AppendLine("List of commands:");

                foreach (KeyValuePair<string, Command> kvp in _commands)
                {
                    Command cmd = kvp.Value;

                    sb.AppendLine($"- {cmd.Keyword} {string.Join(" ", cmd.Parameters)}");
                }

                Debug("Use 'help (command)' to see more info. about a specific command");
            }
            else
            {
                string commandKeyword = commandParameter.Value as string;

                if (!_commands.ContainsKey(commandKeyword))
                {
                    Error($"Could not find the command '{commandKeyword}'.\n Use 'help' to see the list of commands.");
                    return false;
                }

                Command command = _commands[commandKeyword];

                sb.AppendLine($"Command: \"{command.Keyword}\"");
                sb.AppendLine($"Description: \"{command.Description}\"");

                if (command.Parameters.Length != 0)
                {
                    sb.AppendLine("Parameters:");

                    foreach (CommandParameter parameter in command.Parameters)
                    {
                        sb.AppendLine($"- {parameter}: \"{parameter.Description}\"");
                    }
                }
            }

            Info(sb.ToString());
            return true;
        }, CommandParameter.CreateString("Command", "If specified, more information about this command will be displayed."));

        CreateCommand("clear", "Clears the console.", parameters =>
        {
            ClearLogHistory();
            return true;
        });
    }

    public static void Initialize()
    {
        if (_isInitialized) return;

        CreateCommandsFromAttributes();

        // Events.
        //DImGui.OnDraw += RenderConsole;
        GameKeyboard.OnKeyPressed += OnKeyPressed;
        _isInitialized = true;
    }

    #region INPUT
    private static void OnKeyPressed(object sender, GameKeyboard.KeyboardEventArgs e)
    {
        if (e.Key == Keys.OemTilde)
        {
            _isOpen = !_isOpen;
            if (_isOpen) _forceInputTextFocus = true;
        }

        if (!_isOpen || !_isInputTextFocused) return;

        switch (e.Key)
        {
            case Keys.Enter:
                if (_hintingState == HintingState.AutoCompleteCommand)
                {
                    PerformHintAutoComplete();
                }
                else
                {
                    ResetHintState();
                }

                ExecuteInputCommand();
                _commandHistoryPosition = -1;
                _forceInputTextFocus = true;
                _reloadInputText = true;
                break;
            case Keys.Escape:
                ResetHintState();
                _commandHistoryPosition = -1;
                _forceInputTextFocus = true;
                break;
        }
    }
    #endregion

    #region COMMANDS
    /// <summary>
    /// Adds a command parameter parser.
    /// </summary>
    /// <remarks>Adding a parser for a custom type allows custom commands to specify parameters of that type.<br/>
    /// Otherwise, they will just throw an exception if the type is unsupported.
    /// </remarks>
    /// <param name="parameterType">The type of the objects returned by the parser.</param>
    /// <param name="parser">Method that converts from strings to objects of the specified <paramref name="parameterType"/>.</param>
    /// <exception cref="InvalidOperationException">Thrown if a parser for the provided <paramref name="parameterType"/> already exists.</exception>
    public static void AddCommandParameterParser(Type parameterType, ParameterParser parser)
    {
        // Parser already exists.
        if (_parameterParsers.ContainsKey(parameterType))
        {
            throw new InvalidOperationException($"A parser for parameters of type {parameterType.Name} already exists.");
        }

        _parameterParsers.Add(parameterType, parser);
    }

    /// <summary>
    /// Creates a command.
    /// </summary>
    /// <param name="keyword">Case-insensitive keyword used to execute this command through the console. Can only contain letters, keywords and underscores, and must not be longer than MAX_KEYWORD_LENGTH characters.</param>
    /// <param name="description">A brief description of what this command does.</param>
    /// <param name="callback">>Function called when a command is executed.</param>
    /// <param name="parameters">A list of parameters that may be passed to this command when executed.</param>
    /// <exception cref="InvalidOperationException">Thrown if a command under the same keyword already exists.</exception>
    /// <exception cref="ArgumentNullException">Thrown if any argument is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="keyword"/> contains invalid characters, or is longer than MAX_KEYWORD_LENGTH characters.</exception>
    public static void CreateCommand(string keyword, string description, Command.ExecutionCallback callback, params CommandParameter[] parameters)
    {
        if (_commands.ContainsKey(keyword))
        {
            throw new InvalidOperationException($"A command with the keyword '{keyword}' already exists. Use a different keyword.");
        }

        _commands.Add(keyword, new(keyword, description, callback, parameters));
    }

    private static void CreateCommandsFromAttributes()
    {
        const BindingFlags SEARCH_METHOD_FLAGS = BindingFlags.Public | BindingFlags.Static;

        // Iterate through all types in this assembly.
        foreach (TypeInfo typeInfo in Program.DefinedTypes)
        {
            // Iterate through each method.
            foreach (MethodInfo method in typeInfo.GetMethods(SEARCH_METHOD_FLAGS))
            {
                ConsoleCommandAttribute commandAttribute = method.GetCustomAttribute<ConsoleCommandAttribute>();

                // No console command attribute.
                if (commandAttribute == null) continue;

                // Create console command from attribute.
                commandAttribute.CreateCommand(method);
            }
        }
    }

    private static bool IsInputValid()
    {
        if (string.IsNullOrEmpty(_inputCommand)) return false;
        if (string.IsNullOrWhiteSpace(_inputCommand)) return false;

        return true;
    }

    private static void ExecuteInputCommand()
    {
        if (!IsInputValid()) return;

        // Add command to history, and log it.
        if (_inputCommand != _lastCommand) _commandsHistory.Insert(0, _inputCommand);
        Debug($"> {_inputCommand}");
        _lastCommand = _inputCommand;

        // Split command into command name, and arguments.
        string[] commandParts = _inputCommand.Split(' ');
        string[] arguments = commandParts.TakeLast(commandParts.Length - 1).ToArray();
        string commandKeyword = commandParts[0].ToLowerInvariant();

        // Clear text input and scroll to bottom.
        _inputCommand = "";

        // Command not found, exit.
        if (!_commands.ContainsKey(commandKeyword))
        {
            Error($"Command '{commandKeyword}' not found.\nUse 'help' to see the list of commands.");
            return;
        }

        try
        {
            // Execute command.
            Command command = _commands[commandKeyword];

            bool commandExecuted = command.Execute(arguments);
        }
        catch (Exception ex)
        {
            // Exception during execution.
            Error($"Internal exception:\n{ex.Message}");
        }
    }
    #endregion

    #region LOGGING
    public static void ClearLogHistory()
    {
        _logHistory.Clear();
    }

    public static void Info(string message) => AddText(message, Severity.Information);

    public static void Debug(string message) => AddText(message, Severity.Debug);

    public static void Warning(string message) => AddText(message, Severity.Warning);

    public static void Error(string message) => AddText(message, Severity.Error);

    public static void Fatal(string message) => AddText(message, Severity.Fatal);

    private static void AddText(string message, Severity severity)
    {
        System.Numerics.Vector4? color;

        // Get color depending on severity.
        switch (severity)
        {
            case Severity.Information:
                color = null;
                break;
            case Severity.Debug:
                color = new(0.5f, 0.5f, 0.5f, 1f);
                break;
            case Severity.Warning:
                color = new(1f, 1f, 0f, 1f);
                break;
            case Severity.Error:
                color = new(1f, 0f, 0f, 1f);
                break;
            case Severity.Fatal:
                color = new(0.6f, 0f, 0f, 1f);
                break;
            default:
                color = null;
                break;
        }

        _logHistory.Add(new(message, color));
        RemoveExtraEntries();

        // Auto scroll if enabled.
        if (_autoScroll) _forceScrollToBottom = true;
    }

    private static void RemoveExtraEntries()
    {
        while (_logHistory.Count > MAX_ENTRIES)
        {
            _logHistory.RemoveAt(0);
        }
    }
    #endregion

    #region HISTORY & HINTING
    private static void SetInputCommandToHistoryPosition()
    {
        _commandHistoryPosition = GameMath.Clamp(_commandHistoryPosition, -1, _commandsHistory.Count - 1);

        if (_commandHistoryPosition == -1)
        {
            _inputCommand = "";
        }
        else
        {
            _inputCommand = _commandsHistory[_commandHistoryPosition];
        }

        _reloadInputText = true;
    }

    private static void ResetHintState()
    {
        // Clear hints.
        _hintingState = HintingState.NotHinting;
        _hintSelection = 0;
        _updateHints = false;
        _hintCommand = null;
        _hints.Clear();
    }

    private static void UpdateHints()
    {
        _hints.Clear();
        _hintSelection = 0;

        // Split command into command and parameters.
        string[] inputParts = _inputCommand.Split(' ');
        string inputCommand = inputParts[0];
        int currentPart = inputParts.Length - 1;

        // Currently typing the first word, which is a command keyword...
        if (currentPart == 0)
        {
            // Search for commands that begin with the input text.
            foreach (var kvp in _commands)
            {
                string keyword = kvp.Key;
                Command command = kvp.Value;

                // Add hints for commands that start with input text.
                if (keyword.StartsWith(inputCommand))
                {
                    _hints.Add($"{keyword}|{command.Description}");
                }
            }

            // If no commands are found, disable hints.
            if (_hints.Count == 0) _hintingState = HintingState.NotHinting;
            // Set hinting mode to autocomplete otherwise.
            else _hintingState = HintingState.AutoCompleteCommand;
        }
        // Currently typing a parameter...
        else
        {
            // Command is valid.
            if (_commands.ContainsKey(inputCommand))
            {
                _hintCommand = _commands[inputCommand];
                int currentParameter = currentPart - 1;

                // Command does not have as many parameters.
                if (currentParameter >= _hintCommand.Value.Parameters.Length)
                {
                    _hintingState = HintingState.NotHinting;
                }
                // Or hint current parameter's name and description.
                else
                {
                    CommandParameter commandParameter = _hintCommand.Value.Parameters[currentParameter];

                    _hints.Add(commandParameter.ToString());
                    _hints.Add(commandParameter.Description);
                    _hintingState = HintingState.HintParameter;
                }
            }
            // Command is invalid.
            else
            {
                _hintCommand = null;
                _hintingState = HintingState.NotHinting;
            }
        }
    }

    private static void PerformHintAutoComplete()
    {
        if (_hints.Count == 0) return;

        // Set input command to selected hint.
        // Essentially performs autocompletion.
        _inputCommand = _hints[_hintSelection].Split('|')[0];

        ResetHintState();
        _reloadInputText = true;
    }
    #endregion

    #region IMGUI
    private static void RenderConsole(object sender, EventArgs e)
    {
        const string CONSOLE_WINDOW_NAME = $"{Fonts.Lucide.Terminal} Console";
        const string INPUT_TEXT_LABEL = "##command_input";
        const string INPUT_TEXT_HINT = "command";
        const string PREFIX = $"/";

        const float MIN_SIZE_X = 300;
        const float MIN_SIZE_Y = 200;

        const ImGuiWindowFlags WINDOW_FLAGS = ImGuiWindowFlags.None;
        const ImGuiWindowFlags CHILD_WINDOW_FLAGS = ImGuiWindowFlags.NoNav;
        const ImGuiChildFlags CHILD_FLAGS = ImGuiChildFlags.Borders;
        const ImGuiInputTextFlags INPUT_TEXT_FLAGS = ImGuiInputTextFlags.CallbackHistory
                                                     | ImGuiInputTextFlags.CallbackEdit
                                                     | ImGuiInputTextFlags.CallbackCompletion
                                                     | ImGuiInputTextFlags.EscapeClearsAll;

        // Exit if window is not open.
        if (!_isOpen) return;

        // Set minimum size.
        ImGui.SetNextWindowSizeConstraints(new(MIN_SIZE_X, MIN_SIZE_Y), new(float.MaxValue, float.MaxValue));

        // Create window.
        ImGui.Begin(CONSOLE_WINDOW_NAME, ref _isOpen, WINDOW_FLAGS);

        ImGuiStylePtr style = ImGui.GetStyle();

        // Calculate logs window size and input text size.
        System.Numerics.Vector2 totalSpace = ImGui.GetContentRegionAvail();
        float inputTextHeight = ImGui.GetFrameHeight();
        float itemSpacingY = style.ItemSpacing.Y;

        System.Numerics.Vector2 childSize = new()
        {
            X = totalSpace.X,
            Y = MathF.Max(1f, totalSpace.Y - inputTextHeight - itemSpacingY),
        };

        // Add context menu to main window.
        RenderContextMenu("console_context");

        // Force focus to text input if window was clicked.
        // Makes it easier to focus the text input, since clicking
        // the console has no purpose anyways.
        if (ImGui.IsItemActive())
        {
            _forceInputTextFocus = true;
        }

        // History child window.
        ImGui.BeginChild("command_history", childSize, CHILD_FLAGS, CHILD_WINDOW_FLAGS);

        // Add context menu to child window.
        RenderContextMenu("console_context");

        // Force focus to text input if child window was clicked.
        // Makes it easier to focus the text input, since clicking
        // the console logs has no purpose anyways.
        if (ImGui.IsItemActive())
        {
            _forceInputTextFocus = true;
        }

        if (_textWrapping) ImGui.PushTextWrapPos();

        // Draw logs history.
        foreach (LogEntry item in _logHistory)
        {
            if (item.Color != null)
            {
                ImGui.TextColored(item.Color.Value, item.Message);
            }
            else
            {
                ImGui.Text(item.Message);
            }
        }

        // Scroll to bottom when requested.
        if (_forceScrollToBottom)
        {
            ImGui.SetScrollHereY(1f);
            _forceScrollToBottom = false;
        }

        if (_textWrapping) ImGui.PopTextWrapPos();

        ImGui.EndChild();

        // Make input text as wide as the window.
        uint inputTextID = ImGui.GetID(INPUT_TEXT_LABEL);
        float inputTextWidth = ImGui.GetContentRegionAvail().X;
        ImGui.SetNextItemWidth(inputTextWidth);

        ImGuiInputTextStatePtr inputTextState = ImGuiP.GetInputTextState(inputTextID);

        // Reload input text buffer when requested.
        if (_reloadInputText)
        {
            inputTextState.ReloadUserBuf = true;
            inputTextState.ReloadSelectionStart = _inputCommand.Length;
            inputTextState.ReloadSelectionEnd = _inputCommand.Length;

            _reloadInputText = false;
        }
        // Force focus to input text only if the buffer has not reloaded.
        // Focusing and reloading the buffer on the same frame
        // does not work.
        else if (_forceInputTextFocus)
        {
            ImGui.SetKeyboardFocusHere();
            _forceInputTextFocus = false;
        }

        // Input text.
        unsafe
        {
            ImGui.InputTextWithHint(INPUT_TEXT_LABEL, INPUT_TEXT_HINT, ref _inputCommand, MAX_INPUT_LENGTH, INPUT_TEXT_FLAGS, InputTextCallback);
        }

        // Draw slash decoration.
        uint decorationsColor = ImGui.GetColorU32(ImGuiCol.TextDisabled);
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        System.Numerics.Vector2 prefixTextPos = ImGui.GetItemRectMin();
        System.Numerics.Vector2 prefixTextSize = ImGui.CalcTextSize(PREFIX);
        prefixTextPos.Y += ImGui.GetItemRectSize().Y * 0.5f;
        prefixTextPos.X -= prefixTextSize.X;
        prefixTextPos.Y -= prefixTextSize.Y * 0.5f;

        drawList.AddText(prefixTextPos, decorationsColor, PREFIX);

        // Draw control icons decorations.
        StringBuilder controlsSb = new();

        if (_commandHistoryPosition > -1)
        {
            controlsSb.Append($"{_commandHistoryPosition + 1} {Fonts.Lucide.History}");
        }

        if (_hinting)
        {
            controlsSb.Append(Fonts.Lucide.ArrowLeftRight);
        }

        string controls = controlsSb.ToString();
        System.Numerics.Vector2 controlsTextPos = ImGui.GetItemRectMax();
        System.Numerics.Vector2 controlsTextSize = ImGui.CalcTextSize(controls);
        controlsTextPos.Y -= ImGui.GetItemRectSize().Y * 0.5f;
        controlsTextPos.X -= controlsTextSize.X;
        controlsTextPos.Y -= controlsTextSize.Y * 0.5f;
        controlsTextPos.X -= style.FramePadding.X;

        drawList.AddText(controlsTextPos, decorationsColor, controls);

        // Store whether the input text is focused or not.
        // Used in input methods outside imgui.
        _isInputTextFocused = ImGui.IsItemFocused();

        // Hinting.
        if (_hinting)
        {
            // Update hints when requested.
            // Updated one frame after request to allow for the text input
            // to apply changes to the string.
            if (_updateHints)
            {
                UpdateHints();
                _updateHints = false;
            }

            if (_hintingState != HintingState.NotHinting)
            {
                System.Numerics.Vector2 autoCompletionPosition = ImGui.GetItemRectMin();

                // Get number of spaces in input command (usually equal to number of words).
                int lastSpaceIndex = _inputCommand.LastIndexOf(' ');

                // Set hinting window position to the beginning of the last word in the input command.
                if (lastSpaceIndex > -1)
                {
                    string inputCommandMinusLastWord = _inputCommand.Substring(0, lastSpaceIndex);
                    autoCompletionPosition.X += ImGui.CalcTextSize(inputCommandMinusLastWord).X;
                }

                ImGui.SetNextWindowPos(autoCompletionPosition, ImGuiCond.Always, new(0f, 1f));

                // Render hinting.
                RenderHints();
            }
        }

        ImGui.End();
    }

    private static void RenderContextMenu(string ID)
    {
        bool contextOpen = ImGui.BeginPopupContextWindow(ID);

        // Exit if context menu is not open.
        if (!contextOpen) return;

        // Title.
        ImGui.TextDisabled("Console");
        ImGui.Spacing();

        // Options menu.
        bool isOptionsMenuOpen = ImGui.BeginMenu("Options##console");

        if (isOptionsMenuOpen)
        {
            bool hintingPressed = ImGui.MenuItem("Hints", "", ref _hinting);
            if (hintingPressed) ResetHintState();

            ImGui.MenuItem("Command history", "", ref _commandHistory);
            ImGui.MenuItem("Auto scroll", "", ref _autoScroll);
            ImGui.MenuItem("Text wrap", "", ref _textWrapping);

            ImGui.EndMenu();
        }

        // Clear menu.
        bool isClearMenuOpen = ImGui.BeginMenu("Clear");

        if (isClearMenuOpen)
        {
            bool clearLogsPressed = ImGui.MenuItem("Log history");
            if (clearLogsPressed)
            {
                ClearLogHistory();
                _forceInputTextFocus = true;
            }

            bool clearCommandsPressed = ImGui.MenuItem("Command history");
            if (clearCommandsPressed)
            {
                if (_commandHistoryPosition > -1)
                {
                    _inputCommand = "";
                    _reloadInputText = true;
                }

                _commandsHistory.Clear();
                _lastCommand = "";
                _forceInputTextFocus = true;
                _commandHistoryPosition = -1;
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();
        bool closePressed = ImGui.MenuItem("Close");
        if (closePressed) _isOpen = false;

        ImGui.EndPopup();
    }

    private static void RenderHints()
    {
        const float MAX_WINDOW_WIDTH = 400f;
        const float MAX_WINDOW_HEIGHT = 200f;

        // Disable window rounding, for a more appealing look.
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.SetNextWindowSizeConstraints(new(0f, 0f), new(MAX_WINDOW_WIDTH, MAX_WINDOW_HEIGHT));

        bool popupOpen = ImGui.BeginTooltip();

        // Exit if the hints popup is not open.
        if (!popupOpen) return;

        // Enable text wrapping.
        ImGui.PushTextWrapPos(MAX_WINDOW_WIDTH);

        switch (_hintingState)
        {
            // Auto completing commands.
            case HintingState.AutoCompleteCommand:
                bool tableOpen = ImGui.BeginTable("commands", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingFixedFit);

                // Exit if the commands table is not open.
                if (!tableOpen) break;

                // Setup columns.
                ImGui.TableSetupColumn("keyword");
                ImGui.TableSetupColumn("description", ImGuiTableColumnFlags.WidthStretch);

                for (int i = 0; i < _hints.Count; i++)
                {
                    string completionOption = _hints[i];
                    bool optionSelected = i == _hintSelection;

                    string[] completionParts = completionOption.Split('|');
                    string commandKeyword = completionParts[0];

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.MenuItem(commandKeyword, false, optionSelected);

                    if (!optionSelected)
                    {
                        continue;
                    }

                    string commandDescription = completionParts[1];

                    ImGui.TableSetColumnIndex(1);
                    ImGui.Spacing(); ImGui.SameLine();
                    ImGui.TextDisabled(commandDescription);

                    var itemMin = ImGui.GetItemRectMin();
                    var itemMax = ImGui.GetItemRectMax();

                    float scrollMin = ImGui.GetWindowPos().Y;
                    float scrollMax = scrollMin + MAX_WINDOW_HEIGHT;

                    if (itemMin.Y < scrollMin)
                    {
                        ImGui.SetScrollHereY(0f);
                    }
                    if (itemMax.Y > scrollMax)
                    {
                        ImGui.SetScrollHereY(1f);
                    }
                }

                ImGui.EndTable();
                break;
            case HintingState.HintParameter:
                foreach (string hint in _hints)
                {
                    ImGui.TextDisabled(hint);
                }
                break;
        }

        ImGui.PopTextWrapPos();
        ImGui.EndTooltip();
        ImGui.PopStyleVar();
    }

    private static unsafe int InputTextCallback(ImGuiInputTextCallbackData* dataPtr)
    {
        ImGuiInputTextCallbackData data = *dataPtr;

        if (data.EventFlag.HasFlag(ImGuiInputTextFlags.CallbackHistory))
        {
            if (_hinting && _hintingState == HintingState.AutoCompleteCommand && _hints.Count > 0)
            {
                // Navigate hints.
                switch (data.EventKey)
                {
                    case ImGuiKey.DownArrow:
                        _hintSelection++;
                        break;
                    case ImGuiKey.UpArrow:
                        _hintSelection--;
                        break;
                }

                if (_hintSelection == -1)
                {
                    _hintSelection = _hints.Count - 1;
                }

                _hintSelection %= _hints.Count;
            }
            else if (_commandHistory && _commandsHistory.Count > 0)
            {
                // Navigate commands history.
                switch (data.EventKey)
                {
                    case ImGuiKey.UpArrow:
                        _commandHistoryPosition++;
                        break;
                    case ImGuiKey.DownArrow:
                        _commandHistoryPosition--;
                        break;
                }

                ResetHintState();
                SetInputCommandToHistoryPosition();
            }
        }

        if (_hinting)
        {
            // When the input text is edited...
            if (data.EventFlag.HasFlag(ImGuiInputTextFlags.CallbackEdit))
            {
                // Clear hints if input text is empty.
                if (data.BufTextLen == 0)
                {
                    ResetHintState();
                }
                // Update hints when input text is modified.
                else
                {
                    _updateHints = true;
                }
            }

            // When tab is pressed...
            if (data.EventFlag.HasFlag(ImGuiInputTextFlags.CallbackCompletion))
            {
                // If currently autocompleting, perform autocompletion.
                if (_hintingState == HintingState.AutoCompleteCommand)
                {
                    PerformHintAutoComplete();
                }
                // Display hints otherwise.
                else
                {
                    _updateHints = true;
                }
            }
        }

        return 0;
    }
    #endregion
}
