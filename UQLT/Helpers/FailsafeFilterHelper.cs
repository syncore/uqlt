﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Helpers
{
    // fail-safe hard-coded filter list in case site cannot be reached to download current filters & they don't exist
    public class FailsafeFilterHelper
    {
        public void DumpBackupFilters()
        {
            string filterbackup = @"{ ""active_locations"": [ { ""display_name"": ""In your vicinity"", ""location_id"": ""any"" }, { ""display_name"": ""All Locations"", ""location_id"": ""ALL"" }, { ""display_name"": ""Africa"", ""location_id"": ""Africa"" }, { ""display_name"": ""Asia"", ""location_id"": ""Asia"" }, { ""display_name"": ""Europe"", ""location_id"": ""Europe"" }, { ""display_name"": ""North America"", ""location_id"": ""North America"" }, { ""display_name"": ""Oceania"", ""location_id"": ""Oceania"" }, { ""display_name"": ""South America"", ""location_id"": ""South America"" }, { ""display_name"": ""Lan Event: AU LAN (AUS, Adelaide)"", ""location_id"": 60 }, { ""display_name"": ""Lan Event: Dreamhack (SWE, Jonkoping)"", ""location_id"": 54 }, { ""display_name"": ""Lan Event: ESL (DEU, Cologne)"", ""location_id"": 52 }, { ""display_name"": ""Lan Event: NL LAN (NLD, Benelux)"", ""location_id"": 61 }, { ""display_name"": ""Lan Event: QuakeCon BYOC (USA, TX)"", ""location_id"": 666 }, { ""display_name"": ""Lan Event: QuakeCon (USA, Dallas)"", ""location_id"": 53 }, { ""display_name"": ""Lan Event :QuakeCon (USA, Richardson)"", ""location_id"": 12 }, { ""display_name"": ""Lan Event: UGC (ITA, Lignano Sabbiadoro)"", ""location_id"": 59 }, { ""display_name"": ""ARG, Buenos Aires"", ""location_id"": 40 }, { ""display_name"": ""AUS, Adelaide"", ""location_id"": 51 }, { ""display_name"": ""AUS, Sydney"", ""location_id"": 14 }, { ""display_name"": ""AUS, Sydney #2"", ""location_id"": 33 }, { ""display_name"": ""BRA, Sao Paulo"", ""location_id"": 66 }, { ""display_name"": ""CAN, Toronto"", ""location_id"": 26 }, { ""display_name"": ""CHL, Santiago"", ""location_id"": 38 }, { ""display_name"": ""DEU, Frankfurt"", ""location_id"": 18 }, { ""display_name"": ""ESP, Madrid"", ""location_id"": 28 }, { ""display_name"": ""GBR, Maidenhead"", ""location_id"": 19 }, { ""display_name"": ""ITA, Milan"", ""location_id"": 50 }, { ""display_name"": ""KOR, Seoul"", ""location_id"": 46 }, { ""display_name"": ""NLD, Amsterdam"", ""location_id"": 17 }, { ""display_name"": ""NOR, Oslo"", ""location_id"": 65 }, { ""display_name"": ""NZL, Auckland"", ""location_id"": 68 }, { ""display_name"": ""POL, Warsaw"", ""location_id"": 32 }, { ""display_name"": ""ROM, Bucharest"", ""location_id"": 37 }, { ""display_name"": ""RUS, Moscow"", ""location_id"": 44 }, { ""display_name"": ""SGP, Singapore"", ""location_id"": 45 }, { ""display_name"": ""SWE, Stockholm"", ""location_id"": 29 }, { ""display_name"": ""TUR, Istanbul"", ""location_id"": 67 }, { ""display_name"": ""UKR, Kiev"", ""location_id"": 58 }, { ""display_name"": ""USA, Ashburn"", ""location_id"": 11 }, { ""display_name"": ""USA, Chicago"", ""location_id"": 21 }, { ""display_name"": ""USA, Dallas"", ""location_id"": 6 }, { ""display_name"": ""USA, Indianapolis"", ""location_id"": 63 }, { ""display_name"": ""USA, Los Angeles"", ""location_id"": 25 }, { ""display_name"": ""USA, Seattle"", ""location_id"": 23 }, { ""display_name"": ""USA, Washington DC"", ""location_id"": 62 }, { ""display_name"": ""ZAF, Johannesburg"", ""location_id"": 46 } ], ""inactive_locations"": [ { ""display_name"": ""AUS, Perth"", ""location_id"": 35 }, { ""display_name"": ""BGR, Sofia"", ""location_id"": 48 }, { ""display_name"": ""CHN, Hangzhou"", ""location_id"": 31 }, { ""display_name"": ""FRA, Paris"", ""location_id"": 20 }, { ""display_name"": ""ISL, Keflavik"", ""location_id"": 41 }, { ""display_name"": ""JPN, Tokyo"", ""location_id"": 27 }, { ""display_name"": ""JPN, Tokyo #2"", ""location_id"": 42 }, { ""display_name"": ""NLD, Rotterdam"", ""location_id"": 64 }, { ""display_name"": ""POL, Warsaw"", ""location_id"": 30 }, { ""display_name"": ""ROM, Bucharest"", ""location_id"": 39 }, { ""display_name"": ""RUS, Moscow"", ""location_id"": 43 }, { ""display_name"": ""SRB, Beograd"", ""location_id"": 47 }, { ""display_name"": ""SWE, Malmo"", ""location_id"": 34 }, { ""display_name"": ""SWE, Stockholm"", ""location_id"": 36 }, { ""display_name"": ""USA, Atlanta"", ""location_id"": 22 }, { ""display_name"": ""USA, New York"", ""location_id"": 24 }, { ""display_name"": ""USA, Palo Alto"", ""location_id"": 10 }, { ""display_name"": ""USA, San Francisco"", ""location_id"": 16 } ], ""serverbrowser_locations"": [ { ""display_name"": ""Lan Event: AU LAN (AUS, Adelaide)"", ""location_id"": 60 }, { ""display_name"": ""Lan Event: Dreamhack (SWE, Jonkoping)"", ""location_id"": 54 }, { ""display_name"": ""Lan Event: ESL (DEU, Cologne)"", ""location_id"": 52 }, { ""display_name"": ""Lan Event: NL LAN (NLD, Benelux)"", ""location_id"": 61 }, { ""display_name"": ""Lan Event: QuakeCon BYOC (USA, TX)"", ""location_id"": 666 }, { ""display_name"": ""Lan Event: QuakeCon (USA, Dallas)"", ""location_id"": 53 }, { ""display_name"": ""Lan Event :QuakeCon (USA, Richardson)"", ""location_id"": 12 }, { ""display_name"": ""Lan Event: UGC (ITA, Lignano Sabbiadoro)"", ""location_id"": 59 }, { ""display_name"": ""ARG, Buenos Aires"", ""location_id"": 40 }, { ""display_name"": ""AUS, Adelaide"", ""location_id"": 51 }, { ""display_name"": ""AUS, Sydney"", ""location_id"": 14 }, { ""display_name"": ""AUS, Sydney #2"", ""location_id"": 33 }, { ""display_name"": ""BRA, Sao Paulo"", ""location_id"": 66 }, { ""display_name"": ""CAN, Toronto"", ""location_id"": 26 }, { ""display_name"": ""CHL, Santiago"", ""location_id"": 38 }, { ""display_name"": ""DEU, Frankfurt"", ""location_id"": 18 }, { ""display_name"": ""ESP, Madrid"", ""location_id"": 28 }, { ""display_name"": ""GBR, Maidenhead"", ""location_id"": 19 }, { ""display_name"": ""ITA, Milan"", ""location_id"": 50 }, { ""display_name"": ""KOR, Seoul"", ""location_id"": 46 }, { ""display_name"": ""NLD, Amsterdam"", ""location_id"": 17 }, { ""display_name"": ""NOR, Oslo"", ""location_id"": 65 }, { ""display_name"": ""NZL, Auckland"", ""location_id"": 68 }, { ""display_name"": ""POL, Warsaw"", ""location_id"": 32 }, { ""display_name"": ""ROM, Bucharest"", ""location_id"": 37 }, { ""display_name"": ""RUS, Moscow"", ""location_id"": 44 }, { ""display_name"": ""SGP, Singapore"", ""location_id"": 45 }, { ""display_name"": ""SWE, Stockholm"", ""location_id"": 29 }, { ""display_name"": ""TUR, Istanbul"", ""location_id"": 67 }, { ""display_name"": ""UKR, Kiev"", ""location_id"": 58 }, { ""display_name"": ""USA, Ashburn"", ""location_id"": 11 }, { ""display_name"": ""USA, Chicago"", ""location_id"": 21 }, { ""display_name"": ""USA, Dallas"", ""location_id"": 6 }, { ""display_name"": ""USA, Indianapolis"", ""location_id"": 63 }, { ""display_name"": ""USA, Los Angeles"", ""location_id"": 25 }, { ""display_name"": ""USA, Seattle"", ""location_id"": 23 }, { ""display_name"": ""USA, Washington DC"", ""location_id"": 62 }, { ""display_name"": ""ZAF, Johannesburg"", ""location_id"": 46 } ], ""arenas"": [ { ""display_name"": ""All Arenas"", ""arena_type"": """", ""arena"": ""any"" }, { ""display_name"": ""Basic Complexity"", ""arena_type"": ""tag"", ""arena"": ""BASIC"" }, { ""display_name"": ""Intermediate Complexity"", ""arena_type"": ""tag"", ""arena"": ""INTERMEDIATE"" }, { ""display_name"": ""Small Size"", ""arena_type"": ""tag"", ""arena"": ""SMALL"" }, { ""display_name"": ""Large Size"", ""arena_type"": ""tag"", ""arena"": ""LARGE"" }, { ""display_name"": ""QuakeCon 2008"", ""arena_type"": ""tag"", ""arena"": ""QCON08"" }, { ""display_name"": ""QuakeCon 2009"", ""arena_type"": ""tag"", ""arena"": ""QCON09"" }, { ""display_name"": ""Intel Extreme Masters 4"", ""arena_type"": ""tag"", ""arena"": ""IEM4"" }, { ""display_name"": ""DreamHack Summer 2010"", ""arena_type"": ""tag"", ""arena"": ""DHS10"" }, { ""display_name"": ""QuakeCon 2010"", ""arena_type"": ""tag"", ""arena"": ""QCON10"" }, { ""display_name"": ""DreamHack Winter 2010"", ""arena_type"": ""tag"", ""arena"": ""DHW10"" }, { ""display_name"": ""Intel Extreme Masters 5"", ""arena_type"": ""tag"", ""arena"": ""IEM5"" }, { ""display_name"": ""DreamHack Summer 2011"", ""arena_type"": ""tag"", ""arena"": ""DHS11"" }, { ""display_name"": ""QuakeCon 2011"", ""arena_type"": ""tag"", ""arena"": ""QCON11"" }, { ""display_name"": ""DreamHack Winter 2011"", ""arena_type"": ""tag"", ""arena"": ""DHW11"" }, { ""display_name"": ""DreamHack Summer 2012"", ""arena_type"": ""tag"", ""arena"": ""DHS12"" }, { ""display_name"": ""QuakeCon 2012"", ""arena_type"": ""tag"", ""arena"": ""QCON12"" }, { ""display_name"": ""DreamHack Winter 2012"", ""arena_type"": ""tag"", ""arena"": ""DHW12"" }, { ""display_name"": ""Face IT EU Legends Cup"", ""arena_type"": ""tag"", ""arena"": ""FIEUL"" }, { ""display_name"": ""QuakeCon 2013"", ""arena_type"": ""tag"", ""arena"": ""QCON13"" }, { ""display_name"": ""Quake III Arena"", ""arena_type"": ""tag"", ""arena"": ""Q3A"" }, { ""display_name"": ""Quake III Team Arena Q3TA"", ""arena_type"": ""tag"", ""arena"": ""Q3TA"" }, { ""display_name"": ""Quake 3 Dreamcast"", ""arena_type"": ""tag"", ""arena"": ""Q3DC"" }, { ""display_name"": ""Quake II"", ""arena_type"": ""tag"", ""arena"": ""Q2"" }, { ""display_name"": ""Q2World"", ""arena_type"": ""tag"", ""arena"": ""Q2W"" }, { ""display_name"": ""Quake 3 Community"", ""arena_type"": ""tag"", ""arena"": ""Q3C"" }, { ""display_name"": ""Challenge ProMode Arena"", ""arena_type"": ""tag"", ""arena"": ""CPMA"" }, { ""display_name"": ""Rocket Arena 3"", ""arena_type"": ""tag"", ""arena"": ""RA3"" }, { ""display_name"": ""Threewave CTF"", ""arena_type"": ""tag"", ""arena"": ""TWAVE"" }, { ""display_name"": ""Quake Arena Arcade"", ""arena_type"": ""tag"", ""arena"": ""QAA"" }, { ""display_name"": ""QUAKE LIVE"", ""arena_type"": ""tag"", ""arena"": ""QL"" }, { ""display_name"": ""Space"", ""arena_type"": ""tag"", ""arena"": ""SPACE"" }, { ""display_name"": ""Gothic"", ""arena_type"": ""tag"", ""arena"": ""GOTHIC"" }, { ""display_name"": ""Tech"", ""arena_type"": ""tag"", ""arena"": ""TECH"" }, { ""display_name"": ""Industrial"", ""arena_type"": ""tag"", ""arena"": ""INDUSTRIAL"" }, { ""display_name"": ""Standard Arenas"", ""arena_type"": ""tag"", ""arena"": ""STANDARD"" }, { ""display_name"": ""All Premium Arenas"", ""arena_type"": ""tag"", ""arena"": ""PREMIUM"" }, { ""display_name"": ""QL Premium Pak 1"", ""arena_type"": ""tag"", ""arena"": ""QLPP1"" }, { ""display_name"": ""QL Premium Pak 2"", ""arena_type"": ""tag"", ""arena"": ""QLPP2"" }, { ""display_name"": ""QL Premium Pak 3"", ""arena_type"": ""tag"", ""arena"": ""QLPP3"" }, { ""display_name"": ""QL Premium Pak 4"", ""arena_type"": ""tag"", ""arena"": ""QLPP4"" }, { ""display_name"": ""QL Premium Pak 5"", ""arena_type"": ""tag"", ""arena"": ""QLPP5"" }, { ""display_name"": ""QL Premium Pak 6"", ""arena_type"": ""tag"", ""arena"": ""QLPP6"" }, { ""display_name"": ""QL Premium Pak 7"", ""arena_type"": ""tag"", ""arena"": ""QLPP7"" }, { ""display_name"": ""QL Premium Pak 8"", ""arena_type"": ""tag"", ""arena"": ""QLPP8"" }, { ""display_name"": ""QL Premium Pak 9"", ""arena_type"": ""tag"", ""arena"": ""QLPP9"" }, { ""display_name"": ""QL Premium Pak 10"", ""arena_type"": ""tag"", ""arena"": ""QLPP10"" }, { ""display_name"": ""QL Premium Pak 11"", ""arena_type"": ""tag"", ""arena"": ""QLPP11"" }, { ""display_name"": ""QL Premium Pak 12"", ""arena_type"": ""tag"", ""arena"": ""QLPP12"" }, { ""display_name"": ""QL Premium Pak 13"", ""arena_type"": ""tag"", ""arena"": ""QLPP13"" }, { ""display_name"": ""QL Premium Pak 14"", ""arena_type"": ""tag"", ""arena"": ""QLPP14"" }, { ""display_name"": ""QL Premium Pak 15"", ""arena_type"": ""tag"", ""arena"": ""QLPP15"" }, { ""display_name"": ""QL Premium Pak 16"", ""arena_type"": ""tag"", ""arena"": ""QLPP16"" }, { ""display_name"": ""QL Premium Pak 17"", ""arena_type"": ""tag"", ""arena"": ""QLPP17"" }, { ""display_name"": ""QL Premium Pak 18"", ""arena_type"": ""tag"", ""arena"": ""QLPP18"" }, { ""display_name"": ""QL Premium Pak 19"", ""arena_type"": ""tag"", ""arena"": ""QLPP19"" }, { ""display_name"": ""Aerowalk"", ""arena_type"": ""map"", ""arena"": ""aerowalk"" }, { ""display_name"": ""Almost Lost"", ""arena_type"": ""map"", ""arena"": ""almostlost"" }, { ""display_name"": ""Arcane Citadel"", ""arena_type"": ""map"", ""arena"": ""arcanecitadel"" }, { ""display_name"": ""Arena Gate"", ""arena_type"": ""map"", ""arena"": ""arenagate"" }, { ""display_name"": ""Asylum"", ""arena_type"": ""map"", ""arena"": ""asylum"" }, { ""display_name"": ""Base Siege"", ""arena_type"": ""map"", ""arena"": ""basesiege"" }, { ""display_name"": ""Battleforged"", ""arena_type"": ""map"", ""arena"": ""battleforged"" }, { ""display_name"": ""Beyond Reality"", ""arena_type"": ""map"", ""arena"": ""beyondreality"" }, { ""display_name"": ""Black Cathedral"", ""arena_type"": ""map"", ""arena"": ""blackcathedral"" }, { ""display_name"": ""Blood Run"", ""arena_type"": ""map"", ""arena"": ""bloodrun"" }, { ""display_name"": ""Bloodlust"", ""arena_type"": ""map"", ""arena"": ""bloodlust"" }, { ""display_name"": ""Brimstone Abbey"", ""arena_type"": ""map"", ""arena"": ""brimstoneabbey"" }, { ""display_name"": ""Camper Crossings"", ""arena_type"": ""map"", ""arena"": ""campercrossings"" }, { ""display_name"": ""Campgrounds"", ""arena_type"": ""map"", ""arena"": ""campgrounds"" }, { ""display_name"": ""Canned Heat"", ""arena_type"": ""map"", ""arena"": ""cannedheat"" }, { ""display_name"": ""Chemical Reaction"", ""arena_type"": ""map"", ""arena"": ""chemicalreaction"" }, { ""display_name"": ""Cliffside"", ""arena_type"": ""map"", ""arena"": ""cliffside"" }, { ""display_name"": ""Cobalt Station"", ""arena_type"": ""map"", ""arena"": ""cobaltstation"" }, { ""display_name"": ""Cold Cathode"", ""arena_type"": ""map"", ""arena"": ""coldcathode"" }, { ""display_name"": ""Cold War"", ""arena_type"": ""map"", ""arena"": ""coldwar"" }, { ""display_name"": ""Concrete Palace"", ""arena_type"": ""map"", ""arena"": ""concretepalace"" }, { ""display_name"": ""Corrosion"", ""arena_type"": ""map"", ""arena"": ""corrosion"" }, { ""display_name"": ""Courtyard"", ""arena_type"": ""map"", ""arena"": ""courtyard"" }, { ""display_name"": ""Cure"", ""arena_type"": ""map"", ""arena"": ""cure"" }, { ""display_name"": ""Deep Inside"", ""arena_type"": ""map"", ""arena"": ""deepinside"" }, { ""display_name"": ""Delirium"", ""arena_type"": ""map"", ""arena"": ""delirium"" }, { ""display_name"": ""Demon Keep"", ""arena_type"": ""map"", ""arena"": ""demonkeep"" }, { ""display_name"": ""Devilish"", ""arena_type"": ""map"", ""arena"": ""devilish"" }, { ""display_name"": ""Dies Irae"", ""arena_type"": ""map"", ""arena"": ""diesirae"" }, { ""display_name"": ""Dismemberment"", ""arena_type"": ""map"", ""arena"": ""dismemberment"" }, { ""display_name"": ""Distant Screams"", ""arena_type"": ""map"", ""arena"": ""distantscreams"" }, { ""display_name"": ""Double Impact"", ""arena_type"": ""map"", ""arena"": ""doubleimpact"" }, { ""display_name"": ""Dreadful Place"", ""arena_type"": ""map"", ""arena"": ""dreadfulplace"" }, { ""display_name"": ""Dredwerkz"", ""arena_type"": ""map"", ""arena"": ""dredwerkz"" }, { ""display_name"": ""Dueling Keeps"", ""arena_type"": ""map"", ""arena"": ""duelingkeeps"" }, { ""display_name"": ""Electric Head"", ""arena_type"": ""map"", ""arena"": ""electrichead"" }, { ""display_name"": ""Eviscerated"", ""arena_type"": ""map"", ""arena"": ""eviscerated"" }, { ""display_name"": ""Evolution"", ""arena_type"": ""map"", ""arena"": ""evolution"" }, { ""display_name"": ""Eye to Eye"", ""arena_type"": ""map"", ""arena"": ""eyetoeye"" }, { ""display_name"": ""Fallout Bunker"", ""arena_type"": ""map"", ""arena"": ""falloutbunker"" }, { ""display_name"": ""Fatal Instinct"", ""arena_type"": ""map"", ""arena"": ""fatalinstinct"" }, { ""display_name"": ""Finnegan's"", ""arena_type"": ""map"", ""arena"": ""finnegans"" }, { ""display_name"": ""Fluorescent"", ""arena_type"": ""map"", ""arena"": ""fluorescent"" }, { ""display_name"": ""Focal Point"", ""arena_type"": ""map"", ""arena"": ""focalpoint"" }, { ""display_name"": ""Foolish Legacy"", ""arena_type"": ""map"", ""arena"": ""foolishlegacy"" }, { ""display_name"": ""Forgotten"", ""arena_type"": ""map"", ""arena"": ""forgotten"" }, { ""display_name"": ""Furious Heights"", ""arena_type"": ""map"", ""arena"": ""furiousheights"" }, { ""display_name"": ""Fuse"", ""arena_type"": ""map"", ""arena"": ""fuse"" }, { ""display_name"": ""Future Crossings"", ""arena_type"": ""map"", ""arena"": ""futurecrossings"" }, { ""display_name"": ""Golgotha Core"", ""arena_type"": ""map"", ""arena"": ""golgothacore"" }, { ""display_name"": ""Gospel Crossings"", ""arena_type"": ""map"", ""arena"": ""gospelcrossings"" }, { ""display_name"": ""Gothic Rage"", ""arena_type"": ""map"", ""arena"": ""gothicrage"" }, { ""display_name"": ""Grim Dungeons"", ""arena_type"": ""map"", ""arena"": ""grimdungeons"" }, { ""display_name"": ""Hearth"", ""arena_type"": ""map"", ""arena"": ""hearth"" }, { ""display_name"": ""Hektik"", ""arena_type"": ""map"", ""arena"": ""hektik"" }, { ""display_name"": ""Hell's Gate"", ""arena_type"": ""map"", ""arena"": ""hellsgate"" }, { ""display_name"": ""Hell's Gate Redux"", ""arena_type"": ""map"", ""arena"": ""hellsgateredux"" }, { ""display_name"": ""Hero's Keep"", ""arena_type"": ""map"", ""arena"": ""heroskeep"" }, { ""display_name"": ""Hidden Fortress"", ""arena_type"": ""map"", ""arena"": ""hiddenfortress"" }, { ""display_name"": ""House of Decay"", ""arena_type"": ""map"", ""arena"": ""houseofdecay"" }, { ""display_name"": ""Industrial Accident"", ""arena_type"": ""map"", ""arena"": ""industrialaccident"" }, { ""display_name"": ""Infinity"", ""arena_type"": ""map"", ""arena"": ""infinity"" }, { ""display_name"": ""Inner Sanctums"", ""arena_type"": ""map"", ""arena"": ""innersanctums"" }, { ""display_name"": ""Intervention"", ""arena_type"": ""map"", ""arena"": ""intervention"" }, { ""display_name"": ""IronWorks"", ""arena_type"": ""map"", ""arena"": ""ironworks"" }, { ""display_name"": ""Japanese Castles"", ""arena_type"": ""map"", ""arena"": ""japanesecastles"" }, { ""display_name"": ""Jumpwerkz"", ""arena_type"": ""map"", ""arena"": ""jumpwerkz"" }, { ""display_name"": ""Left Behind"", ""arena_type"": ""map"", ""arena"": ""leftbehind"" }, { ""display_name"": ""Leviathan"", ""arena_type"": ""map"", ""arena"": ""leviathan"" }, { ""display_name"": ""Limbus"", ""arena_type"": ""map"", ""arena"": ""limbus"" }, { ""display_name"": ""Longest Yard"", ""arena_type"": ""map"", ""arena"": ""longestyard"" }, { ""display_name"": ""Lost World"", ""arena_type"": ""map"", ""arena"": ""lostworld"" }, { ""display_name"": ""Nameless Place"", ""arena_type"": ""map"", ""arena"": ""namelessplace"" }, { ""display_name"": ""Overkill"", ""arena_type"": ""map"", ""arena"": ""overkill"" }, { ""display_name"": ""Overlord"", ""arena_type"": ""map"", ""arena"": ""overlord"" }, { ""display_name"": ""Phrantic"", ""arena_type"": ""map"", ""arena"": ""phrantic"" }, { ""display_name"": ""Pillbox"", ""arena_type"": ""map"", ""arena"": ""pillbox"" }, { ""display_name"": ""Power Station"", ""arena_type"": ""map"", ""arena"": ""powerstation"" }, { ""display_name"": ""Proving Grounds"", ""arena_type"": ""map"", ""arena"": ""provinggrounds"" }, { ""display_name"": ""Purgatory"", ""arena_type"": ""map"", ""arena"": ""purgatory"" }, { ""display_name"": ""Quarantine"", ""arena_type"": ""map"", ""arena"": ""quarantine"" }, { ""display_name"": ""Railyard"", ""arena_type"": ""map"", ""arena"": ""railyard"" }, { ""display_name"": ""Realm of Steel Rats"", ""arena_type"": ""map"", ""arena"": ""realmofsteelrats"" }, { ""display_name"": ""Rebound"", ""arena_type"": ""map"", ""arena"": ""rebound"" }, { ""display_name"": ""Reflux"", ""arena_type"": ""map"", ""arena"": ""reflux"" }, { ""display_name"": ""Repent"", ""arena_type"": ""map"", ""arena"": ""repent"" }, { ""display_name"": ""Retribution"", ""arena_type"": ""map"", ""arena"": ""retribution"" }, { ""display_name"": ""Revolver"", ""arena_type"": ""map"", ""arena"": ""revolver"" }, { ""display_name"": ""Sacellum"", ""arena_type"": ""map"", ""arena"": ""sacellum"" }, { ""display_name"": ""Scornforge"", ""arena_type"": ""map"", ""arena"": ""scornforge"" }, { ""display_name"": ""Seams and Bolts"", ""arena_type"": ""map"", ""arena"": ""seamsandbolts"" }, { ""display_name"": ""Shining Forces"", ""arena_type"": ""map"", ""arena"": ""shiningforces"" }, { ""display_name"": ""Siberia"", ""arena_type"": ""map"", ""arena"": ""siberia"" }, { ""display_name"": ""Silence"", ""arena_type"": ""map"", ""arena"": ""silence"" }, { ""display_name"": ""Silent Fright"", ""arena_type"": ""map"", ""arena"": ""silentfright"" }, { ""display_name"": ""Sinister"", ""arena_type"": ""map"", ""arena"": ""sinister"" }, { ""display_name"": ""Skyward"", ""arena_type"": ""map"", ""arena"": ""skyward"" }, { ""display_name"": ""Solid"", ""arena_type"": ""map"", ""arena"": ""solid"" }, { ""display_name"": ""Somewhat Damaged"", ""arena_type"": ""map"", ""arena"": ""somewhatdamaged"" }, { ""display_name"": ""Sorrow"", ""arena_type"": ""map"", ""arena"": ""sorrow"" }, { ""display_name"": ""Space Camp"", ""arena_type"": ""map"", ""arena"": ""spacecamp"" }, { ""display_name"": ""Space Chamber"", ""arena_type"": ""map"", ""arena"": ""spacechamber"" }, { ""display_name"": ""Space CTF"", ""arena_type"": ""map"", ""arena"": ""spacectf"" }, { ""display_name"": ""Spider Crossings"", ""arena_type"": ""map"", ""arena"": ""spidercrossings"" }, { ""display_name"": ""Spillway"", ""arena_type"": ""map"", ""arena"": ""spillway"" }, { ""display_name"": ""Stonekeep"", ""arena_type"": ""map"", ""arena"": ""stonekeep"" }, { ""display_name"": ""Stronghold"", ""arena_type"": ""map"", ""arena"": ""stronghold"" }, { ""display_name"": ""Terminal Heights"", ""arena_type"": ""map"", ""arena"": ""terminalheights"" }, { ""display_name"": ""Terminatria"", ""arena_type"": ""map"", ""arena"": ""terminatria"" }, { ""display_name"": ""Terminus"", ""arena_type"": ""map"", ""arena"": ""terminus"" }, { ""display_name"": ""The Edge"", ""arena_type"": ""map"", ""arena"": ""theedge"" }, { ""display_name"": ""Theatre of Pain"", ""arena_type"": ""map"", ""arena"": ""theatreofpain"" }, { ""display_name"": ""Three Story"", ""arena_type"": ""map"", ""arena"": ""threestory"" }, { ""display_name"": ""Thunderstruck"", ""arena_type"": ""map"", ""arena"": ""thunderstruck"" }, { ""display_name"": ""Tornado"", ""arena_type"": ""map"", ""arena"": ""tornado"" }, { ""display_name"": ""Toxicity"", ""arena_type"": ""map"", ""arena"": ""toxicity"" }, { ""display_name"": ""Trinity"", ""arena_type"": ""map"", ""arena"": ""trinity"" }, { ""display_name"": ""Troubled Waters"", ""arena_type"": ""map"", ""arena"": ""troubledwaters"" }, { ""display_name"": ""Use and Abuse"", ""arena_type"": ""map"", ""arena"": ""useandabuse"" }, { ""display_name"": ""Vertical Vengeance"", ""arena_type"": ""map"", ""arena"": ""verticalvengeance"" }, { ""display_name"": ""Vortex Portal"", ""arena_type"": ""map"", ""arena"": ""vortexportal"" }, { ""display_name"": ""Warehouse"", ""arena_type"": ""map"", ""arena"": ""warehouse"" }, { ""display_name"": ""Wargrounds"", ""arena_type"": ""map"", ""arena"": ""wargrounds"" }, { ""display_name"": ""Wicked"", ""arena_type"": ""map"", ""arena"": ""wicked"" }, { ""display_name"": ""Window Pain"", ""arena_type"": ""map"", ""arena"": ""windowpain"" }, { ""display_name"": ""Windsong Keep"", ""arena_type"": ""map"", ""arena"": ""windsongkeep"" } ], ""difficulty"": [ { ""display_name"": ""Any Difficulty"", ""difficulty"": ""any"" }, { ""display_name"": ""Unrestricted"", ""difficulty"": ""-1"" }, { ""display_name"": ""Skill Matched"", ""difficulty"": ""1"" }, { ""display_name"": ""More Challenging"", ""difficulty"": ""2"" }, { ""display_name"": ""Very Difficult"", ""difficulty"": ""3"" } ], ""gamestate"": [ { ""display_name"": ""Any Game State"", ""state"": ""any"" }, { ""display_name"": ""Only Games With Players"", ""state"": ""POPULATED"" }, { ""display_name"": ""Pre-Game Warmup"", ""state"": ""PRE_GAME"" }, { ""display_name"": ""Games In Progress"", ""state"": ""IN_PROGRESS"" }, { ""display_name"": ""Waiting for Players (Empty)"", ""state"": ""EMPTY"" } ], ""game_types"": [ { ""display_name"": ""All Game Types"", ""game_type"": ""any"" }, { ""display_name"": ""Any Team Game"", ""game_type"": ""1"" }, { ""display_name"": ""Any Ranked Game"", ""game_type"": ""12"" }, { ""display_name"": ""Any Unranked Game"", ""game_type"": ""13"" }, { ""display_name"": ""Free For All"", ""game_type"": ""2"" }, { ""display_name"": ""Capture the Flag"", ""game_type"": ""3"" }, { ""display_name"": ""Clan Arena"", ""game_type"": ""4"" }, { ""display_name"": ""Freeze Tag"", ""game_type"": ""5"" }, { ""display_name"": ""Team Death Match"", ""game_type"": ""6"" }, { ""display_name"": ""Duel"", ""game_type"": ""7"" }, { ""display_name"": ""Instagib FFA"", ""game_type"": ""8"" }, { ""display_name"": ""Insta CTF"", ""game_type"": ""9"" }, { ""display_name"": ""Insta Freeze"", ""game_type"": ""10"" }, { ""display_name"": ""Insta TDM"", ""game_type"": ""11"" }, { ""display_name"": ""Insta Clan Arena"", ""game_type"": ""14"" }, { ""display_name"": ""Domination"", ""game_type"": ""15"" }, { ""display_name"": ""FCTF"", ""game_type"": ""16"" }, { ""display_name"": ""Harvester"", ""game_type"": ""17"" }, { ""display_name"": ""Attack and Defend"", ""game_type"": ""18"" }, { ""display_name"": ""Red Rover"", ""game_type"": ""19"" }, { ""display_name"": ""Insta Domination"", ""game_type"": ""20"" }, { ""display_name"": ""Insta 1-Flag CTF"", ""game_type"": ""21"" }, { ""display_name"": ""Insta Harvester"", ""game_type"": ""22"" }, { ""display_name"": ""Insta Attack and Defend"", ""game_type"": ""23"" }, { ""display_name"": ""Insta Red Rover"", ""game_type"": ""24"" }, { ""display_name"": ""Race"", ""game_type"": ""25"" } ], ""game_info"": [ { ""type"": ""any"", ""ig"": ""any"", ""gtarr"": [ 5, 4, 3, 0, 1, 9, 10, 11, 8, 6 ], ""ranked"": ""any"" }, { ""type"": ""1"", ""ig"": 0, ""gtarr"": [ 5, 4, 3, 9, 10, 11, 8, 6 ], ""ranked"": ""any"" }, { ""type"": ""2"", ""ig"": 0, ""gtarr"": [ 0 ], ""ranked"": ""any"" }, { ""type"": ""3"", ""ig"": 0, ""gtarr"": [ 5 ], ""ranked"": ""any"" }, { ""type"": ""4"", ""ig"": 0, ""gtarr"": [ 4 ], ""ranked"": ""any"" }, { ""type"": ""5"", ""ig"": 0, ""gtarr"": [ 9 ], ""ranked"": ""any"" }, { ""type"": ""6"", ""ig"": 0, ""gtarr"": [ 3 ], ""ranked"": ""any"" }, { ""type"": ""7"", ""ig"": 0, ""gtarr"": [ 1 ], ""ranked"": ""any"" }, { ""type"": ""8"", ""ig"": 1, ""gtarr"": [ 0 ], ""ranked"": ""any"" }, { ""type"": ""9"", ""ig"": 1, ""gtarr"": [ 5 ], ""ranked"": ""any"" }, { ""type"": ""10"", ""ig"": 10, ""gtarr"": [ 9 ], ""ranked"": ""any"" }, { ""type"": ""11"", ""ig"": 1, ""gtarr"": [ 3 ], ""ranked"": ""any"" }, { ""type"": ""12"", ""ig"": ""any"", ""gtarr"": [ 5, 4, 3, 9, 0, 1, 10, 11, 8, 6, 12 ], ""ranked"": 1 }, { ""type"": ""13"", ""ig"": ""any"", ""gtarr"": [ 5, 4, 3, 9, 0, 1, 10, 11, 8, 6, 12 ], ""ranked"": 0 }, { ""type"": ""14"", ""ig"": 1, ""gtarr"": [ 4 ], ""ranked"": 0 }, { ""type"": ""15"", ""ig"": 0, ""gtarr"": [ 10 ], ""ranked"": ""any"" }, { ""type"": ""16"", ""ig"": 0, ""gtarr"": [ 6 ], ""ranked"": ""any"" }, { ""type"": ""17"", ""ig"": 0, ""gtarr"": [ 8 ], ""ranked"": ""any"" }, { ""type"": ""18"", ""ig"": 0, ""gtarr"": [ 11 ], ""ranked"": ""any"" }, { ""type"": ""19"", ""ig"": 0, ""gtarr"": [ 12 ], ""ranked"": ""any"" }, { ""type"": ""20"", ""ig"": 1, ""gtarr"": [ 10 ], ""ranked"": ""any"" }, { ""type"": ""21"", ""ig"": 1, ""gtarr"": [ 6 ], ""ranked"": ""any"" }, { ""type"": ""22"", ""ig"": 2, ""gtarr"": [ 8 ], ""ranked"": ""any"" }, { ""type"": ""23"", ""ig"": 3, ""gtarr"": [ 11 ], ""ranked"": ""any"" }, { ""type"": ""24"", ""ig"": 4, ""gtarr"": [ 12 ], ""ranked"": ""any"" }, { ""type"": ""25"", ""ig"": 0, ""gtarr"": [ 2 ], ""ranked"": ""any"" }, { ""type"": ""any"", ""ig"": ""any"", ""gtarr"": [ 5, 4, 3, 0, 1, 9, 10, 11, 8, 6 ], ""ranked"": ""any"" } ] }";

            using (FileStream fs = File.Create(UQLTGlobals.CurrentFilterPath))
            using (TextWriter writer = new StreamWriter(fs))
            {
                writer.WriteLine(filterbackup);
            }

            Console.WriteLine("Fail-safe filters restored.");
        }
    }
}
