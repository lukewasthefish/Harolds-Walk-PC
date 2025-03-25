using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Holds hard-coded information about which times are required for which medals in every level.
/// </summary> <summary>
/// 
/// </summary>
public partial class TimeAttackMedalRecords : Node
{
	public Dictionary<string, TimeRecord> records = new Dictionary<string, TimeRecord>();

	//TODO : Deal with the case where there is not anything for a queried level
	public TimeAttackMedalRecords()
	{
		records["afterparty"] = new TimeRecord(45, 38, 33);
		records["binary_01"] = new TimeRecord(20, 16, 12);
		records["binary_011"] = new TimeRecord(60, 50, 40); //Remember this won't match with the in-game name here
		records["binary_100"] = new TimeRecord(42, 36, 30);
		records["binary_11"] = new TimeRecord(50, 42, 34);
		records["bridges"] = new TimeRecord(30, 24, 16);
		records["castle"] = new TimeRecord(16, 12, 7);

		records["challenge_stage_1"] = new TimeRecord(7, 5, 3);
		records["challenge_stage_2"] = new TimeRecord(32, 25, 19);
		records["challenge_stage_3"] = new TimeRecord(32, 27, 23);
		records["challenge_stage_4"] = new TimeRecord(40, 32, 24);
		records["challenge_stage_5"] = new TimeRecord(15, 11, 9);
		records["challenge_stage_6"] = new TimeRecord(40, 34, 29);
		records["challenge_stage_7"] = new TimeRecord(42, 32, 25);
		records["challenge_stage_8"] = new TimeRecord(15, 11, 8);

		records["debug_room"] = new TimeRecord(12, 9, 6);

		records["desert"] = new TimeRecord(18, 15, 9);

		records["diamond_mountain"] = new TimeRecord(20, 16, 11);

		records["escape"] = new TimeRecord(8, 6, 4);

		records["fly_swatter"] = new TimeRecord(20, 16, 13);

		records["gas_station"] = new TimeRecord(20, 17, 14);

		records["harold_statue"] = new TimeRecord(32, 27, 22);

		records["inverse_gas_station"] = new TimeRecord(14, 11, 8);

		records["Islands"] = new TimeRecord(22, 17, 13);

		records["laptops"] = new TimeRecord(26, 20, 17);

		records["pineapple"] = new TimeRecord(25, 22, 17);

		records["pink_world"] = new TimeRecord(4, 3, 2);

		records["pink_world_2"] = new TimeRecord(27, 22, 18);

		records["spheres"] = new TimeRecord(18, 15, 13);

		records["stairs"] = new TimeRecord(32, 25, 20);

		records["WaterTower"] = new TimeRecord(16, 12, 8);

		records["wheel_of_eight"] = new TimeRecord(8, 6, 4);
	}

	public struct TimeRecord
	{
		//🥉
		public double bronze = 260;
		
		//🥈
		public double silver = 220;

		//🥇
		public double gold = 190;

		public TimeRecord(double bronze, double silver, double gold)
		{
			this.bronze = bronze;
			this.silver = silver;
			this.gold = gold;
		}

        public override string ToString()
        {
			return $"B:{this.bronze} S:{this.silver} G:{this.gold}";
        }
    }
}
