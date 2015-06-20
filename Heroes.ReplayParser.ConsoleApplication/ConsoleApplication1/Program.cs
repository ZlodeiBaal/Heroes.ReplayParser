using System.IO;
using MpqLib.Mpq;
using System;
using System.Linq;
using Heroes.ReplayParser;
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {
        public class tryam
        {
            public Point P;
            public TimeSpan ts;
            public tryam(Point p, TimeSpan t)
            {
                P = p;
                ts = t;
            }
        }
        static void Main(string[] args)
        {
            var heroesAccountsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Heroes of the Storm\Accounts");
            var randomReplayFileName = Directory.GetFiles(heroesAccountsFolder, "*.StormReplay", SearchOption.AllDirectories).OrderBy(i => Guid.NewGuid()).First();

            // Use temp directory for MpqLib directory permissions requirements
            var tmpPath = Path.GetTempFileName();
            File.Copy(randomReplayFileName, tmpPath, true);

            try
            {
                // Create our Replay object: this object will be filled as you parse the different files in the .StormReplay archive
                var replay = new Replay();
                MpqHeader.ParseHeader(replay, tmpPath);
                List<Unit.HeroPosList> u;
                using (var archive = new CArchive(tmpPath))
                {
                    ReplayInitData.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.initData"));
                    ReplayDetails.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.details"));
                    ReplayTrackerEvents.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.tracker.events"));
                    ReplayAttributeEvents.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.attributes.events"));
                    if (replay.ReplayBuild >= 32455)
                        ReplayGameEvents.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.game.events"));
                    ReplayServerBattlelobby.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.server.battlelobby"));
                    u= Unit.ParseUnitData(replay);
                }
          //     TrackerEventStructure x = new TrackerEventStructure();
               
         //      long y = 0;
         //      long z = 0;
                //heroCCmdEventList
                
                List<tryam> t = new List<tryam>();
                List<tryam> t2 = new List<tryam>();
                for (int i = 0; i < replay.GameEvents.Count; i++)
                {
                    if (replay.GameEvents[i].eventType == GameEventType.CCmdUpdateTargetUnitEvent)
                    {
                        if (replay.GameEvents[i].player!=null)
                            if (replay.GameEvents[i].player.Name == "Wormkeeper")
                            {
                                t.Add(new tryam(Point.FromEventFormat(
                            replay.GameEvents[i].data.array[6].array[0].unsignedInt.Value,
                            replay.GameEvents[i].data.array[6].array[1].unsignedInt.Value), replay.GameEvents[i].TimeSpan));
                            }
                    }
                    else if (replay.GameEvents[i].eventType == Heroes.ReplayParser.GameEventType.CCameraUpdateEvent)

                            if (replay.GameEvents[i].player != null)
                                if (replay.GameEvents[i].player.Name == "Wormkeeper")
                                {
                                    if (replay.GameEvents[i].data.array[0]!=null)
                                    t2.Add(new tryam(Point.FromEventFormat(
                                replay.GameEvents[i].data.array[0].array[0].unsignedInt.Value,
                                replay.GameEvents[i].data.array[0].array[1].unsignedInt.Value), replay.GameEvents[i].TimeSpan));
                                }
                   
                    /*if (replay.GameEvents[i].player!=null)
                    if (replay.GameEvents[i].player.Name=="Wormkeeper")
                    {
                        if (replay.GameEvents[i].TimeSpan.TotalSeconds>10)
                        if (replay.GameEvents[i].data.array!=null)
                        if (replay.GameEvents[i].data.array.Length==3)
                        {
                            x.vInt = replay.GameEvents[i].data.array[0].vInt;
                        }
                    }*/
                }
                    // Our Replay object now has all currently available information
                    Console.WriteLine("Replay Build: " + replay.ReplayBuild);
                
                Console.WriteLine("Map: " + replay.Map);
                foreach (var player in replay.Players.OrderByDescending(i => i.IsWinner))
                    Console.WriteLine("Player: " + player.Name + ", Win: " + player.IsWinner + ", Hero: " + player.Character + ", Lvl: " + player.CharacterLevel + (replay.ReplayBuild >= 32524 ? ", Talents: " + string.Join(",", player.Talents.OrderBy(i => i)) : ""));

                Console.WriteLine("Press Any Key to Close");
                Console.Read();
            }
            finally
            {
                if (File.Exists(tmpPath))
                    File.Delete(tmpPath);
            }
        }

        private static byte[] GetMpqArchiveFileBytes(CArchive archive, string archivedFileName)
        {
            var buffer = new byte[archive.FindFiles(archivedFileName).Single().Size];
            archive.ExportFile(archivedFileName, buffer);
            return buffer;
        }
    }
}
