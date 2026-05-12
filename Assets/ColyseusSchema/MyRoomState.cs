// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 4.0.20
// 

using Colyseus.Schema;
#if UNITY_5_3_OR_NEWER
using UnityEngine.Scripting;
#endif

public partial class MyRoomState : Schema {
#if UNITY_5_3_OR_NEWER
[Preserve]
#endif
public MyRoomState() { }
	[Type(0, "map", typeof(MapSchema<FPSPlayer>))]
	public MapSchema<FPSPlayer> players = null;

	[Type(1, "number")]
	public float gameStartTime = default(float);

	[Type(2, "boolean")]
	public bool isGameActive = default(bool);

	[Type(3, "float32")]
	public float timeRemaining = default(float);
}

