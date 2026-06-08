import { Schema, MapSchema, type } from "@colyseus/schema";

export class FPSPlayer extends Schema {
    @type("float32") x: number = 0;
    @type("float32") y: number = 0;
    @type("float32") z: number = 0;
    @type("float32") rotY: number = 0;
    @type("boolean") isWalking = false;
    @type("boolean") isSprinting = false;
    @type("boolean") isReloading = false;
    @type("float32") health: number = 100;
    @type("float32") maxHealth: number = 100;
    @type("string") skin: string = "default";
    @type("string") currentWeaponId: string = "ak47";
}

export class MyRoomState extends Schema {
    @type({ map: FPSPlayer }) players = new MapSchema<FPSPlayer>();
    @type("number") gameStartTime: number = 0;
    @type("boolean") isGameActive: boolean = false;
    @type("float32") timeRemaining: number = 300;
}