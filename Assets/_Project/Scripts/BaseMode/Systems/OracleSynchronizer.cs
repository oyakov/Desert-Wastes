using System;
using System.Collections.Generic;
using System.Linq;
using Wastelands.Core.Data;

namespace Wastelands.BaseMode
{
    internal static class OracleSynchronizer
    {
        private const float RaidTensionIncrease = 0.08f;
        private const float MandateCompletionDelta = -0.05f;
        private const float MandateFailureDelta = 0.06f;
        private const int DefaultIncidentCooldown = 6;

        public static void RecordRaidOutcome(in BaseModeTickContext context, string attackerFactionId, string eventId)
        {
            if (context.World.OracleState == null)
            {
                return;
            }

            context.World.OracleState.TensionScore = BaseMath.Clamp01(context.World.OracleState.TensionScore + RaidTensionIncrease);

            var triggerParameters = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "attacker", attackerFactionId },
                { "eventId", eventId }
            };

            TryInjectIncident(context, "raid", triggerParameters);
        }

        public static void RecordMandateOutcome(in BaseModeTickContext context, BaseMandateResolution resolution)
        {
            if (context.World.OracleState == null)
            {
                return;
            }

            var delta = resolution.Result switch
            {
                MandateStatus.Completed => MandateCompletionDelta,
                MandateStatus.Failed => MandateFailureDelta,
                _ => 0f
            };

            if (Math.Abs(delta) > float.Epsilon)
            {
                context.World.OracleState.TensionScore = BaseMath.Clamp01(context.World.OracleState.TensionScore + delta);
            }

            var triggerParameters = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "mandateId", resolution.Mandate.Id },
                { "result", resolution.Result.ToString() }
            };

            TryInjectIncident(context, "mandate", triggerParameters);
        }

        private static void TryInjectIncident(in BaseModeTickContext context, string trigger, IReadOnlyDictionary<string, string> triggerParameters)
        {
            var oracle = context.World.OracleState;
            if (oracle == null || string.IsNullOrEmpty(oracle.ActiveDeckId))
            {
                return;
            }

            var deck = oracle.AvailableDecks.FirstOrDefault(d => string.Equals(d.Id, oracle.ActiveDeckId, StringComparison.Ordinal));
            if (deck == null)
            {
                return;
            }

            var availableCards = deck.Cards
                .Where(card => card != null && IsCardAvailable(oracle, card.Id))
                .ToList();

            if (availableCards.Count == 0)
            {
                return;
            }

            var channel = context.GetChannel($"oracle.{trigger}.{deck.Id}");
            var selected = availableCards[channel.NextInt(0, availableCards.Count)];

            oracle.Cooldowns[selected.Id] = DefaultIncidentCooldown;

            var payload = new Dictionary<string, string>(triggerParameters, StringComparer.Ordinal);
            var clonedEffects = CloneEffects(selected.Effects);

            context.EventBus.Publish(new OracleIncidentInjected(
                deck.Id,
                selected.Id,
                selected.Narrative,
                trigger,
                payload,
                clonedEffects,
                context.Tick));
        }

        private static bool IsCardAvailable(OracleState oracle, string cardId)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                return false;
            }

            return !oracle.Cooldowns.TryGetValue(cardId, out var remaining) || remaining <= 0;
        }

        private static IReadOnlyList<EventEffect> CloneEffects(IEnumerable<EventEffect> effects)
        {
            var list = new List<EventEffect>();
            if (effects == null)
            {
                return list;
            }

            foreach (var effect in effects)
            {
                list.Add(new EventEffect
                {
                    EffectType = effect.EffectType,
                    Parameters = effect.Parameters != null
                        ? new Dictionary<string, string>(effect.Parameters, StringComparer.Ordinal)
                        : new Dictionary<string, string>(StringComparer.Ordinal)
                });
            }

            return list;
        }
    }

    public readonly struct OracleIncidentInjected
    {
        public OracleIncidentInjected(
            string deckId,
            string cardId,
            string narrative,
            string trigger,
            IReadOnlyDictionary<string, string> triggerParameters,
            IReadOnlyList<EventEffect> effects,
            long tick)
        {
            DeckId = deckId ?? throw new ArgumentNullException(nameof(deckId));
            CardId = cardId ?? throw new ArgumentNullException(nameof(cardId));
            Narrative = narrative ?? string.Empty;
            Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
            TriggerParameters = triggerParameters ?? throw new ArgumentNullException(nameof(triggerParameters));
            Effects = effects ?? throw new ArgumentNullException(nameof(effects));
            Tick = tick;
        }

        public string DeckId { get; }
        public string CardId { get; }
        public string Narrative { get; }
        public string Trigger { get; }
        public IReadOnlyDictionary<string, string> TriggerParameters { get; }
        public IReadOnlyList<EventEffect> Effects { get; }
        public long Tick { get; }
    }
}
