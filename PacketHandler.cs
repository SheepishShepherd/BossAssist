using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossAssist
{
	internal abstract class PacketHandler
	{
		internal byte HandlerType { get; set; }

		public abstract void HandlePacket(BinaryReader reader, int fromWho);

		protected PacketHandler(byte handlerType)
		{
			HandlerType = handlerType;
		}

		protected ModPacket GetPacket(byte packetType, int fromWho)
		{
			var packet = BossAssist.instance.GetPacket();
			packet.Write(HandlerType);
			packet.Write(packetType);
			if (Main.netMode == NetmodeID.Server)
			{
				packet.Write((byte)fromWho);
			}
			return packet;
		}
	}

	internal class BossRecordPacketHandler : PacketHandler
	{
		public const byte SyncTarget = 1;

		public BossRecordPacketHandler(byte handlerType) : base(handlerType)
		{

		}

		public override void HandlePacket(BinaryReader reader, int fromWho)
		{
			switch (reader.ReadByte())
			{
				case (SyncTarget):
					Receive(reader, fromWho);
					break;
			}
		}

		public void Send(int toWho, int fromWho, int npc, string type, int value)
		{
			ModPacket packet = GetPacket(SyncTarget, fromWho);
			packet.Write(npc);
			packet.Write(type);
			packet.Write(value);
			packet.Send(toWho, fromWho);
		}

		public void Receive(BinaryReader reader, int fromWho)
		{
			int npc = reader.ReadInt32();
			string recordtype = reader.ReadString();
			int value = reader.ReadInt32();
			if (Main.netMode == NetmodeID.Server)
			{
				Send(-1, fromWho, npc, recordtype, value);
			}
			else
			{
				NPC boss = Main.npc[npc];
				Main.player[fromWho].GetModPlayer<PlayerAssist>().AllBossRecords[NPCAssist.SpecialBossCheck(boss)].stat.kills++;
			}
		}
	}

	internal class ModNetHandler
	{
		public const byte RecordType = 1;
		internal static BossRecordPacketHandler newRecord = new BossRecordPacketHandler(RecordType);

		public static void HandlePacket(BinaryReader reader, int fromWho)
		{
			BossAssist.MessageType msgType = (BossAssist.MessageType)reader.ReadByte();
			switch (msgType)
			{
				case BossAssist.MessageType.SyncPlayer:
					byte plrnum = reader.ReadByte();
					PlayerAssist player = Main.player[plrnum].GetModPlayer<PlayerAssist>();
					///List<BossRecord> exampleLifeFruits = reader.Read();
					///player.exampleLifeFruits = exampleLifeFruits;
					///player.nonStopParty = reader.ReadBoolean();
					// SyncPlayer will be called automatically, so there is no need to forward this data to other clients.
					break;
				case BossAssist.MessageType.RecordUpdate:
					plrnum = reader.ReadByte();
					player = Main.player[plrnum].GetModPlayer<PlayerAssist>();
					///player.nonStopParty = reader.ReadBoolean();
					// Unlike SyncPlayer, here we have to relay/forward these changes to all other connected clients
					if (Main.netMode == NetmodeID.Server)
					{
						/*
						var packet = GetPacket();
						packet.Write((byte)BossAssist.MessageType.RecordUpdate);
						packet.Write(plrnum);
						packet.Write(player.nonStopParty);
						packet.Send(-1, plrnum);
						*/
					}
					break;
				case BossAssist.MessageType.NewRecord:
					
					newRecord.HandlePacket(reader, fromWho);
					break;
			}
		}
	}
}
