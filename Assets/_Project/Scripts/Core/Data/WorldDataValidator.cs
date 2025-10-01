using System;
using System.Collections.Generic;
using System.Linq;

namespace Wastelands.Core.Data
{
    public sealed class WorldDataValidator
    {
        public ValidationResult Validate(WorldData world)
        {
            var errors = new List<string>();
            if (world == null)
            {
                errors.Add("World data is null.");
                return ValidationResult.FromErrors(errors);
            }

            WorldDataNormalizer.Normalize(world);

            ValidateTiles(world, errors);
            ValidateFactions(world, errors);
            ValidateSettlements(world, errors);
            ValidateCharacters(world, errors);
            ValidateEvents(world, errors);
            ValidateLegends(world, errors);
            ValidateOracle(world, errors);
            ValidateBaseState(world, errors);

            return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.FromErrors(errors);
        }

        private static void ValidateTiles(WorldData world, ICollection<string> errors)
        {
            if (!EnsureUnique(world.Tiles.Select(t => t.Id), "Tile", errors))
            {
                return;
            }

            foreach (var tile in world.Tiles)
            {
                if (string.IsNullOrWhiteSpace(tile.Id))
                {
                    errors.Add("Tile ID must not be empty.");
                }
            }
        }

        private static void ValidateFactions(WorldData world, ICollection<string> errors)
        {
            if (!EnsureUnique(world.Factions.Select(f => f.Id), "Faction", errors))
            {
                return;
            }

            var factionIds = new HashSet<string>(world.Factions.Select(f => f.Id), StringComparer.Ordinal);
            foreach (var faction in world.Factions)
            {
                if (string.IsNullOrWhiteSpace(faction.Id))
                {
                    errors.Add("Faction ID must not be empty.");
                }

                foreach (var relation in faction.Relations)
                {
                    if (!factionIds.Contains(relation.TargetFactionId))
                    {
                        errors.Add($"Faction relation points to unknown faction '{relation.TargetFactionId}'.");
                    }
                }

                foreach (var roster in faction.NobleRoster)
                {
                    if (string.IsNullOrWhiteSpace(roster.CharacterId))
                    {
                        errors.Add($"Faction '{faction.Id}' has a noble role without character id.");
                    }
                }

                foreach (var holding in faction.Holdings)
                {
                    if (!world.Settlements.Any(s => s.Id == holding))
                    {
                        errors.Add($"Faction '{faction.Id}' references unknown settlement '{holding}'.");
                    }
                }
            }
        }

        private static void ValidateSettlements(WorldData world, ICollection<string> errors)
        {
            if (!EnsureUnique(world.Settlements.Select(s => s.Id), "Settlement", errors))
            {
                return;
            }

            var factionIds = new HashSet<string>(world.Factions.Select(f => f.Id), StringComparer.Ordinal);
            var tileIds = new HashSet<string>(world.Tiles.Select(t => t.Id), StringComparer.Ordinal);

            foreach (var settlement in world.Settlements)
            {
                if (!factionIds.Contains(settlement.FactionId))
                {
                    errors.Add($"Settlement '{settlement.Id}' references unknown faction '{settlement.FactionId}'.");
                }

                if (!tileIds.Contains(settlement.TileId))
                {
                    errors.Add($"Settlement '{settlement.Id}' references unknown tile '{settlement.TileId}'.");
                }
            }
        }

        private static void ValidateCharacters(WorldData world, ICollection<string> errors)
        {
            if (!EnsureUnique(world.Characters.Select(c => c.Id), "Character", errors))
            {
                return;
            }

            var factionIds = new HashSet<string>(world.Factions.Select(f => f.Id), StringComparer.Ordinal);
            var characterIds = new HashSet<string>(world.Characters.Select(c => c.Id), StringComparer.Ordinal);

            foreach (var character in world.Characters)
            {
                if (!factionIds.Contains(character.FactionId))
                {
                    errors.Add($"Character '{character.Id}' references unknown faction '{character.FactionId}'.");
                }

                foreach (var relationship in character.Relationships)
                {
                    if (!characterIds.Contains(relationship.TargetId))
                    {
                        errors.Add($"Character '{character.Id}' has relationship to unknown character '{relationship.TargetId}'.");
                    }
                }
            }

            foreach (var faction in world.Factions)
            {
                foreach (var roster in faction.NobleRoster)
                {
                    if (!characterIds.Contains(roster.CharacterId))
                    {
                        errors.Add($"Faction '{faction.Id}' assigns noble role '{roster.Role}' to unknown character '{roster.CharacterId}'.");
                    }
                }
            }
        }

        private static void ValidateEvents(WorldData world, ICollection<string> errors)
        {
            if (!EnsureUnique(world.Events.Select(e => e.Id), "Event", errors))
            {
                return;
            }

            var characterIds = new HashSet<string>(world.Characters.Select(c => c.Id), StringComparer.Ordinal);
            var tileIds = new HashSet<string>(world.Tiles.Select(t => t.Id), StringComparer.Ordinal);
            var settlementIds = new HashSet<string>(world.Settlements.Select(s => s.Id), StringComparer.Ordinal);

            foreach (var record in world.Events)
            {
                foreach (var actor in record.Actors)
                {
                    if (!characterIds.Contains(actor))
                    {
                        errors.Add($"Event '{record.Id}' references unknown character '{actor}'.");
                    }
                }

                if (!string.IsNullOrEmpty(record.LocationId))
                {
                    var valid = tileIds.Contains(record.LocationId) || settlementIds.Contains(record.LocationId) ||
                                string.Equals(record.LocationId, world.BaseState.SiteTileId, StringComparison.Ordinal);

                    if (!valid)
                    {
                        errors.Add($"Event '{record.Id}' references unknown location '{record.LocationId}'.");
                    }
                }
            }
        }

        private static void ValidateLegends(WorldData world, ICollection<string> errors)
        {
            if (!EnsureUnique(world.Legends.Select(l => l.Id), "Legend", errors))
            {
                return;
            }

            var eventIds = new HashSet<string>(world.Events.Select(e => e.Id), StringComparer.Ordinal);
            foreach (var legend in world.Legends)
            {
                foreach (var id in legend.EventIds)
                {
                    if (!eventIds.Contains(id))
                    {
                        errors.Add($"Legend '{legend.Id}' references unknown event '{id}'.");
                    }
                }
            }
        }

        private static void ValidateOracle(WorldData world, ICollection<string> errors)
        {
            var deckIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var deck in world.OracleState.AvailableDecks)
            {
                if (!deckIds.Add(deck.Id))
                {
                    errors.Add($"Oracle deck id '{deck.Id}' is duplicated.");
                }

                var cardIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (var card in deck.Cards)
                {
                    if (!cardIds.Add(card.Id))
                    {
                        errors.Add($"Deck '{deck.Id}' has duplicate card id '{card.Id}'.");
                    }
                }
            }
        }

        private static void ValidateBaseState(WorldData world, ICollection<string> errors)
        {
            var baseState = world.BaseState;
            if (baseState == null)
            {
                errors.Add("Base state is missing.");
                return;
            }

            var zoneIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var zone in baseState.Zones)
            {
                if (!zoneIds.Add(zone.Id))
                {
                    errors.Add($"Base zone id '{zone.Id}' is duplicated.");
                }
            }

            var characterIds = new HashSet<string>(world.Characters.Select(c => c.Id), StringComparer.Ordinal);
            foreach (var member in baseState.Population)
            {
                if (!characterIds.Contains(member))
                {
                    errors.Add($"Base population references unknown character '{member}'.");
                }
            }
        }

        private static bool EnsureUnique(IEnumerable<string> ids, string label, ICollection<string> errors)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in ids)
            {
                if (!set.Add(id))
                {
                    errors.Add($"Duplicate {label} id '{id}'.");
                }
            }

            return errors.Count == 0;
        }
    }

    public readonly struct ValidationResult
    {
        private ValidationResult(bool isValid, IReadOnlyList<string> errors)
        {
            IsValid = isValid;
            Errors = errors;
        }

        public bool IsValid { get; }
        public IReadOnlyList<string> Errors { get; }

        public static ValidationResult Success() => new(true, Array.Empty<string>());

        public static ValidationResult FromErrors(IEnumerable<string> errors)
        {
            var list = errors.ToArray();
            return new ValidationResult(list.Length == 0, list);
        }
    }
}
