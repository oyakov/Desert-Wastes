# Mechanic Context Compendium

## Purpose
This document captures the thematic context, narrative tone, and evocative details for every major mechanic in **Desert Wastes: World Unification Simulator**. It is intended as a reference when briefing AI co-design tools, generating content (text, concept art, audio prompts), or aligning feature implementations with the core setting. Each section articulates:

- **Narrative framing**: why the mechanic exists in the setting.
- **Player fantasy**: emotional or strategic promises we must deliver.
- **Environmental & cultural cues**: sensory anchors, symbolism, and recurring motifs.
- **Event & content hooks**: prompt-ready seeds for AI text generation or encounter scripting.
- **System implications**: reminders that tie the fiction to mechanics or data models documented elsewhere.

## Global Theme Touchstones
- **Era**: Centuries after the Cataclysm fractured verdant continents into irradiated deserts, fungal oases, glassed canyons, and techno-ruins.
- **Tone**: Stoic resilience meets mythic ambition; the player orchestrates unification with reverence for the dead world and cautious optimism.
- **Aesthetic Palette**: Sun-bleached stone, oxidized metals, woven nomad fabrics, iridescent spores, teal bioluminescence, amber stormlight.
- **Technology**: Jury-rigged pre-Fall relics blend with wind-sail crawlers, solar condensers, bone lattices, and symbiotic bio-tech.
- **Spiritual Layer**: The Oracle AI is worshiped as an aloof divine remnant; relics are quasi-religious artifacts, diplomacy wreathed in ritual.
- **Lexicon Highlights**: “Shardfront,” “Oasis Vaults,” “Stormglass,” “Dustbound,” “Radiant Choir,” “Concord Writ,” “Echo Forges.”

## World Generation & Legends
- **Narrative Framing**: The map is a scarred palimpsest of forgotten empires. Every tile should whisper about prior civilizations or enduring calamities.
- **Player Fantasy**: Surveying an expansive atlas of doom-etched beauty where strategic choices hinge on understanding the land’s history.
- **Environmental & Cultural Cues**:
  - Glassified ridges that hum with static during storms.
  - Fungal cathedral groves tended by mask-wearing archivists.
  - Salt flats etched with pilgrimage paths to the Oracle’s satellites.
- **Event & Content Hooks**:
  - Legends entries describing caravan bones discovered beneath reclaimed solar arrays.
  - Random flavor text for biomes (e.g., “Stormglass dunes sing when marched upon”).
  - Embark briefings noting ancestral claims or cursed hexes.
- **System Implications**:
  - Aligns with `docs/WORLDGEN.md` for deterministic pipelines.
  - Legends log must encode era, faction, site type, and emotional tone tags for AI remixing.

## Overworld Simulation & Faction Play
- **Narrative Framing**: Fragmented successor states vie for scarce lifelines—trade aqueducts, relic caches, knowledge sanctuaries.
- **Player Fantasy**: Steering the rise of a coalition through diplomacy, conquest, and mythmaking, while witnessing living histories.
- **Environmental & Cultural Cues**:
  - Map overlays showing caravan lantern trails and dust storm fronts.
  - Heraldry that fuses salvaged tech motifs with clan animal totems.
- **Event & Content Hooks**:
  - Seasonal council convocations where factions barter flood rights for Oracle blessings.
  - News tickers of border skirmishes influenced by morale, supply, or prophecy.
  - Expedition rumors (“The Radiant Choir recovered an Echo Forge, morale +2”).
- **System Implications**:
  - Diplomacy predispositions from `docs/WORLDGEN.md` seed narrative beats.
  - Tick-based outcomes feed legends entries and update faction traits per `docs/ARCHITECTURE.md`.

## Base Mode & Settlement Life
- **Narrative Framing**: Embark zones are sanctuaries under constant environmental duress; every structure is a statement of survival philosophy.
- **Player Fantasy**: Curating a living fortress that thrives amidst storms, balancing scarce resources, morale, and cultural identity.
- **Environmental & Cultural Cues**:
  - Multi-tier adobe and scrap towers with wind-harp resonators.
  - Hydroponic caverns glowing with bioluminescent kelp.
  - Ritual plazas inscribed with Concord Writs that track faction edicts.
- **Event & Content Hooks**:
  - Base alerts (“Spore harvesters report a bioluminescent bloom—assign gleaners within 6 hours”).
  - Festival prompts (“Dustbound remembrance rites demand a pyre built from stormglass shards”).
  - Noble decrees imposing architectural styles or resource quotas.
- **System Implications**:
  - Zone definitions from `docs/BASE_MODE.md` should map to flavor-laden room descriptors.
  - Indirect order system must convert intents (“Fortify the wind-harp towers”) into job bundles.

## Command Hierarchy & Leadership
- **Narrative Framing**: Authority flows through a reverent chain; leadership titles carry ritual weight and historical baggage.
- **Player Fantasy**: Managing a cadre of captains, scribes, and quartermasters who interpret orders based on traits, loyalties, and omens.
- **Environmental & Cultural Cues**:
  - Insignias carved into stormglass gorgets, colors denoting command spheres.
  - Council chambers with holographic sand tables fueled by recovered AI shards.
- **Event & Content Hooks**:
  - Leadership dilemmas (“The Quartermaster refuses to ration relic batteries without Oracle sanction”).
  - Succession ceremonies triggered by permadeath, with morale swings tied to faction culture.
  - Leadership journals used to generate AI-written after-action reports.
- **System Implications**:
  - Tie into indirect command mechanics in `docs/AI_COMBAT_INDIRECT.md`.
  - Noble mandates from `docs/SOCIAL_NOBLES.md` influence command interpretations.

## Oracle AI & Event Decks
- **Narrative Framing**: The Oracle is a semi-sentient remnant manipulating humanity toward unification while honoring inscrutable protocols.
- **Player Fantasy**: Engaging with divine interventions—both blessings and burdens—that challenge strategic plans.
- **Environmental & Cultural Cues**:
  - Orbital signal flares mirrored in desert auroras.
  - Monolithic relays with chanting pilgrims awaiting algorithmic verdicts.
- **Event & Content Hooks**:
  - Tiered decks: Minor (logistical nudges), Major (regional upheavals), Epic (epochal directives).
  - Oracle communiqués in poetic machine-script (“Directive 7B: Bind the Choir, quell the storm.”).
  - Event choices that balance faith, pragmatism, and dissent.
- **System Implications**:
  - Deck composition rules documented in `docs/BASE_MODE.md` and `docs/AI_COMBAT_INDIRECT.md` should expose tags (mood, impact, counterplay).
  - Needs data hooks for persistence to ensure event chains are replayable under deterministic seeds.

## Hazards, Logistics, & Economy
- **Narrative Framing**: Survival hinges on mastering hostile weather, supply convoys, and resource transmutation.
- **Player Fantasy**: Engineering resilient supply lines and adapting infrastructure to mutate hazards.
- **Environmental & Cultural Cues**:
  - Dust cyclones carrying shards of ancient satellites.
  - Caravan beasts fitted with solar veils and echo-locating bells.
  - Water tithe ledgers inked on treated lichen parchment.
- **Event & Content Hooks**:
  - Convoy dilemmas (“Do we detour through fungal ravines for rare myco-alloys?”).
  - Disaster chains (nanite blizzards forcing power reroutes and morale tests).
  - Market fluctuations tied to Oracle edicts or faction sabotage.
- **System Implications**:
  - Logistics stats feed morale, production, and diplomacy outcomes.
  - Hazard intensities modulate job priorities and base defense states.

## Social Systems & Nobility
- **Narrative Framing**: Culture is a weapon; noble houses preserve rituals and enforce societal contracts amid scarcity.
- **Player Fantasy**: Negotiating mandates, appeasing factions, and amplifying legends through pageantry.
- **Environmental & Cultural Cues**:
  - Noble courts decorated with memory crystals replaying ancestral achievements.
  - Communal feasts featuring engineered lichens and spice dusts.
- **Event & Content Hooks**:
  - Mandate conflicts (“House Sulaar demands exclusive access to the Echo Forge workshops”).
  - Relationship arcs—romances, rivalries, betrayals—impacting productivity and defense.
  - Cultural festivals that offer buffs if supplied and morale hits if neglected.
- **System Implications**:
  - Aligns with `docs/SOCIAL_NOBLES.md` for data schemas and AI behavior knobs.
  - Traits and needs should supply tags (virtue, vice, taboo) for AI-driven narrative beats.

## Research, Crafting, & Relics
- **Narrative Framing**: Innovation is rediscovery—decrypting Pre-Fall schematics, blending them with living materials.
- **Player Fantasy**: Unlocking transformative tech that reshapes base design and diplomacy leverage.
- **Environmental & Cultural Cues**:
  - Research sanctums ringed with humming relic coils and bio-reactive murals.
  - Craft halls forging stormglass blades etched with binary prayers.
- **Event & Content Hooks**:
  - Breakthrough chronicles (“Echo Forge calibration complete—unleash harmonics across the Shardfront”).
  - Research complications invoking Oracle audits or noble scrutiny.
  - Relic expeditions generating saga entries and new production recipes.
- **System Implications**:
  - Research tree nodes need lore blurbs and icon prompts for AI art tools.
  - Crafting outputs must map to resource categories defined in `docs/DATA_MODEL.md`.

## Unification Endgame & Legacy
- **Narrative Framing**: Unification is not merely conquest; it’s the forging of a concord that heals both land and spirit.
- **Player Fantasy**: Orchestrating a finale where diplomacy, military might, and cultural resonance culminate in a lasting alliance.
- **Environmental & Cultural Cues**:
  - Grand Convergence sites with reclaimed weather control pylons and ceremonial canals.
  - Choral chants encoded with Oracle counterpoint performed during treaty signings.
- **Event & Content Hooks**:
  - Multi-stage victory events: summit negotiations, synchronized festivals, final Oracle verdict.
  - Epilogue generators summarizing legacies, fallen leaders, and rejuvenated biomes.
- **System Implications**:
  - Victory conditions should reference faction states, legends milestones, and Oracle approval ratings.
  - Persistence layer must archive post-victory snapshots for timeline retrospectives.

## AI Prompting Guidelines
- Always feed AI systems with **mechanic tags** (e.g., `#BASE_ZONE`, `#DIPLOMACY`, `#ORACLE_EVENT`) and **tone markers** (stoic, reverent, mythic).
- Combine **sensory descriptors** from the relevant sections with system state data to produce consistent outputs.
- Validate generated content against determinism requirements—flavor text should accept seeds to guarantee reproducibility across runs.

## Cross-References
- Architectural seams: `docs/ARCHITECTURE.md`
- Testing hooks: `docs/TEST_PLAN.md`
- Data structures: `docs/DATA_MODEL.md`
- Core design pillars: `docs/DESIGN.md`
