using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Wastelands.Core.Services;

namespace Wastelands.BaseMode
{
    public interface IBaseIndirectCommandDispatcher
    {
        IReadOnlyList<BaseIndirectCommand> RecentCommands { get; }
        void Issue(BaseIndirectCommand command);
    }

    public sealed class BaseIndirectCommandDispatcher : IBaseIndirectCommandDispatcher
    {
        private readonly IEventBus _eventBus;
        private readonly List<BaseIndirectCommand> _recentCommands = new();
        private readonly int _historyLimit;

        public BaseIndirectCommandDispatcher(IEventBus eventBus, int historyLimit = 32)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            if (historyLimit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(historyLimit));
            }

            _historyLimit = historyLimit;
            RecentCommands = new ReadOnlyCollection<BaseIndirectCommand>(_recentCommands);
        }

        public IReadOnlyList<BaseIndirectCommand> RecentCommands { get; }

        public void Issue(BaseIndirectCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.CommandType))
            {
                throw new ArgumentException("CommandType must be provided.", nameof(command));
            }

            _recentCommands.Add(command);
            if (_recentCommands.Count > _historyLimit)
            {
                _recentCommands.RemoveAt(0);
            }

            _eventBus.Publish(new BaseIndirectCommandQueued(command));
        }
    }

    public readonly struct BaseIndirectCommand
    {
        public BaseIndirectCommand(string commandType, string? targetId = null, IReadOnlyDictionary<string, string>? payload = null)
        {
            if (string.IsNullOrWhiteSpace(commandType))
            {
                throw new ArgumentException("Command type must be supplied.", nameof(commandType));
            }

            CommandType = commandType;
            TargetId = targetId;
            Payload = payload != null
                ? new ReadOnlyDictionary<string, string>(payload.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal))
                : EmptyPayload;
        }

        public string CommandType { get; }
        public string? TargetId { get; }
        public IReadOnlyDictionary<string, string> Payload { get; }

        private static IReadOnlyDictionary<string, string> EmptyPayload { get; } = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
    }

    public readonly struct BaseIndirectCommandQueued
    {
        public BaseIndirectCommandQueued(BaseIndirectCommand command)
        {
            Command = command;
        }

        public BaseIndirectCommand Command { get; }
    }

    public readonly struct BaseIndirectCommandDispatcherReady
    {
        public BaseIndirectCommandDispatcherReady(IBaseIndirectCommandDispatcher dispatcher)
        {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public IBaseIndirectCommandDispatcher Dispatcher { get; }
    }
}
