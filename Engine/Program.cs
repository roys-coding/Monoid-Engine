global using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MonoidEngine;

/// <summary>
/// Entry point class.
/// </summary>
public class Program
{
    /// <summary>
    /// All the defined types in this program's assembly.
    /// </summary>
    public static IEnumerable<TypeInfo> DefinedTypes { get; private set; }

    /// <summary>
    /// Program's entry point.
    /// </summary>
    /// <param name="args">Program's arguments.</param>
    public static void Main(string[] args)
    {
        DefinedTypes = Assembly.GetExecutingAssembly().DefinedTypes;

        Monoid.StartupParameters config = Monoid.StartupParameters.None;

        if (args.Contains("-fmodliveupdate"))
        {
            config |= Monoid.StartupParameters.FMODLiveUpdateEnabled;
        }

        Monoid game = new(config)
        {

        };

        game.Run();
    }
}