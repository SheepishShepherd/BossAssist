
namespace BossAssist
{
	internal enum MessageType : byte
	{
		SendRecordsToServer,
		RecordUpdate,
		DeathCount
	}

	internal enum RecordID : int
	{
		None,
		Kills,
		Deaths,
		ShortestFightTime,
		LongestFightTime,
		DodgeTime,
		MostHits,
		LeastHits,
		BestBrink,
		BestBrinkPercent,
		WorstBrink,
		WorstBrinkPercent,

		LastFightTime,
		LastDodgeTime,
		LastHits,
		LastBrink,
		LastBrinkPercent
	}
}
