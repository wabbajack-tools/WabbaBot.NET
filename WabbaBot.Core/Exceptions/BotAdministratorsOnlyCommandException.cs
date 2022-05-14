﻿using DSharpPlus.SlashCommands;
using Wabbajack.DTOs;

namespace WabbaBot.Core.Exceptions {
    public class BotAdministratorsOnlyCommandException : ACommandException {
        public BotAdministratorsOnlyCommandException(InteractionContext ic) : base(ic, $"You must be a bot administrator to use that command!") {
        }
    }
}
