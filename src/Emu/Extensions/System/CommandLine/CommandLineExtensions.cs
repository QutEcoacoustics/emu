// <copyright file="CommandLineExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Extensions.System.CommandLine
{
    using Emu.Extensions.Microsoft.Extensions;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Hosting;
    using global::Microsoft.Extensions.Logging;
    using global::System.CommandLine;
    using global::System.CommandLine.Binding;
    using global::System.CommandLine.Invocation;
    using global::System.CommandLine.Parsing;

    /// <summary>
    /// Extensions to System.CommandLine.
    /// </summary>
    public static class CommandLineExtensions
    {
        /// <summary>
        /// Adapts System.CommandLine to our command class and hosting conventions.
        /// </summary>
        /// <param name="builder">The command builder instance.</param>
        /// <typeparam name="TCommand">The command to bind to.</typeparam>
        /// <typeparam name="THandler">The handler to invoke.</typeparam>
        /// <typeparam name="TResult">The type of the results output by the command.</typeparam>
        /// <returns>The same command builder instance.</returns>
        public static IHostBuilder UseEmuCommand<TCommand, THandler, TResult>(this IHostBuilder builder)
            where TCommand : Command
            where THandler : EmuCommandHandler<TResult>
        {
            // adapted from: https://github.com/dotnet/command-line-api/blob/43be76901630aae866657dd5ec1978a5d48d5b09/src/System.CommandLine.Hosting/HostingExtensions.cs#L85

            var commandType = typeof(TCommand);
            var handlerType = typeof(THandler);
            if (!typeof(Command).IsAssignableFrom(commandType))
            {
                throw new ArgumentException($"{nameof(commandType)} must be a type of {nameof(Command)}", nameof(THandler));
            }

            if (!typeof(ICommandHandler).IsAssignableFrom(handlerType))
            {
                throw new ArgumentException($"{nameof(handlerType)} must implement {nameof(ICommandHandler)}", nameof(THandler));
            }

            // only register services for the current command!
            if (builder.Properties[typeof(InvocationContext)] is InvocationContext invocation
                && invocation.ParseResult.CommandResult.Command is Command command
                && command.GetType() == commandType)
            {
                invocation.BindingContext.AddService<THandler>(
                    c => c.GetRequiredService<IHost>().Services.GetRequiredService<THandler>());

                //command.Handler = CommandHandler.Create(handlerType.GetMethod(nameof(ICommandHandler.InvokeAsync)));

                command.Handler = CommandHandler.Create<THandler, IHost, InvocationContext>(async (handler, host, context) =>
                {
                    var modelBinder = new ModelBinder<THandler>();
                    modelBinder.UpdateInstance(handler, invocation.BindingContext);

                    var logger = host.Services.GetRequiredService<ILogger<THandler>>();

                    using (logger.Measure(command.Name, LogLevel.Debug))
                    {
                        logger.LogDebug("Handler: {@args}", handler);

                        int result = 1;
                        try
                        {
                            result = await handler.InvokeAsync(context);
                        }
                        finally
                        {
                            // flush output footer
                            handler?.Writer?.Dispose();
                        }

                        if (logger.IsEnabled(LogLevel.Information))
                        {
                            // Hack: flush a new line so that any stdout that was just written is delmitted a little bit
                            Console.Error.WriteLine();
                        }

                        return result;
                    }
                });

                builder.ConfigureServices((collection) =>
                {
                    // the "current command" an alias for the base type
                    collection.AddTransient<Command, TCommand>();

                    // the handler
                    collection.AddTransient<THandler>();

                    // but also add a alias for the base type
                    collection.AddTransient<EmuCommandHandler<TResult>, THandler>();
                });
            }

            return builder;
        }

        public static void BindOptions<TOptions>(this IServiceCollection collection)
          where TOptions : class, new()
        {
            collection.AddSingleton<ModelBinder<TOptions>>();
            collection.AddTransient<TOptions>((provider) =>
            {
                var context = provider.GetRequiredService<BindingContext>();
                var modelBinder = provider.GetRequiredService<ModelBinder<TOptions>>();
                var options = new TOptions();
                modelBinder.UpdateInstance(options, context);
                return options;
            });
        }

        /// <summary>
        /// Add an alias to an option.
        /// </summary>
        /// <param name="option">The option to mutate.</param>
        /// <param name="alias">The alias to add.</param>
        /// <typeparam name="T">The option's value type.</typeparam>
        /// <returns>The mutated option.</returns>
        public static Option<T> WithAlias<T>(this Option<T> option, string alias)
        {
            option.AddAlias(alias);
            return option;
        }

        public static Option<T> WithValidator<T>(this Option<T> option, ValidateSymbol<OptionResult> validator)
        {
            option.AddValidator(validator);
            return option;
        }

        public static ParseArgument<TValue[]> SplitOnComma<TValue>() =>
            (result) =>
            {
                var items = result.Tokens.Count == 1 ? result.Tokens.Single().Value.Split(',') : result.Tokens.Select(t => t.Value);

                return items.Select(item => (TValue)Convert.ChangeType(item, typeof(TValue))).ToArray();
            };
    }
}
