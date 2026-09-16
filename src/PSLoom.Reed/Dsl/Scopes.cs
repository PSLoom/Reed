// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Warp.Dsl;

namespace PSLoom.Reed.Dsl;

/// <summary>
///   Inside a completer's body: the subcommands, options, arguments and option groups of the command itself.
/// </summary>
public abstract class CompleterScope : DslScope;

/// <summary>
///   Inside a subcommand's body: the same vocabulary, one level down.
/// </summary>
public abstract class CommandScope : DslScope;

/// <summary>
///   Inside an option's body: the value that option takes.
/// </summary>
public abstract class OptionScope : DslScope;

/// <summary>
///   Inside an option group's body: the options the group collects for later reuse.
/// </summary>
public abstract class OptionGroupScope : DslScope;
