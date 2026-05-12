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

public partial class FPSPlayer : Schema {
#if UNITY_5_3_OR_NEWER
[Preserve]
#endif
public FPSPlayer() { }
	[Type(0, "float32")]
	public float x = default(float);

	[Type(1, "float32")]
	public float y = default(float);

	[Type(2, "float32")]
	public float z = default(float);

	[Type(3, "float32")]
	public float rotY = default(float);

	[Type(4, "boolean")]
	public bool isWalking = default(bool);

	[Type(5, "float32")]
	public float health = default(float);

	[Type(6, "float32")]
	public float maxHealth = default(float);

	[Type(7, "string")]
	public string skin = default(string);
}

