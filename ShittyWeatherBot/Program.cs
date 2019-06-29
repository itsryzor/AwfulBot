//AwfulBot Version 0.0001
//Treating this as a serious entry is a waste of time
//Literally all this bot does at the moment is echo text typed to it using !say
//But it is my first programming project
//The entire point was to learn how Async and the Discord API work
//As the name suggests, one day it will display weather information
//As soon as I figure out how to use the API and parse data properly :)

using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;

namespace ShittyWeatherBot
{
    class Program
    {
        public static void Main()
        {
            
            //jump straight to Async context
            new Program().MainAsync().GetAwaiter().GetResult();
            
        }

        //No idea what this does but MainAsync() doesn't work without it
        private DiscordSocketClient _client;
        
        //new instance of each
        private CommandHandler _handler;
        private CommandService _service;

        public async Task MainAsync()
        {
            //creating new instanced connection to discord
            _client = new DiscordSocketClient();

            //new instances of service and command handler
            _service = new CommandService();
            _handler = new CommandHandler(_client, _service);

            //install all commands
            await _handler.InstallCommandsAsync();

            //forward logs to Log()
            _client.Log += Log;

            //privately grab Discord token
            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("token")); 

            //connect to Discord
            await _client.StartAsync();

            //block return until program is closed
            await Task.Delay(-1); 

            
        }

        private Task Log(LogMessage msg)
        {
            //print log to console
            Console.WriteLine(msg.ToString());
            //ready for other tasks
            return Task.CompletedTask;
        }

        /*private async Task MessageReceived(SocketMessage msg)
        {
            if (msg.Content == "!ping")
            {
                //user types !ping, return Pong!
                await msg.Channel.SendMessageAsync("Pong!");
            }
        }*/ //message interception test, bad practice

        public class CommandHandler
        {
            private readonly DiscordSocketClient _client;
            private readonly CommandService _commands;
            
            public CommandHandler(DiscordSocketClient client, CommandService commands)
            {
                _commands = commands;
                _client = client;
            }

            public async Task InstallCommandsAsync()
            {
                //send MessageReceived to command handler
                _client.MessageReceived += HandleCommandAsync;

                //discover and load command modules in the entry assembly
                await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
            }

            private async Task HandleCommandAsync(SocketMessage messageParam)
            {
                //Don't process commands from system messages
                var message = messageParam as SocketUserMessage;
                if (message == null) return;

                //Create number to track where the prefix ends and the command begins
                int argPos = 0;

                //Ignore messages without !prefix or self mention and ensure bots cannot trigger commands
                if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) || message.Author.IsBot) return;

                //Create websocket-based command context based on the message
                var context = new SocketCommandContext(_client, message);

                //Execute the command with the context we create, along with the service provider
                //Doesn't indicate return value, but only if command was successful
                var result = await _commands.ExecuteAsync(context: context, argPos: argPos, services: null);

                //print error if command doesn't succeed
                if (!result.IsSuccess)
                {
                    await context.Channel.SendMessageAsync(result.ErrorReason);
                }
            }
        }
        
        public class InfoModule : ModuleBase<SocketCommandContext>
        {
            //!say whatever -> whatever
            [Command("say")]
            [Summary("Echoes a message.")]
            public Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
            {
                //testing proper initialization
                //WriteLine("Command 'works'"); 
                return ReplyAsync(echo);
            }

            /*[Command("")]
            [Summary("Parses weather input.")]
            public Task WeatherAsync([Remainder] [Summary("The city and state to look up")] string loc)
            {
                return "hello";
            }*/
        }
    }
}
