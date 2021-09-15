// <copyright file="CommandLineExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Extensions.System.CommandLine
{
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Hosting;
    using global::Microsoft.Extensions.Logging;
    using global::System.CommandLine;
    using global::System.CommandLine.Binding;
    using global::System.CommandLine.Invocation;
    using MetadataUtility.Extensions.Microsoft.Extensions;

    public static class CommandLineExtensions
    {

        // adapted from: https://github.com/dotnet/command-line-api/blob/43be76901630aae866657dd5ec1978a5d48d5b09/src/System.CommandLine.Hosting/HostingExtensions.cs#L85
        public static IHostBuilder UseEmuCommand<TCommand, THandler>(this IHostBuilder builder)
            where TCommand : Command
            where THandler : EmuCommandHandler
        {
            var commandType = typeof(TCommand);
            var handlerType = typeof(THandler);
            if (!typeof(Command).IsAssignableFrom(commandType))
            {
                throw new ArgumentException($"{nameof(commandType)} must be a type of {nameof(Command)}", nameof(handlerType));
            }

            if (!typeof(ICommandHandler).IsAssignableFrom(handlerType))
            {
                throw new ArgumentException($"{nameof(handlerType)} must implement {nameof(ICommandHandler)}", nameof(handlerType));
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
                    using var _ = logger.Measure(command.Name, LogLevel.Debug);
                    logger.LogDebug("Handler: {@args}", handler);

                    var result = await handler.InvokeAsync(context);

                    // flush output footer
                    handler?.Writer?.Dispose();

                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        // Hack: flush a new line so that any stdout that was just written is delmitted a little bit
                        Console.Error.WriteLine();
                    }

                    return result;
                });

                builder.ConfigureServices((collection) =>
                {
                    // the "current command" an alias for the base type
                    collection.AddTransient<Command, TCommand>();

                    // the handler
                    collection.AddTransient<THandler>();

                    // but also add a alias for the base type
                    collection.AddTransient<EmuCommandHandler, THandler>();
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
    }
}
